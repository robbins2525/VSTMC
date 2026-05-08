#nullable enable

using EnvDTE;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using VSTMC.Properties;
using System.Web;
using WinForms = System.Windows.Forms;

namespace VSTMC.Utilities
{
    public class VsUtilities
    {
        #region Public Methods

        public void SearchBentleyForums()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var dte = Dte;
            if (dte.ActiveDocument is null)
                return;

            if (dte.ActiveDocument.Selection is not TextSelection selection)
                return;

            if (string.IsNullOrWhiteSpace(selection.Text))
                return;

            string encoded = HttpUtility.UrlEncode(selection.Text.Trim());
            //string url = "https://communities.bentley.com/search?q=" + encoded + "#serpsort=date%20desc&serpgroup=444";
            
            string url = "https://www.google.com/search?q=site:bentleysystems.service-now.com/community+" + encoded + "#serpsort=date%20desc&serpgroup=444";
            
            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }

        // Keep original signature for compatibility
        //public void ImportNativeApps(Package package)
        //{
        //    ThreadHelper.ThrowIfNotOnUIThread();

        //    this.package = package ?? throw new ArgumentNullException(nameof(package));

        //    var dte = Dte;

        //    string projectLocation =
        //        dte.get_Properties("Environment", "ProjectsAndSolution")
        //           .Item("ProjectsLocation")
        //           .Value?.ToString() ?? string.Empty;

        //    string? file = GetFilesDialog(
        //        "Select CONNECT Edition Make File",
        //        "Make File (*.mke)|*.mke",
        //        MSCESDKPath);

        //    if (string.IsNullOrWhiteSpace(file))
        //        return;

        //    FileInfo originalFileInfo = new FileInfo(file);
        //    InitialDirectory = originalFileInfo.DirectoryName;

        //    string[] pathSplit = (originalFileInfo.DirectoryName ?? string.Empty).Split('\\');

        //    bool isCanceled = false;
        //    string? solutionFile = null;
        //    string? solutionFileName = null;
        //    string? solutionPath = null;

        //    if (string.IsNullOrEmpty(dte.Solution.FullName))
        //    {
        //        solutionFile = GetFilesDialog(
        //            "Select Solution or Create New Solution to add Project",
        //            "Solution (*.sln)|*.sln",
        //            projectLocation,
        //            originalFileInfo.Name.Replace(originalFileInfo.Extension, "") + ".sln"
        //        );

        //        if (!string.IsNullOrEmpty(solutionFile))
        //        {
        //            try
        //            {
        //                FileInfo solutionFileInfo = new FileInfo(solutionFile);
        //                solutionFileName = solutionFileInfo.Name.Replace(".sln", "");
        //                solutionPath = (solutionFileInfo.DirectoryName ?? string.Empty) + "\\" + solutionFileName;
        //                dte.Solution.Open(solutionFile);
        //            }
        //            catch
        //            {
        //                FileInfo solutionFileInfo = new FileInfo(solutionFile);
        //                solutionFileName = solutionFileInfo.Name.Replace(".sln", "");
        //                solutionPath = (solutionFileInfo.DirectoryName ?? string.Empty) + "\\" + solutionFileName;
        //                dte.Solution.Create(solutionPath, solutionFileInfo.Name);
        //            }
        //        }
        //        else
        //        {
        //            isCanceled = true;
        //        }
        //    }
        //    else
        //    {
        //        FileInfo solutionFileInfo = new FileInfo(dte.Solution.FullName);
        //        solutionFileName = solutionFileInfo.Name.Replace(".sln", "");
        //        solutionPath = solutionFileInfo.DirectoryName;
        //        solutionFile = dte.Solution.FullName;
        //    }

        //    if (isCanceled || string.IsNullOrEmpty(solutionPath) || string.IsNullOrEmpty(solutionFileName))
        //        return;

        //    if (!Directory.Exists(solutionPath))
        //    {
        //        Directory.CreateDirectory(solutionPath);
        //        dte.Solution.SaveAs(solutionPath + "\\" + solutionFileName + ".sln");
        //    }

        //    if (solutionFile is null)
        //        return;

