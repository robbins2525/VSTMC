using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VSTMC
{
    public static class BentleyDataCollector
    {
        private const string ProductsRegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\UserData\S-1-5-18\Products";

        private sealed class BentleyProductDefinition
        {
            public BentleyProductDefinition(string familyKey, string displayLabel, string[] nameTokens, string executableName, string? appFolderName = null)
            {
                FamilyKey = familyKey;
                DisplayLabel = displayLabel;
                NameTokens = nameTokens;
                ExecutableName = executableName;
                AppFolderName = appFolderName;
            }

            public string FamilyKey { get; }
            public string DisplayLabel { get; }
            public string[] NameTokens { get; }
            public string ExecutableName { get; }
            public string? AppFolderName { get; }
        }

        private sealed class BentleyInstallation
        {
            public BentleyInstallation(BentleyProductDefinition definition, string displayName, string version, string installLocation)
            {
                Definition = definition;
                DisplayName = displayName;
                Version = version;
                InstallLocation = installLocation;
            }

            public BentleyProductDefinition Definition { get; }
            public string DisplayName { get; }
            public string Version { get; }
            public string InstallLocation { get; }
            public string KeyName => string.IsNullOrWhiteSpace(Version)
                ? DisplayName
                : $"{DisplayName} - v{Version}";
        }

        private static readonly BentleyProductDefinition[] ProductDefinitions =
        {
            new BentleyProductDefinition("MicroStation", "MicroStation", new[] { "microstation" }, "MicroStation.exe", "MicroStation"),
            new BentleyProductDefinition("OpenRoads", "OpenRoads Designer", new[] { "openroads designer" }, "OpenRoadsDesigner.exe", "OpenRoadsDesigner"),
            new BentleyProductDefinition("PowerDraft", "MicroStation PowerDraft", new[] { "powerdraft", "microstation powerdraft" }, "Draft.exe", "PowerDraft"),
            new BentleyProductDefinition("AECOsim", "AECOsim Building Designer", new[] { "aecosim building designer" }, "AECOsimBuildingDesigner.exe", "AECOsimBuildingDesigner"),
            new BentleyProductDefinition("Descartes", "Bentley Descartes", new[] { "bentley descartes", "descartes" }, "DescartesStandAlone.exe", "DescartesStandAlone"),
            new BentleyProductDefinition("Map", "Bentley Map", new[] { "bentley map" }, "BentleyMap.exe", "BentleyMap"),
            new BentleyProductDefinition("OpenBridge", "OpenBridge Modeler", new[] { "openbridge modeler" }, "OpenBridgeModeler.exe", "OpenBridgeModeler"),
            new BentleyProductDefinition("OpenPlant", "OpenPlant Modeler", new[] { "openplant modeler" }, "OpenPlantModeler.exe", "OpenPlantModeler"),
            new BentleyProductDefinition("OpenRail", "OpenRail Designer", new[] { "openrail designer" }, "OpenRailDesigner.exe", "OpenRailDesigner"),
            new BentleyProductDefinition("Substation", "Bentley Substation", new[] { "bentley substation", "substation" }, "Substation.exe", "Substation")
        };

        public static SortedDictionary<string, string> BentleyProducts { get; private set; } = new(StringComparer.OrdinalIgnoreCase);

        private static bool ContainsIgnoreCase(string source, string value)
        {
            return !string.IsNullOrEmpty(source) && source.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static string Bentley_AppPath()
        {
            var installations = GetInstalledApplications();
            BentleyProducts = BuildProductsDictionary(installations);

            string envPath = Environment.GetEnvironmentVariable("Bentley_AppPath") ?? string.Empty;
            if (PathLooksInstalled(envPath))
            {
                string normalizedEnvPath = EnsureTrailingSlash(envPath);
                if (BentleyProducts.Count > 0 && !BentleyProducts.ContainsValue(normalizedEnvPath))
                {
                    BentleyProducts["Configured Environment Path"] = normalizedEnvPath;
                }

                return normalizedEnvPath;
            }

            return installations.FirstOrDefault()?.InstallLocation ?? "Not Installed";
        }

        public static string SDKPath(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "Not Installed";

            string normalizedValue = value.Trim();
            BentleyProductDefinition? definition = FindDefinition(normalizedValue);
            if (definition is null)
                return "Not Installed";

            return GetInstalledSdkPath(definition);
        }

        public static string GetSDKPath(string bentleyAppPath)
        {
            string envSdkPath = GetConfiguredSdkPath();
            if (PathLooksInstalled(envSdkPath))
                return EnsureTrailingSlash(envSdkPath);

            BentleyProductDefinition? definition = FindDefinitionFromPath(bentleyAppPath);
            if (definition is null)
                definition = FindDefinition(Environment.GetEnvironmentVariable("Bentley_App") ?? string.Empty);

            return definition is null ? "Not Installed" : GetInstalledSdkPath(definition);
        }

        public static string GetBentleyApp(string bentleyAppPath)
        {
            BentleyProductDefinition? definition = FindDefinitionFromPath(bentleyAppPath);
            if (definition is not null)
                return definition.ExecutableName;

            string configuredApp = Environment.GetEnvironmentVariable("Bentley_App") ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(configuredApp))
                return configuredApp;

            return "MicroStation.exe";
        }

        public static string GetMdlappsPath(string bentleyAppPath)
        {
            string envMdlAppsPath = Environment.GetEnvironmentVariable("Bentley_MdlappsPath") ?? string.Empty;
            if (PathLooksInstalled(envMdlAppsPath))
                return EnsureTrailingSlash(envMdlAppsPath);

            if (string.Equals(bentleyAppPath, "Not Installed", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(bentleyAppPath))
                return "Not Installed";

            return EnsureTrailingSlash(Path.Combine(EnsureTrailingSlash(bentleyAppPath), "Mdlapps"));
        }

        public static string BentleyBuildBatchFilePath(string bentleyApp)
        {
            string configuredBuildFile = Environment.GetEnvironmentVariable("Bentley_NativeBuildFile") ?? string.Empty;
            if (File.Exists(configuredBuildFile))
                return configuredBuildFile;

            string programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

            if (ContainsIgnoreCase(bentleyApp, "AECOsimBuildingDesigner"))
                return Path.Combine(programData, @"innovoCAD\Bentley\VisualStudioTools\AECOsim.exe.bat");
            else if (ContainsIgnoreCase(bentleyApp, "OpenRoadsDesigner") ||
                     ContainsIgnoreCase(bentleyApp, "OpenRailDesigner"))
                return Path.Combine(programData, @"innovoCAD\Bentley\VisualStudioTools\OpenRoadsDesigner.exe.bat");
            else if (ContainsIgnoreCase(bentleyApp, "BentleyMap"))
                return Path.Combine(programData, @"innovoCAD\Bentley\VisualStudioTools\MapBuild.bat");
            else
                return Path.Combine(programData, @"innovoCAD\Bentley\VisualStudioTools\MicroStation.exe.bat");
        }

        private static List<BentleyInstallation> GetInstalledApplications()
        {
            var installations = new List<BentleyInstallation>();

            foreach (var propertyKey in EnumerateInstallPropertyKeys())
            {
                try
                {
                    string displayName = propertyKey.GetValue("DisplayName", string.Empty) as string ?? string.Empty;
                    string installLocation = propertyKey.GetValue("InstallLocation", string.Empty) as string ?? string.Empty;
                    string version = propertyKey.GetValue("DisplayVersion", string.Empty) as string ?? string.Empty;

                    if (string.IsNullOrWhiteSpace(displayName) || string.IsNullOrWhiteSpace(installLocation))
                        continue;

                    string normalizedName = displayName.ToLowerInvariant();
                    if (normalizedName.Contains("sdk") || normalizedName.Contains("documentation"))
                        continue;

                    BentleyProductDefinition? definition = FindDefinition(displayName);
                    if (definition is null)
                        continue;

                    string normalizedPath = NormalizeAppInstallLocation(installLocation, definition);
                    if (!PathLooksInstalled(normalizedPath))
                        continue;

                    installations.Add(new BentleyInstallation(definition, displayName, version, normalizedPath));
                }
                catch
                {
                    // Keep scanning. Bentley's installer catalog can be a junk drawer.
                }
            }

            return installations
                .GroupBy(i => i.InstallLocation, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.OrderByDescending(x => x.Version, StringComparer.OrdinalIgnoreCase).First())
                .OrderBy(i => i.DisplayName, StringComparer.OrdinalIgnoreCase)
                .ThenByDescending(i => i.Version, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static IEnumerable<RegistryKey> EnumerateInstallPropertyKeys()
        {
            var propertyKeys = new List<RegistryKey>();

            using (RegistryKey localMachineRegistry = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
            using (RegistryKey? productsKey = localMachineRegistry.OpenSubKey(ProductsRegistryPath))
            {
                if (productsKey is null)
                    return propertyKeys;

                foreach (string subKeyName in productsKey.GetSubKeyNames())
                {
                    try
                    {
                        RegistryKey? propertyKey = localMachineRegistry.OpenSubKey($"{ProductsRegistryPath}\\{subKeyName}\\InstallProperties");
                        if (propertyKey is not null)
                            propertyKeys.Add(propertyKey);
                    }
                    catch
                    {
                        // Skip anything half-installed or malformed.
                    }
                }
            }

            return propertyKeys;
        }

        private static SortedDictionary<string, string> BuildProductsDictionary(IEnumerable<BentleyInstallation> installations)
        {
            var products = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (BentleyInstallation installation in installations)
            {
                string keyName = installation.KeyName;
                if (!products.ContainsKey(keyName))
                    products.Add(keyName, installation.InstallLocation);
            }

            if (products.Count == 0)
                products.Add("Not Installed", "Not Installed");

            return products;
        }

        private static BentleyProductDefinition? FindDefinition(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            string normalized = text.ToLowerInvariant();
            return ProductDefinitions.FirstOrDefault(d => d.NameTokens.Any(token => normalized.Contains(token)));
        }

        private static BentleyProductDefinition? FindDefinitionFromPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || string.Equals(path, "Not Installed", StringComparison.OrdinalIgnoreCase))
                return null;

            string normalizedPath = path.Replace('/', '\\');
            return ProductDefinitions.FirstOrDefault(d =>
                normalizedPath.IndexOf(d.ExecutableName.Replace(".exe", string.Empty), StringComparison.OrdinalIgnoreCase) >= 0 ||
                (!string.IsNullOrWhiteSpace(d.AppFolderName) && normalizedPath.IndexOf(d.AppFolderName, StringComparison.OrdinalIgnoreCase) >= 0));
        }

        private static string GetInstalledSdkPath(BentleyProductDefinition definition)
        {
            foreach (var propertyKey in EnumerateInstallPropertyKeys())
            {
                try
                {
                    string displayName = propertyKey.GetValue("DisplayName", string.Empty) as string ?? string.Empty;
                    string installLocation = propertyKey.GetValue("InstallLocation", string.Empty) as string ?? string.Empty;

                    if (string.IsNullOrWhiteSpace(displayName) || string.IsNullOrWhiteSpace(installLocation))
                        continue;

                    string normalizedName = displayName.ToLowerInvariant();
                    if (!normalizedName.Contains("sdk"))
                        continue;

                    if (!definition.NameTokens.Any(token => normalizedName.Contains(token)))
                        continue;

                    string normalizedPath = EnsureTrailingSlash(installLocation);
                    if (PathLooksInstalled(normalizedPath))
                        return normalizedPath;
                }
                catch
                {
                    // ignore and continue scanning
                }
            }

            return "Not Installed";
        }

        private static string NormalizeAppInstallLocation(string installLocation, BentleyProductDefinition definition)
        {
            string normalizedPath = EnsureTrailingSlash(installLocation);

            if (!string.IsNullOrWhiteSpace(definition.AppFolderName))
            {
                string folderSegment = $"\\{definition.AppFolderName}\\";
                if (normalizedPath.IndexOf(folderSegment, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    string appendedPath = EnsureTrailingSlash(Path.Combine(normalizedPath, definition.AppFolderName));
                    if (Directory.Exists(appendedPath))
                        normalizedPath = appendedPath;
                }
            }

            return normalizedPath;
        }

        private static string GetConfiguredSdkPath()
        {
            string sdkPath = Environment.GetEnvironmentVariable("Bentley_SDKPath") ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(sdkPath))
                return sdkPath;

            return Environment.GetEnvironmentVariable("MSCESDKPath") ?? string.Empty;
        }

        private static bool PathLooksInstalled(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || string.Equals(path, "Not Installed", StringComparison.OrdinalIgnoreCase))
                return false;

            return Directory.Exists(path.TrimEnd('\\')) || File.Exists(path);
        }

        private static string EnsureTrailingSlash(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return string.Empty;

            return path.EndsWith("\\", StringComparison.Ordinal) ? path : path + "\\";
        }
    }
}
