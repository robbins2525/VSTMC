using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using System.IO;
using Task = System.Threading.Tasks.Task;
using VSTMC.Utilities;

namespace VSTMC.Tools
{
    internal sealed class OpenSDKFolderCommand
    {
        public const int CommandId = 4193;
        public static readonly Guid CommandSet = new Guid(PackageGuids.guidPackageCmdSetString);

        private readonly AsyncPackage package;

        private OpenSDKFolderCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            _ = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new OleMenuCommand(this.Execute, menuCommandID);
            menuItem.BeforeQueryStatus += MenuItem_BeforeQueryStatus;
            commandService.AddCommand(menuItem);
        }

        // ---- Singleton (no CS8618) ----
        private static OpenSDKFolderCommand? _instance;
        public static OpenSDKFolderCommand Instance =>
            _instance ?? throw new InvalidOperationException("OpenSDKFolderCommand not initialized.");

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            object? svc = await package.GetServiceAsync(typeof(IMenuCommandService));
            if (svc is not OleMenuCommandService commandService)
            {
                ActivityLog.LogError("VSTMC", "OleMenuCommandService not available for OpenSDKFolderCommand.");
                return;
            }

            _instance = new OpenSDKFolderCommand(package, commandService);
        }

        private void MenuItem_BeforeQueryStatus(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

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
                if (Directory.Exists(utilities.Bentley_SDKPath))
                {
                    utilities.OpenFolderLocation(utilities.Bentley_SDKPath);
                }
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show(
                    "Unable to Open Bentley SDK Folder. Verify that it exist.",
                    "Error Opening Folder",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);
            }
        }
    }
}