        //    string destinationDir = solutionPath + "\\" + originalFileInfo.Name.Replace(originalFileInfo.Extension, "") + "\\";

        //    if (!Directory.Exists(destinationDir))
        //    {
        //        Directory.CreateDirectory(destinationDir);

        //        string[] originalFiles = Directory.GetFiles(originalFileInfo.DirectoryName ?? string.Empty, "*", SearchOption.AllDirectories);
        //        Array.ForEach(originalFiles, originalFileLocation =>
        //        {
        //            FileInfo originalFile = new FileInfo(originalFileLocation);
        //            FileInfo destFile = new FileInfo(originalFileLocation.Replace(originalFileInfo.DirectoryName ?? string.Empty, destinationDir));

        //            if (destFile.Exists)
        //            {
        //                if (originalFile.Length > destFile.Length)
        //                    originalFile.CopyTo(destFile.FullName, true);
        //            }
        //            else
        //            {
        //                Directory.CreateDirectory(destFile.DirectoryName ?? destinationDir);
        //                originalFile.CopyTo(destFile.FullName, false);
        //            }
        //        });

        //        DirectoryInfo directoryInfo = new DirectoryInfo(destinationDir);

        //        string safeProjectName = pathSplit.Length > 0 ? pathSplit[pathSplit.GetUpperBound(0)] : originalFileInfo.Name.Replace(originalFileInfo.Extension, "");

        //        string result = Resources.CONNECTvcproj
        //            .Replace("$safeprojectname$", safeProjectName)
        //            .Replace("$guid1$", "{" + Guid.NewGuid() + "}")
        //            .Replace("$platformtoolset$", PlatFormToolSet(dte.Version));

        //        string resultFilters = Resources.CONNECTFilters
        //           .Replace("$guid1$", "{" + Guid.NewGuid() + "}")
        //           .Replace("$guid2$", "{" + Guid.NewGuid() + "}")
        //           .Replace("$guid3$", "{" + Guid.NewGuid() + "}");

        //        foreach (var item in directoryInfo.GetFiles("*", SearchOption.AllDirectories).OrderBy(f => f.Extension))
        //        {
        //            if (item.Extension.Equals(".cpp", StringComparison.OrdinalIgnoreCase) ||
        //                item.Extension.Equals(".h", StringComparison.OrdinalIgnoreCase))
        //            {
        //                result += "<ClCompile Include=";
        //            }
        //            else
        //            {
        //                result += "<ClInclude Include=";
        //            }
        //            result += "\"" + item.FullName.Replace(destinationDir, "") + "\"/>\n";
        //        }

        //        foreach (var directories in directoryInfo.GetDirectories())
        //        {
        //            resultFilters += "<ItemGroup>\n<Filter Include=\"" + directories.Name + "\">\n" +
        //                        "<UniqueIdentifier>{" + Guid.NewGuid() + "}</UniqueIdentifier>\n" +
        //                        "</Filter>\n" +
        //                        "</ItemGroup>\n";

        //            foreach (var files in directoryInfo.GetFiles("*", SearchOption.AllDirectories).OrderBy(f => f.Extension))
        //            {
        //                if (files.FullName.Contains(directories.Name))
        //                {
        //                    if (files.Extension.Equals(".cpp", StringComparison.OrdinalIgnoreCase) ||
        //                        files.Extension.Equals(".h", StringComparison.OrdinalIgnoreCase))
        //                    {
        //                        resultFilters += "<ItemGroup>\n" +
        //                                        "<ClCompile Include=\"" + files.FullName.Replace(destinationDir, "") + "\">\n" +
        //                                        "<Filter>" + directories.Name + "</Filter>\n" +
        //                                        "</ClCompile>\n" +
        //                                        "</ItemGroup>\n";
        //                    }
        //                    else if (files.Extension.Equals(".r", StringComparison.OrdinalIgnoreCase) ||
        //                             files.Extension.Equals(".rc", StringComparison.OrdinalIgnoreCase))
        //                    {
        //                        resultFilters += "<ItemGroup>\n" +
        //                                        "<ClInclude Include=\"" + files.FullName.Replace(destinationDir, "") + "\">\n" +
        //                                        "<Filter>" + directories.Name + "</Filter>\n" +
        //                                        "</ClInclude>\n" +
        //                                        "</ItemGroup>\n";
        //                    }
        //                }
        //            }
        //        }

