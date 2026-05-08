#nullable enable

using EnvDTE;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using WinForms = System.Windows.Forms;
using VSTMC.Utilities;

namespace VSTMC.Options
{
    [ComVisible(true)]
    [ToolboxItem(false)]
    [Guid(PackageGuids.guidPageString)]
    public class OptionsPage : DialogPage
    {
        #region Fields

        // Nullable because it is created lazily in Window getter and disposed later.
        private OptionsControlPage? optionsControl;

        #endregion

        #region Constructors

        public OptionsPage()
        {
            string detectedBentleyAppPath = BentleyDataCollector.Bentley_AppPath();
            Bentley_AppPath = GetConfiguredDirectory("Bentley_AppPath") ?? detectedBentleyAppPath;
            MSCESDKPath = GetConfiguredDirectory("Bentley_SDKPath")
                ?? GetConfiguredDirectory("MSCESDKPath")
                ?? BentleyDataCollector.GetSDKPath(Bentley_AppPath);

            BentleyApp = GetConfiguredValue("Bentley_App") ?? BentleyDataCollector.GetBentleyApp(Bentley_AppPath);
            MDLAPPSPath = GetConfiguredDirectory("Bentley_MdlappsPath") ?? BentleyDataCollector.GetMdlappsPath(Bentley_AppPath);
            BentleyBuildFilePath = GetConfiguredFile("Bentley_NativeBuildFile") ?? BentleyDataCollector.BentleyBuildBatchFilePath(BentleyApp);
            BatchLock = false;
            MDLAPPSLock = false;
        }

        #endregion

        #region Properties

        [Category("Bentley")]
        [Description("MSCE Installation Path")]
        public string Bentley_AppPath { get; set; }

        [Category("Bentley")]
        [Description("MSCE SDK Installation Path")]
        public string MSCESDKPath { get; set; }

        [Category("Bentley")]
        [Description("MSCE Bentley App")]
        public string BentleyApp { get; set; }

        [Category("Bentley")]
        [Description("MSCE MDLAPPS Path")]
        public string MDLAPPSPath { get; set; }

        [Category("Bentley")]
        [Description("MicroStation CONNECT Bentley Build Path")]
        public string BentleyBuildFilePath { get; set; }

        [Category("Bentley")]
        [Description("Bentley build batch file path lock")]
        public bool BatchLock { get; set; }

        [Category("Bentley")]
        [Description("MDLApps path lock")]
        public bool MDLAPPSLock { get; set; }

        #endregion

        #region Methods

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected override WinForms.IWin32Window Window
        {
            get
            {
                if (optionsControl is null)
                {
                    optionsControl = new OptionsControlPage
                    {
                        Location = new Point(0, 0),
                        OptionsPage = this
                    };

                    optionsControl.Initialize();
                }

                return optionsControl;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                optionsControl?.Dispose();
                optionsControl = null;
            }

            base.Dispose(disposing);
        }

        #endregion

