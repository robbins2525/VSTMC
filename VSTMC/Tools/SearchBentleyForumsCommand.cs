using System;
using System.ComponentModel.Design;
using System.Web;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;
using VSTMC.Utilities;

namespace VSTMC.Tools
{
    internal sealed class SearchBentleyForumsCommand
    {
        public const int CommandId = 4131;
        public static readonly Guid CommandSet = new Guid(PackageGuids.guidPackageCmdSetString);

        private readonly AsyncPackage package;

        private SearchBentleyForumsCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            _ = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new OleMenuCommand(this.Execute, menuCommandID);
            menuItem.BeforeQueryStatus += MenuItem_BeforeQueryStatus;
            commandService.AddCommand(menuItem);
        }

        // ---- Singleton (no CS8618) ----
        private static SearchBentleyForumsCommand? _instance;
        public static SearchBentleyForumsCommand Instance =>
            _instance ?? throw new InvalidOperationException("SearchBentleyForumsCommand not initialized.");

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            object? svc = await package.GetServiceAsync(typeof(IMenuCommandService));
            if (svc is not OleMenuCommandService commandService)
            {
                ActivityLog.LogError("VSTMC", "OleMenuCommandService not available for SearchBentleyForumsCommand.");
                return;
            }

            _instance = new SearchBentleyForumsCommand(package, commandService);
        }

        private void MenuItem_BeforeQueryStatus(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (sender is OleMenuCommand menuCommand)
            {
                menuCommand.Visible =
                    VsUtilities.IsBentleyProject &&
                    VsUtilities.IsActiveDocumentBentleyProject &&
                    VsUtilities.IsTextSelected;
            }
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var utilities = new VsUtilities();
            utilities.SearchBentleyForums();
        }
    }
}