        //        resultFilters += "</Project>\n";
        //        result += "</ItemGroup >\n" +
        //                  "<Import Project = \"$(VCTargetsPath)\\Microsoft.Cpp.targets\"/>\n" +
        //                  "<ImportGroup Label = \"ExtensionTargets\" >\n" +
        //                  "</ImportGroup>\n" +
        //                  "</Project>\n";

        //        string projectBase = originalFileInfo.Name.Replace(originalFileInfo.Extension, "");
        //        string projectFile = destinationDir + projectBase + ".vcxproj";
        //        string filterFile = destinationDir + projectBase + ".vcxproj.filters";

        //        File.WriteAllText(projectFile, result);
        //        File.WriteAllText(filterFile, resultFilters);

        //        UpdateStatusBar(projectBase + " successfully upgraded to Visual Studio Tools format.");
        //        dte.Solution.AddFromFile(projectFile);
        //    }
        //    else
        //    {
        //        UpdateStatusBar(originalFileInfo.Name.Replace(originalFileInfo.Extension, "") +
        //            " project creation failed: Project already exist in Visual Studio projects folder.");
        //    }
        //}

        public async Task ImportNativeAppsAsync(AsyncPackage package, CancellationToken cancellationToken)
        {
            // UI thread: DTE + dialogs
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            this.package = package;

            // Use your existing static property once, then use the local variable
            DTE dte = VsUtilities.Dte;

            string projectLocation =
                dte.get_Properties("Environment", "ProjectsAndSolution")
                   .Item("ProjectsLocation")
                   .Value?.ToString() ?? string.Empty;

            // Dialog 1: select .mke
            string? file = GetFilesDialog(
                "Select CONNECT Edition Make File",
                "Make File (*.mke)|*.mke",
                Bentley_SDKPath);

            if (string.IsNullOrWhiteSpace(file))
                return;


            if (string.IsNullOrWhiteSpace(file))
                return;

            FileInfo originalFileInfo = new(file);
            InitialDirectory = originalFileInfo.DirectoryName;

            string[] pathSplit = (originalFileInfo.DirectoryName ?? string.Empty)
                .Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);

            bool isCanceled = false;
            string? solutionFile = null;
            string? solutionFileName = null;
            string? solutionPath = null;

            // Ensure a solution exists/open
            if (string.IsNullOrEmpty(dte.Solution.FullName))
            {
                solutionFile = GetFilesDialog(
                    "Select Solution or Create New Solution to add Project",
                    "Solution (*.sln)|*.sln",
                    projectLocation,
                    originalFileInfo.Name.Replace(originalFileInfo.Extension, "") + ".sln"
                );

                if (!string.IsNullOrEmpty(solutionFile))
                {
                    FileInfo solutionFileInfo = new(solutionFile);
                    solutionFileName = solutionFileInfo.Name.Replace(".sln", "");
                    solutionPath = (solutionFileInfo.DirectoryName ?? string.Empty) + "\\" + solutionFileName;

                    try
                    {
                        dte.Solution.Open(solutionFile);
                    }
                    catch
                    {
                        dte.Solution.Create(solutionPath, solutionFileInfo.Name);
                    }
                }
                else
                {
                    isCanceled = true;
                }
            }
            else
            {
                FileInfo solutionFileInfo = new(dte.Solution.FullName);
                solutionFileName = solutionFileInfo.Name.Replace(".sln", "");
                solutionPath = solutionFileInfo.DirectoryName;
                solutionFile = dte.Solution.FullName;
            }

            if (isCanceled || string.IsNullOrEmpty(solutionPath) || string.IsNullOrEmpty(solutionFileName))
                return;

            if (!Directory.Exists(solutionPath))
            {
                Directory.CreateDirectory(solutionPath);
                dte.Solution.SaveAs(solutionPath + "\\" + solutionFileName + ".sln");
            }

            if (string.IsNullOrEmpty(solutionFile))
                return;

            string destinationDir = solutionPath + "\\" + originalFileInfo.Name.Replace(originalFileInfo.Extension, "") + "\\";

