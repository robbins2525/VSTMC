using EnvDTE;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Task = System.Threading.Tasks.Task;
using VSTMC.Tools;
using VSTMC.Options;

namespace VSTMC
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "6.0.1.2", IconResourceID = 400)] // Info on this package for Help/About
    [Guid(PackageGuids.guidPackageString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideOptionPage(typeof(OptionsPage), "Bentley", "MicroStation CONNECT", 100, 102, true, new string[] { "Bentley Developer Network - Bentley CONNECT Edition" })]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionOpening_string, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class VSTMCPackage : AsyncPackage
    {
        private static OptionsPage? _options;

        public static OptionsPage Options =>
            _options ?? throw new InvalidOperationException("OptionsPage not initialized yet.");


        /// <summary>
        /// Initializes a new instance of the <see cref="VSTMCPackage"/> class.
        /// </summary>
        public VSTMCPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        #region Package Members

        internal static VSTMCPackage? Instance { get; private set; }


        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            _options = (OptionsPage)GetDialogPage(typeof(OptionsPage));

            await base.InitializeAsync(cancellationToken, progress);

            // await LoadPackageAsync(cancellationToken);

            await AddFilesAsync(cancellationToken);

            await NativeImportCommand.InitializeAsync(this);
            await OpenFolderCommand.InitializeAsync(this);
            await OpenMdlAppsFolderCommand.InitializeAsync(this);
            await OpenSDKFolderCommand.InitializeAsync(this);
            await SearchBentleyForumsCommand.InitializeAsync(this);
            await VSTMCHelpCommand.InitializeAsync(this);
            await BentleyHelpCommand.InitializeAsync(this);
        }
       
        //private async Task LoadPackageAsync(CancellationToken ct)
        //{
        //    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(ct);

        //    var shell = await GetServiceAsync(typeof(SVsShell)) as IVsShell;
        //    Assumes.Present(shell);

        //    if (shell.IsPackageLoaded(ref PackageGuids.guidPackage, out IVsPackage package) != VSConstants.S_OK)
        //    {
        //        ErrorHandler.ThrowOnFailure(shell.LoadPackage(ref PackageGuids.guidPackage, out package));
        //    }
        //}        

        private async Task AddFilesAsync(CancellationToken ct)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(ct);

            var dte = await GetServiceAsync(typeof(DTE)) as DTE;
            
            if (dte is null)
            {
                ActivityLog.LogWarning("VSTMC", "DTE service not available in AddFilesAsync.");
                return;
            }

            string vsPath = GetVisualStudioPathFromDte(dte);

            AddFile(GetAssemblyPath + "\\Resources\\KeyinTree.xsd", vsPath + "\\xml\\Schemas\\KeyinTree.xsd");
            AddFile(GetAssemblyPath + "\\Resources\\RibbonDefinitions.xsd", vsPath + "\\xml\\Schemas\\RibbonDefinitions.xsd");
            AddFile(GetAssemblyPath + "\\ExampleBatchFiles\\AECOsim.exe.bat",
                Environment.GetEnvironmentVariable("ProgramData") + @"\innovoCAD\Bentley\VisualStudioTools\AECOsim.exe.bat");
            AddFile(GetAssemblyPath + "\\ExampleBatchFiles\\MicroStation.exe.bat",
                Environment.GetEnvironmentVariable("ProgramData") + @"\innovoCAD\Bentley\VisualStudioTools\MicroStation.exe.bat");
            AddFile(GetAssemblyPath + "\\ExampleBatchFiles\\OpenRoadsDesigner.exe.bat",
                Environment.GetEnvironmentVariable("ProgramData") + @"\innovoCAD\Bentley\VisualStudioTools\OpenRoadsDesigner.exe.bat");
            AddFile(GetAssemblyPath + "\\include\\Declare_ustnTaskId_Import.h",
                Environment.GetEnvironmentVariable("ProgramData") + @"\innovoCAD\Bentley\include\Declare_ustnTaskId_Import.h");
            AddFile(GetAssemblyPath + "\\include\\std_collection_typedefs.h",
                Environment.GetEnvironmentVariable("ProgramData") + @"\innovoCAD\Bentley\include\std_collection_typedefs.h");
            AddFile(GetAssemblyPath + "\\include\\tstring_typedef.h",
                Environment.GetEnvironmentVariable("ProgramData") + @"\innovoCAD\Bentley\include\tstring_typedef.h");
        }


        /// <summary>
        /// Get Visual Studio path.
        /// </summary>
        //public string GetVisualStudioPath
        //{
        //    get
        //    {
        //        ThreadHelper.ThrowIfNotOnUIThread();
        //        return Path.GetFullPath(Path.Combine(Dte.FullName, @"..\..\..\"));
        //    }
        //}

        private static string GetVisualStudioPathFromDte(EnvDTE.DTE dte)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return Path.GetFullPath(Path.Combine(dte.FullName, @"..\..\..\"));
        }


        /// <summary>
        /// Get Assembly path.
        /// </summary
        public string GetAssemblyPath
        {
            get
            {
                return System.IO.Path.GetDirectoryName(new System.Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            }
        }

        public DTE? Dte { get; set; }

        /// <summary>
        /// Add necessary files.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        private void AddFile(string source, string target)
        {
            try
            {
                if (!File.Exists(target))
                {
                    if (!Directory.Exists(Path.GetDirectoryName(target)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(target));
                    }
                    File.Copy(source, target, true);
                }
            }
            catch (Exception)
            {
                //do nothing...
            }
        }
        #endregion
    }
}
