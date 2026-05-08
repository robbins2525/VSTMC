using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using System.IO;
using Task = System.Threading.Tasks.Task;
using VSTMC.Utilities;

namespace VSTMC.Tools
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class OpenFolderCommand
    {
        public const int CommandId = 4129;

        public static readonly Guid CommandSet = new Guid(PackageGuids.guidPackageCmdSetString);

        private readonly AsyncPackage package;

        private OpenFolderCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            _ = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new OleMenuCommand(this.Execute, menuCommandID);
            menuItem.BeforeQueryStatus += MenuItem_BeforeQueryStatus;
            commandService.AddCommand(menuItem);
        }

        //public static OpenFolderCommand? Instance { get; private set; }
        private static OpenFolderCommand? _instance;
        public static OpenFolderCommand Instance =>
            _instance ?? throw new InvalidOperationException("OpenFolderCommand not initialized.");

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            object? svc = await package.GetServiceAsync(typeof(IMenuCommandService));
            if (svc is not OleMenuCommandService commandService)
            {
                ActivityLog.LogError("VSTMC", "OleMenuCommandService not available for OpenFolderCommand.");
                return;
            }

            _instance = new OpenFolderCommand(package, commandService);
        }


        private void MenuItem_BeforeQueryStatus(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // ✅ Fix CS8600/CS8602 (safe pattern match)
            if (sender is OleMenuCommand menuCommand)
            {
                menuCommand.Visible = VsUtilities.IsBentleyProject;
            }
        }

        private void Execute(object sender, EventArgs e)
        {
            var utilities = new VsUtilities();

            try
            {
                // ✅ small safety: utilities.Bentley_AppPath could be null depending on your Utilities implementation
                var path = utilities.Bentley_AppPath;

                if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
                {
                    utilities.OpenFolderLocation(path);
                }
            }
            catch (Exception)
            {
                System.Windows.Forms.MessageBox.Show(
                    "Unable to Open Bentley Application Folder. Verify that it exist.",
                    "Error Opening Folder",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);
            }
        }
    }
}
