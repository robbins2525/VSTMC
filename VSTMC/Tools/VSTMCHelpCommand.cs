using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using Task = System.Threading.Tasks.Task;
using VSTMC.Utilities;

namespace VSTMC.Tools
{
    internal sealed class VSTMCHelpCommand
    {
        private const bool UseHtmlHelp = true;

        public const int CommandId = 4194;

        public static readonly Guid CommandSet = new Guid(PackageGuids.guidPackageCmdSetString);

        private readonly AsyncPackage package;

        private VSTMCHelpCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            CommandID menuCommandID = new CommandID(CommandSet, CommandId);
            MenuCommand menuItem = new MenuCommand(Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        private static VSTMCHelpCommand? _instance;
        public static VSTMCHelpCommand Instance => _instance ?? throw new InvalidOperationException("Not initialized.");

        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider => package;

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            object? svc = await package.GetServiceAsync(typeof(IMenuCommandService));
            if (svc is not OleMenuCommandService commandService)
            {
                ActivityLog.LogError("VSTMC", "OleMenuCommandService not available for VSTMCHelpCommand.");
                return;
            }

            _instance = new VSTMCHelpCommand(package, commandService);
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                string helpFolder = Path.Combine(VsUtilities.GetExtensionAssemblyPath, "Help");
                string htmlPath = Path.Combine(helpFolder, "index.html");
                string chmPath = Path.Combine(helpFolder, "VSToolsForMicroStationCONNECTEdition.chm");

                if (UseHtmlHelp && File.Exists(htmlPath))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = htmlPath,
                        UseShellExecute = true
                    });
                    return;
                }

                if (File.Exists(chmPath))
                {
                    System.Windows.Forms.Help.ShowHelp(new System.Windows.Forms.TextBox(), chmPath);
                    return;
                }

                System.Windows.Forms.MessageBox.Show(
                    "Cannot find help file.",
                    "Help",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Exclamation);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    $"Cannot open help file.\n\n{ex.Message}",
                    "Help",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Exclamation);
            }
        }
    }
}