            string safeProjectName =
                (pathSplit.Length > 0) ? pathSplit[pathSplit.Length - 1] : originalFileInfo.Name.Replace(originalFileInfo.Extension, "");

            string platformToolset = PlatFormToolSet(dte.Version);

            // Background: file IO + generate project files
            BgResult bgResult = await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (Directory.Exists(destinationDir))
                {
                    return new BgResult(
                        false,
                        safeProjectName + " project creation failed: Project already exist in Visual Studio projects folder.",
                        null,
                        null);
                }

                Directory.CreateDirectory(destinationDir);

                string[] originalFiles = Directory.GetFiles(originalFileInfo.DirectoryName ?? string.Empty, "*", SearchOption.AllDirectories);

                foreach (var originalFileLocation in originalFiles)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    FileInfo originalFile = new(originalFileLocation);
                    FileInfo destFile = new(originalFileLocation.Replace(originalFileInfo.DirectoryName ?? string.Empty, destinationDir));

                    if (destFile.Exists)
                    {
                        if (originalFile.Length > destFile.Length)
                            originalFile.CopyTo(destFile.FullName, true);
                    }
                    else
                    {
                        Directory.CreateDirectory(destFile.DirectoryName ?? destinationDir);
                        originalFile.CopyTo(destFile.FullName, false);
                    }
                }

                DirectoryInfo directoryInfo = new(destinationDir);

                string result = Resources.CONNECTvcproj
                    .Replace("$safeprojectname$", safeProjectName)
                    .Replace("$guid1$", "{" + Guid.NewGuid().ToString() + "}")
                    .Replace("$platformtoolset$", platformToolset);

                string resultFilters = Resources.CONNECTFilters
                    .Replace("$guid1$", "{" + Guid.NewGuid().ToString() + "}")
                    .Replace("$guid2$", "{" + Guid.NewGuid().ToString() + "}")
                    .Replace("$guid3$", "{" + Guid.NewGuid().ToString() + "}");

                foreach (var item in directoryInfo.GetFiles("*", SearchOption.AllDirectories).OrderBy(f => f.Extension))
                {
                    if (item.Extension.Equals(".cpp", StringComparison.OrdinalIgnoreCase) ||
                        item.Extension.Equals(".h", StringComparison.OrdinalIgnoreCase))
                    {
                        result += "<ClCompile Include=";
                    }
                    else
                    {
                        result += "<ClInclude Include=";
                    }

                    result += "\"" + item.FullName.Replace(destinationDir, "") + "\"/>\n";
                }

                foreach (var dir in directoryInfo.GetDirectories("*", SearchOption.AllDirectories))
                {
                    resultFilters += "<ItemGroup>\n<Filter Include=\"" + dir.Name + "\">\n" +
                                     "<UniqueIdentifier>{" + Guid.NewGuid().ToString() + "}</UniqueIdentifier>\n" +
                                     "</Filter>\n" +
                                     "</ItemGroup>\n";
                }

                foreach (var f in directoryInfo.GetFiles("*", SearchOption.AllDirectories).OrderBy(f => f.Extension))
                {
                    string relative = f.FullName.Replace(destinationDir, "");
                    string? filterName = null;

                    var parts = relative.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 1)
                        filterName = parts[0];

                    if (string.IsNullOrEmpty(filterName))
                        continue;

                    if (f.Extension.Equals(".cpp", StringComparison.OrdinalIgnoreCase) ||
                        f.Extension.Equals(".h", StringComparison.OrdinalIgnoreCase))
                    {
                        resultFilters += "<ItemGroup>\n" +
                                         "<ClCompile Include=\"" + relative + "\">\n" +
                                         "<Filter>" + filterName + "</Filter>\n" +
                                         "</ClCompile>\n" +
                                         "</ItemGroup>\n";
                    }
                    else if (f.Extension.Equals(".r", StringComparison.OrdinalIgnoreCase) ||
                             f.Extension.Equals(".rc", StringComparison.OrdinalIgnoreCase))
                    {
                        resultFilters += "<ItemGroup>\n" +
                                         "<ClInclude Include=\"" + relative + "\">\n" +
                                         "<Filter>" + filterName + "</Filter>\n" +
                                         "</ClInclude>\n" +
                                         "</ItemGroup>\n";
                    }
                }

                resultFilters += "</Project>\n";
                result += "</ItemGroup >\n" +
                          "<Import Project = \"$(VCTargetsPath)\\Microsoft.Cpp.targets\"/>\n" +
                          "<ImportGroup Label = \"ExtensionTargets\" >\n" +
                          "</ImportGroup>\n" +
                          "</Project>\n";

                string projBaseName = originalFileInfo.Name.Replace(originalFileInfo.Extension, "");
                string projectFile = destinationDir + projBaseName + ".vcxproj";
                string filterFile = destinationDir + projBaseName + ".vcxproj.filters";

                File.WriteAllText(projectFile, result);
                File.WriteAllText(filterFile, resultFilters);

                return new BgResult(
                    true,
                    projBaseName + " successfully upgraded to Visual Studio Tools format.",
                    projectFile,
                    filterFile);

            }, cancellationToken);

            // UI: update + add to solution
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            UpdateStatusBar(bgResult.Message);

            if (bgResult.Success && !string.IsNullOrEmpty(bgResult.ProjectFile))
            {
                dte.Solution.AddFromFile(bgResult.ProjectFile);
            }
        }

        private sealed class BgResult
        {
            public bool Success { get; }
            public string Message { get; }
            public string? ProjectFile { get; }
            public string? FilterFile { get; }

            public BgResult(bool success, string message, string? projectFile, string? filterFile)
            {
                Success = success;
                Message = message;
                ProjectFile = projectFile;
                FilterFile = filterFile;
            }
        }


        // Your async version is already mostly safe; leaving it as-is (it already uses ?? and guards)

        #endregion

        #region Private Methods

        private string? GetFilesDialog(string title, string filter, string initialDirectory, string? fileName = null)
        {
            var openFileDialog = new WinForms.OpenFileDialog
            {
                Title = title,
                CheckFileExists = false,
                Multiselect = false,
                Filter = filter,
                FilterIndex = 1,
                RestoreDirectory = true
            };

            if (!string.IsNullOrEmpty(fileName))
            {
                openFileDialog.InitialDirectory = initialDirectory;
                openFileDialog.FileName = fileName;
            }
            else
            {
                if (string.IsNullOrEmpty(InitialDirectory))
                    InitialDirectory = Bentley_SDKPath;

                openFileDialog.InitialDirectory = InitialDirectory ?? string.Empty;
            }

            return openFileDialog.ShowDialog() == WinForms.DialogResult.OK
                ? openFileDialog.FileName
                : null;
        }

        private void UpdateStatusBar(string message)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var sp = ServiceProvider;
            var statusBar = (IVsStatusbar)sp.GetService(typeof(SVsStatusbar));
            Assumes.Present(statusBar);

            statusBar.IsFrozen(out int frozen);
            if (frozen != 0)
                statusBar.FreezeOutput(0);

            statusBar.SetText(message);
            statusBar.FreezeOutput(1);
            statusBar.FreezeOutput(0);
            statusBar.Clear();
        }

        #endregion

        #region Public Properties

        public static Project? ActiveDocumentProject
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                var dte = Dte;
                return dte.ActiveDocument?.ProjectItem?.ContainingProject;
            }
        }        

        public static Project? FirstSolutionProject
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                try
                {
                    if (Dte?.Solution == null || !Dte.Solution.IsOpen)
                        return null;

                    foreach (Project p in Dte.Solution.Projects)
                    {
                        if (p != null && !string.IsNullOrEmpty(p.Name))
                            return p;
                    }
                }
                catch (COMException)
                {
                    // Solution still loading
                }
                catch { }

                return null;
            }
        }


        public static bool IsActiveDocumentBentleyProject
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                var docProj = ActiveDocumentProject;
                if (docProj?.FileName is null || !File.Exists(docProj.FileName))
                    return false;

                using var sr = new StreamReader(docProj.FileName);
                return sr.ReadToEnd().IndexOf("bentley", StringComparison.OrdinalIgnoreCase) >= 0;
            }
        }

        public static bool IsBentleyProject
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                var proj = SelectedProject;
                if (proj?.FileName is null || !File.Exists(proj.FileName))
                    return false;

                string text = File.ReadAllText(proj.FileName);

                return text.Contains("Bentley.MstnPlatformNET", StringComparison.OrdinalIgnoreCase)
                    || text.Contains("Bentley.MicroStation", StringComparison.OrdinalIgnoreCase)
                    || text.Contains("Bentley.DgnPlatformNET", StringComparison.OrdinalIgnoreCase)
                    || text.Contains("ustation", StringComparison.OrdinalIgnoreCase)
                    || text.Contains("mdl", StringComparison.OrdinalIgnoreCase)
                    || text.Contains("MicroStation", StringComparison.OrdinalIgnoreCase)
                    || text.Contains("MSCE", StringComparison.OrdinalIgnoreCase)
                    || text.Contains("OpenRoads", StringComparison.OrdinalIgnoreCase)
                    || text.Contains("OpenRail", StringComparison.OrdinalIgnoreCase)
                    || text.Contains("OpenBridge", StringComparison.OrdinalIgnoreCase)
                    || text.Contains("OpenPlant", StringComparison.OrdinalIgnoreCase);
            }
        }

        public static Project? SelectedProject
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                var dte = Package.GetGlobalService(typeof(SDTE)) as EnvDTE.DTE;
                if (dte?.SelectedItems is null || dte.SelectedItems.Count == 0)
                    return null;

                EnvDTE.SelectedItem item = dte.SelectedItems.Item(1);
                return item.Project ?? item.ProjectItem?.ContainingProject;
            }
        }

        public static bool IsCommandTableActiveDocument
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                return string.Equals(Dte.ActiveDocument?.Name, "Commands.xml", StringComparison.OrdinalIgnoreCase);
            }
        }

        public static bool IsTextSelected
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                var dte = Dte;
                if (dte.ActiveDocument?.Selection is not TextSelection selection)
                    return false;

                return !string.IsNullOrEmpty(selection.Text);
            }
        }

        public string Bentley_AppPath => Environment.GetEnvironmentVariable("Bentley_AppPath") ?? string.Empty;
        public string Bentley_SDKPath => Environment.GetEnvironmentVariable("Bentley_SDKPath") ?? string.Empty;
        public string Bentley_MDLPath => Environment.GetEnvironmentVariable("Bentley_MdlappsPath") ?? string.Empty;

        public void OpenFolderLocation(string folderLocation)
        {
            if (string.IsNullOrWhiteSpace(folderLocation))
                return;

            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = folderLocation,
                UseShellExecute = true,
                Verb = "open"
            });
        }

        public static string GetVisualStudioPath
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                return Path.GetFullPath(Path.Combine(Dte.FullName, @"..\..\..\"));
            }
        }

        public static string GetExtensionAssemblyPath =>
            Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath) ?? string.Empty;

        #endregion

        #region Private Properties

        public static DTE Dte
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                return (Package.GetGlobalService(typeof(SDTE)) as DTE)
                    ?? throw new InvalidOperationException("Unable to acquire DTE service (SDTE).");
            }
        }

        private static Array? ActiveSolutionProjects
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                // If no solution is open, ActiveSolutionProjects can throw E_FAIL
                if (Dte?.Solution == null || !Dte.Solution.IsOpen)
                    return Array.Empty<object>();

                try
                {
                    return Dte.ActiveSolutionProjects as Array ?? Array.Empty<object>();
                }
                catch (COMException)
                {
                    // VS sometimes throws E_FAIL while solution/projects are still loading
                    return Array.Empty<object>();
                }
                catch
                {
                    return Array.Empty<object>();
                }
            }
        }

        private string? InitialDirectory { get; set; }

        private string PlatFormToolSet(string version)
        {
            _ = double.TryParse(version, out double platformtoolset);
            if (platformtoolset == 0.00)
                return "120";

            return (platformtoolset * 10).ToString().Trim();
        }

        private IServiceProvider ServiceProvider =>
            package ?? throw new InvalidOperationException("Package has not been initialized on Utilities.");

        private Package? package { get; set; }

        #endregion
    }
}
