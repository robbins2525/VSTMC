using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Task = System.Threading.Tasks.Task;

namespace VSTMC
{
    /// <summary>
    /// Dynamically populates the Bentley Help submenu with clickable CHM help files
    /// found under Bentley_SDKPath\Doc and Bentley_SDKPath\Documentation.
    /// </summary>
    internal sealed class BentleyHelpCommand
    {
        // Must match the DynamicItemStart placeholder command ID in the .vsct file
        private const int DynamicCommandIdBase = 4195;

        public static readonly Guid CommandSet = new Guid(PackageGuids.guidPackageCmdSetString);

        private readonly AsyncPackage package;
        private readonly OleMenuCommandService commandService;

        public static BentleyHelpCommand? Instance { get; private set; }

        private readonly List<string> helpFiles = new();

        private BentleyHelpCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            this.commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            ThreadHelper.ThrowIfNotOnUIThread();
            LoadHelpFiles();
            AddDynamicHelpCommands();
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            object? commandServiceObj = await package.GetServiceAsync(typeof(IMenuCommandService));
            if (commandServiceObj is not OleMenuCommandService commandService)
            {
                ActivityLog.LogError(nameof(BentleyHelpCommand), "OleMenuCommandService not available.");
                return;
            }

            Instance = new BentleyHelpCommand(package, commandService);
        }

        private void LoadHelpFiles()
        {
            helpFiles.Clear();

            string sdkPath = GetBentleySdkPath();
            if (string.IsNullOrWhiteSpace(sdkPath))
                return;

            HashSet<string> names = new(StringComparer.OrdinalIgnoreCase);

            string docDir = Path.Combine(sdkPath, "Doc");
            if (Directory.Exists(docDir))
            {
                foreach (string file in Directory.GetFiles(docDir, "*.chm"))
                {
                    string? name = Path.GetFileNameWithoutExtension(file);
                    if (!string.IsNullOrWhiteSpace(name))
                        names.Add(name);
                }
            }

            string documentationDir = Path.Combine(sdkPath, "Documentation");
            if (Directory.Exists(documentationDir))
            {
                foreach (string file in Directory.GetFiles(documentationDir, "*.chm"))
                {
                    string? name = Path.GetFileNameWithoutExtension(file);
                    if (!string.IsNullOrWhiteSpace(name))
                        names.Add(name);
                }
            }

            helpFiles.AddRange(names.OrderBy(n => n, StringComparer.CurrentCultureIgnoreCase));
        }

        private void AddDynamicHelpCommands()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (helpFiles.Count == 0)
                return;

            for (int i = 0; i < helpFiles.Count; i++)
            {
                string helpName = helpFiles[i];
                int commandId = DynamicCommandIdBase + i;

                CommandID menuCommandId = new(CommandSet, commandId);
                OleMenuCommand menuItem = new((sender, e) => OpenHelpFile(helpName), menuCommandId)
                {
                    Text = helpName
                };

                menuItem.BeforeQueryStatus += (sender, e) =>
                {
                    if (sender is OleMenuCommand cmd)
                    {
                        cmd.Visible = true;
                        cmd.Enabled = File.Exists(GetSelectedChmPath(helpName));
                        cmd.Text = helpName;
                    }
                };

                commandService.AddCommand(menuItem);
            }
        }

        private static string GetBentleySdkPath()
        {
            return Environment.GetEnvironmentVariable("Bentley_SDKPath", EnvironmentVariableTarget.Process) ?? string.Empty;
        }

        private static string GetSelectedChmPath(string helpName)
        {
            if (string.IsNullOrWhiteSpace(helpName))
                return string.Empty;

            string sdkPath = GetBentleySdkPath();
            if (string.IsNullOrWhiteSpace(sdkPath))
                return string.Empty;

            string docChm = Path.Combine(sdkPath, "Doc", helpName + ".chm");
            if (File.Exists(docChm))
                return docChm;

            string documentationChm = Path.Combine(sdkPath, "Documentation", helpName + ".chm");
            if (File.Exists(documentationChm))
                return documentationChm;

            return string.Empty;
        }

        private static void OpenHelpFile(string helpName)
        {
            string chmPath = GetSelectedChmPath(helpName);
            if (string.IsNullOrWhiteSpace(chmPath) || !File.Exists(chmPath))
                return;

            Help.ShowHelp(new Button(), chmPath);
        }
    }
}