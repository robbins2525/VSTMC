using System;
using System.ComponentModel.Design;
using System.Threading;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;
using VSTMC.Utilities;

namespace VSTMC.Tools
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class NativeImportCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid(PackageGuids.guidPackageCmdSetString);

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        private NativeImportCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            this.package = package ?? throw new ArgumentNullException(nameof(package));
            _ = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static NativeImportCommand? Instance { get; private set; }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            // ✅ Null-safe service retrieval (fixes CS8600/CS8604)
            object? svc = await package.GetServiceAsync(typeof(IMenuCommandService));
            if (svc is not OleMenuCommandService commandService)
            {
                ActivityLog.LogError("VSTMC", "OleMenuCommandService not available for NativeImportCommand.");
                return;
            }

            Instance = new NativeImportCommand(package, commandService);
        }

        private void Execute(object sender, EventArgs e)
        {
            // fire-and-forget, but tracked correctly to satisfy analyzers
            this.package.JoinableTaskFactory.RunAsync(async () =>
            {
                try
                {
                    var utilities = new VsUtilities();
                    await utilities.ImportNativeAppsAsync(this.package, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    ActivityLog.LogError("VSTMC", ex.ToString());
                }
            }).FileAndForget("VSTMC/ImportNativeApps");
        }
    }
}