        private static string? GetConfiguredValue(string variableName)
        {
            string? value = Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.Process);
            if (string.IsNullOrWhiteSpace(value))
                value = Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.User);

            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        private static string? GetConfiguredDirectory(string variableName)
        {
            string? path = GetConfiguredValue(variableName);
            if (string.IsNullOrWhiteSpace(path))
                return null;

            path = path?.Trim();
            return Directory.Exists(path?.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)) ? path : null;
        }

        private static string? GetConfiguredFile(string variableName)
        {
            string? path = GetConfiguredValue(variableName);
            if (string.IsNullOrWhiteSpace(path))
                return null;

            path = path?.Trim();
            return File.Exists(path) ? path : null;
        }

        #region Event Handlers

        protected override void OnActivate(CancelEventArgs e)
        {
            base.OnActivate(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
        }

        protected override void OnDeactivate(CancelEventArgs e)
        {
            base.OnDeactivate(e);
        }

        protected override void OnApply(PageApplyEventArgs e)
        {
            base.OnApply(e);

            var pkg = VSTMCPackage.Instance;
            if (pkg is null)
            {
                ActivityLog.LogWarning("VSTMC", "Package instance not available during OnApply.");
                return;
            }

            pkg.JoinableTaskFactory.RunAsync(async () =>
            {
                try
                {
                    await SetEnvironmentAsync(pkg, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    ActivityLog.LogError("VSTMC", ex.ToString());
                }
            }).FileAndForget("VSTMC/SetEnvironmentOnApply");
        }

        #endregion

        #region Helper Methods

        private async Task SetEnvironmentAsync(AsyncPackage package, CancellationToken cancellationToken)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            var dte = await package.GetServiceAsync(typeof(DTE)) as DTE;
            Assumes.Present(dte);

            EnvDTE.StatusBar statusBar = dte.StatusBar;

            string msPath = Bentley_AppPath ?? string.Empty;
            string sdkPath = MSCESDKPath ?? string.Empty;
            string mdlAppsPath = MDLAPPSPath ?? string.Empty;
            string bentleyApp = BentleyApp ?? string.Empty;
            string buildFilePath = BentleyBuildFilePath ?? string.Empty;

            void UiProgress(int amount)
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                IncrementStatus(statusBar, amount);
            }

            // ✅ FIX: return a bool instead of a 1-element tuple
            bool updated = await Task.Run(() =>
            {
                bool localSetEnvironment = false;
                bool localUpdated = false;

                if (!string.Equals(Environment.GetEnvironmentVariable("Bentley_AppPath"), msPath, StringComparison.Ordinal))
                {
                    localSetEnvironment = true;
                    Environment.SetEnvironmentVariable("Bentley_AppPath", msPath, EnvironmentVariableTarget.User);
                    Environment.SetEnvironmentVariable("Bentley_AppPath", msPath, EnvironmentVariableTarget.Process);
                    localUpdated = true;
                }

                if (!string.Equals(Environment.GetEnvironmentVariable("Bentley_SDKPath"), sdkPath, StringComparison.Ordinal))
                {
                    Environment.SetEnvironmentVariable("Bentley_SDKPath", sdkPath, EnvironmentVariableTarget.User);
                    Environment.SetEnvironmentVariable("Bentley_SDKPath", sdkPath, EnvironmentVariableTarget.Process);
                    localUpdated = true;
                }

                if (!string.Equals(Environment.GetEnvironmentVariable("MSCESDKPath"), sdkPath, StringComparison.Ordinal))
                {
                    Environment.SetEnvironmentVariable("MSCESDKPath", sdkPath, EnvironmentVariableTarget.User);
                    Environment.SetEnvironmentVariable("MSCESDKPath", sdkPath, EnvironmentVariableTarget.Process);
                    localUpdated = true;
                }

                if (!string.Equals(Environment.GetEnvironmentVariable("Bentley_MdlappsPath"), mdlAppsPath, StringComparison.Ordinal))
                {
                    Environment.SetEnvironmentVariable("Bentley_MdlappsPath", mdlAppsPath, EnvironmentVariableTarget.User);
                    Environment.SetEnvironmentVariable("Bentley_MdlappsPath", mdlAppsPath, EnvironmentVariableTarget.Process);
                    localUpdated = true;
                }

                if (!string.Equals(Environment.GetEnvironmentVariable("Bentley_App"), bentleyApp, StringComparison.Ordinal))
                {
                    Environment.SetEnvironmentVariable("Bentley_App", bentleyApp, EnvironmentVariableTarget.User);
                    Environment.SetEnvironmentVariable("Bentley_App", bentleyApp, EnvironmentVariableTarget.Process);
                    localUpdated = true;
                }

                if (!string.Equals(Environment.GetEnvironmentVariable("Bentley_NativeBuildFile"), buildFilePath, StringComparison.Ordinal))
                {
                    Environment.SetEnvironmentVariable("Bentley_NativeBuildFile", buildFilePath, EnvironmentVariableTarget.User);
                    Environment.SetEnvironmentVariable("Bentley_NativeBuildFile", buildFilePath, EnvironmentVariableTarget.Process);
                    localUpdated = true;
                }

                if (localSetEnvironment && Directory.Exists(msPath) && Directory.Exists(sdkPath))
                {
                    string msceInclude =
                        sdkPath + "include;" +
                        (Environment.GetEnvironmentVariable("Temp") ?? string.Empty) +
                        "\\Bentley\\MicroStationSDK\\objects";

                    if (msPath.IndexOf("AECOsim", StringComparison.OrdinalIgnoreCase) >= 0)
                        msceInclude += ";" + sdkPath + "ABDSDK\\include";

                    if (msPath.IndexOf("OpenRoads", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        msPath.IndexOf("OpenRail", StringComparison.OrdinalIgnoreCase) >= 0)
                        msceInclude += ";" + sdkPath + "Objects";

                    msceInclude += ";" + @"C:\ProgramData\innovoCAD\Bentley\Include\";

                    Environment.SetEnvironmentVariable("Bentley_IncludePath", msceInclude, EnvironmentVariableTarget.User);

                    string msceLibrary = sdkPath + "Library";
                    if (msPath.IndexOf("AECOsim", StringComparison.OrdinalIgnoreCase) >= 0)
                        msceLibrary += ";" + sdkPath + "ABDSDK\\Library";

                    Environment.SetEnvironmentVariable("Bentley_LibraryPath", msceLibrary, EnvironmentVariableTarget.User);

                    string msceReferencePaths =
                        msPath + ";" + msPath + "Assemblies\\;" + msPath + "Assemblies\\ECFramework\\";

                    void AddIfExists(string relative)
                    {
                        string full = Path.Combine(msPath, relative);
                        if (Directory.Exists(full))
                            msceReferencePaths += ";" + full + "\\";
                    }

                    AddIfExists(@"Map\bin\assemblies");
                    AddIfExists(@"Descartes\Assemblies");
                    AddIfExists(@"OpenRoads");
                    AddIfExists(@"Cif");
                    AddIfExists(@"Subsurface");
                    AddIfExists(@"Subsurface\SUDA");
                    AddIfExists(@"Assemblies\Cmf");
                    AddIfExists(@"Assemblies\Ism");
                    AddIfExists(@"Assemblies\OpenStaad");
                    AddIfExists(@"OBMAssemblies");
                    AddIfExists(@"DevExpress");
                    AddIfExists(@"Assemblies\Telerik");
                    AddIfExists(@"OpenRail");
                    AddIfExists(@"OverheadLine");

                    Environment.SetEnvironmentVariable("Bentley_ReferencePaths", msceReferencePaths, EnvironmentVariableTarget.User);

                    Environment.SetEnvironmentVariable(
                        "MSCE_Dependencies",
                        "bentley.lib;BentleyAllocator.lib;mdlbltin.lib;RmgrTools.lib;BentleyGeom.lib;DgnPlatform.lib;dgnview.lib",
                        EnvironmentVariableTarget.User);

                    Environment.SetEnvironmentVariable(
                        "MSCE_PreUserorDefinitions",
                        "BENTLEY_WARNINGS_HIGHEST_LEVEL;WIN32;winNT;__EXCEPTIONS;_VISCXX;_CONVERSION_DONT_USE_THREAD_LOCALE;_SECURE_SCL=0;WIN32_LEAN_AND_MEAN;NTDDI_WIN7SP1=0x06010100",
                        EnvironmentVariableTarget.User);

                    localUpdated = true;
                }

                return localUpdated;
            }, cancellationToken);

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            UiProgress(100);
            statusBar.Progress(false);

            if (updated)
            {
                WinForms.MessageBox.Show(
                    "Please restart Visual Studio for changes to take effect.",
                    "Visual Studio Tools for MicroStation CONNECT Edition",
                    WinForms.MessageBoxButtons.OK,
                    WinForms.MessageBoxIcon.Exclamation);
            }

            await AddFilesAsync(cancellationToken);
        }

        private void IncrementStatus(EnvDTE.StatusBar statusBar, int increment)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            statusBar.Progress(true, "Saving MicroStation CONNECT Edition Settings...", increment, 200);
        }

        private async Task AddFilesAsync(CancellationToken ct)
        {
            string visualStudioPath = await GetVisualStudioPathAsync(ct);
            string programData = Environment.GetEnvironmentVariable("ProgramData") ?? @"C:\ProgramData";
            string assemblyPath = GetAssemblyPath;

            await Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();

                AddFile(Path.Combine(assemblyPath, @"Resources\KeyinTree.xsd"),
                        Path.Combine(visualStudioPath, @"xml\Schemas\KeyinTree.xsd"));

                AddFile(Path.Combine(assemblyPath, @"Resources\RibbonDefinitions.xsd"),
                        Path.Combine(visualStudioPath, @"xml\Schemas\RibbonDefinitions.xsd"));

                AddFile(Path.Combine(assemblyPath, @"ExampleBatchFiles\AECOsim.exe.bat"),
                        Path.Combine(programData, @"innovoCAD\Bentley\VisualStudioTools\AECOsim.exe.bat"));

                AddFile(Path.Combine(assemblyPath, @"ExampleBatchFiles\MicroStation.exe.bat"),
                        Path.Combine(programData, @"innovoCAD\Bentley\VisualStudioTools\MicroStation.exe.bat"));

                AddFile(Path.Combine(assemblyPath, @"ExampleBatchFiles\OpenRoadsDesigner.exe.bat"),
                        Path.Combine(programData, @"innovoCAD\Bentley\VisualStudioTools\OpenRoadsDesigner.exe.bat"));

                AddFile(Path.Combine(assemblyPath, @"include\Declare_ustnTaskId_Import.h"),
                        Path.Combine(programData, @"innovoCAD\Bentley\include\Declare_ustnTaskId_Import.h"));

                AddFile(Path.Combine(assemblyPath, @"include\std_collection_typedefs.h"),
                        Path.Combine(programData, @"innovoCAD\Bentley\include\std_collection_typedefs.h"));

                AddFile(Path.Combine(assemblyPath, @"include\tstring_typedef.h"),
                        Path.Combine(programData, @"innovoCAD\Bentley\include\tstring_typedef.h"));
            }, ct);
        }

        private async Task<string> GetVisualStudioPathAsync(CancellationToken ct)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(ct);

            var dte = VsUtilities.Dte;
            Assumes.Present(dte);

            return Path.GetFullPath(Path.Combine(dte.FullName, @"..\..\..\"));
        }

        public string GetAssemblyPath =>
            Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath) ?? string.Empty;

        private void AddFile(string source, string target)
        {
            try
            {
                if (!File.Exists(target))
                {
                    var dir = Path.GetDirectoryName(target);
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    File.Copy(source, target, true);
                }
            }
            catch (Exception)
            {
                // swallow (original behavior)
            }
        }

        #endregion
    }
}
