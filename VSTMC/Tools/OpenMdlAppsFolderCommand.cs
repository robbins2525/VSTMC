using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using System.IO;
using Task = System.Threading.Tasks.Task;
using VSTMC.Utilities;

namespace VSTMC.Tools
{
    internal sealed class OpenMdlAppsFolderCommand
    {
        public const int CommandId = 4130;
        public static readonly Guid CommandSet = new Guid(PackageGuids.guidPackageCmdSetString);

        private readonly AsyncPackage package;

        private OpenMdlAppsFolderCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            _ = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new OleMenuCommand(this.Execute, menuCommandID);
            menuItem.BeforeQueryStatus += MenuItem_BeforeQueryStatus;
            commandService.AddCommand(menuItem);
        }

        // ---- Singleton (no CS8618) ----
        private static OpenMdlAppsFolderCommand? _instance;
        public static OpenMdlAppsFolderCommand Instance =>
            _instance ?? throw new InvalidOperationException("OpenMdlAppsFolderCommand not initialized.");

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            object? svc = await package.GetServiceAsync(typeof(IMenuCommandService));
            if (svc is not OleMenuCommandService commandService)
            {
                ActivityLog.LogError("VSTMC", "OleMenuCommandService not available for OpenMdlAppsFolderCommand.");
                return;
            }

            _instance = new OpenMdlAppsFolderCommand(package, commandService);
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
                if (Directory.Exists(utilities.Bentley_MDLPath))
                {
                    utilities.OpenFolderLocation(utilities.Bentley_MDLPath);
                }
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show(
                    "Unable to Open Bentley MDLAPPS Folder. Verify that it exist.",
                    "Error Opening Folder",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);
            }
        }
    }
}
