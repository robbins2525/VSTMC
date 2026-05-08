
# OpenRoads Designer 2024 – Native C++ Developer Setup Guide

## Purpose

This document captures the configuration changes and environment setup required to successfully build OpenRoads Designer 2024 Native (C++) projects using Visual Studio 2022.

The default SDK configuration assumes Visual Studio 2019 and relies on specific environment variables and working-directory behavior during the MKI/BMake pipeline. These notes prevent future rebuild issues.

---

# Executive Summary (Quick Reference)

To build OpenRoads 2024 Native C++ with Visual Studio 2022:

1. Update `SDKVersionInfo.bat` to target VS2022 (major version 17).
2. Modify `OpenRoadsDesigner.exe.bat` to explicitly set `SrcRoot` and control working directories.
3. Ensure required system environment variables are defined.
4. Add the SDK `objects` directory to Visual Studio include paths.
5. Ignore IntelliSense-only errors if the actual build succeeds.

---

# 1. SDKVersionInfo.bat Modification

**File Location:**

C:\Program Files\Bentley\OpenRoadsDesigner2024SDK\bin\SDKVersionInfo.bat

### Required Settings for VS2022

```bat
set SDKVSVER=2022
set SDKVSMAJORVER=17
set SDKNETVER=v4.8
```

---

# 2. OpenRoadsDesigner.exe.bat – Final Working Template

**File Location:**

C:\ProgramData\innovoCAD\Bentley\VisualStudioTools\OpenRoadsDesigner.exe.bat

```bat
@echo off
setlocal

set "SDKROOT=%~fs2"
if not "%SDKROOT:~-1%"=="\" set "SDKROOT=%SDKROOT%\"

set "SrcRoot=%SDKROOT%"

set "MS=%~fs1"
set "MSMD=%SDKROOT%"
set "MSMDE=%SDKROOT%"

pushd "%SDKROOT%"
call "%SDKROOT%OpenRoadsDesignerDeveloperShell.bat" "%~fs1" "%SDKROOT%"
popd

pushd "%~4"
bmake %~5
popd

endlocal
```

---

# 3. Required Environment Variables

```
Bentley_App=OpenRoadsDesigner.exe
Bentley_MdlappsPath=C:\Program Files\Bentley\OpenRoads Designer 2024.00\OpenRoadsDesigner\Mdlapps\
Bentley_NativeBuildFile=%Bentley_NativeBuildPath%%Bentley_App%.bat
        C:\ProgramData\innovoCAD\Bentley\VisualStudioTools\OpenRoadsDesigner.exe.bat
Bentley_NativeBuildPath=C:\ProgramData\innovoCAD\Bentley\VisualStudioTools\
Bentley_AppPath=C:\Program Files\Bentley\OpenRoads Designer 2024.00\OpenRoadsDesigner\
Bentley_SDKPath=C:\Program Files\Bentley\OpenRoadsDesigner2024SDK\
Bentley_IncludePath=C:\Program Files\Bentley\OpenRoadsDesigner2024SDK\include\
Bentley_LibraryPath=C:\Program Files\Bentley\OpenRoadsDesigner2024SDK\library\
Bentley_ReferencePaths=C:\Program Files\Bentley\OpenRoads Designer 2024.00\OpenRoadsDesigner;C:\Program Files\Bentley\OpenRoads Designer 2024.00\OpenRoadsDesigner
Bentley_IncludePathExtra=C:\ProgramData\innovoCAD\Bentley\include\

```

---

# 4. Visual Studio Include Paths

Add to:

Project Properties → C/C++ → General → Additional Include Directories

```
$(Bentley_IncludePath)
$(Bentley_SDKPath)objects
$(IntDir)objects
```

---

# 5. IntelliSense vs Real Build Errors

If the Output window shows:

Build succeeded.

but Error List shows missing headers, those are IntelliSense-only diagnostics.

Switch Error List filter to **Build Only** to confirm.

---

# 6. PowerShell Environment Validation Script

Save as:

Verify-OpenRoads-DevEnv.ps1

```powershell
Write-Host "Checking OpenRoads Developer Environment..." -ForegroundColor Cyan

$vars = @(
    "Bentley_AppPath",
    "Bentley_SDKPath",
    "Bentley_NativeBuildFile",
    "Bentley_IncludePath",
    "Bentley_LibraryPath",
    "VSDIR"
)

foreach ($var in $vars) {
    $value = [Environment]::GetEnvironmentVariable($var, "Machine")
    if (-not $value) {
        Write-Host "❌ $var is NOT defined." -ForegroundColor Red
    } else {
        Write-Host "✅ $var = $value" -ForegroundColor Green
    }
}

$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"

if (Test-Path $vswhere) {
    Write-Host "Checking Visual Studio installation..." -ForegroundColor Cyan
    & $vswhere -latest -products * -property displayName
} else {
    Write-Host "⚠ vswhere not found." -ForegroundColor Yellow
}

Write-Host "Environment check complete."
```
# Visual Studio Extension – Strong-Name Signing & PublicKeyToken Guide

## Purpose

This section documents the required steps to properly strong-name sign a Visual Studio extension assembly and retrieve the correct `PublicKeyToken` for use in `.vstemplate` files.

A `PublicKeyToken` only exists for strong-named assemblies. If the assembly is not strong-named, the correct value is:

```
PublicKeyToken=null
```

---

# Executive Summary (Quick Reference)

To generate a valid PublicKeyToken:

1. Create a strong-name key (`.snk`).
2. Configure the project to sign the assembly.
3. Ensure **Delay Sign is disabled**.
4. Clean and rebuild.
5. Validate with `sn -vf`.
6. Retrieve token using `sn -T` (capital T).
7. Update `.vstemplate`.

---

# 1. Create a Strong-Name Key (.snk)

Open:

Developer Command Prompt for Visual Studio

Run:

```bat
sn -k innovoCAD.snk
```

Expected output:

```
Key pair written to innovoCAD.snk
```

Store the `.snk` file in a stable project or solution folder.  
Do not delete or regenerate it later unless you intend to change the PublicKeyToken.

---

# 2. Configure Project Signing

Open:

Project → Properties → Signing

Required settings:

- ✔ Sign the assembly
- Select: `innovoCAD.snk`
- ☐ Delay sign only (must be unchecked)

Rebuild the project after applying changes.

---

# 3. Verify Assembly is Strong-Named

After rebuild, validate the compiled DLL:

```bat
sn -vf "FullPathToYourAssembly.dll"
```

Expected:

```
Assembly '...' is valid
```

If validation fails, the assembly is not properly signed.

---

# 4. Retrieve the PublicKeyToken

⚠ Important: Use **capital T**

```bat
sn -T "FullPathToYourAssembly.dll"
```

Expected output:

```
Public key token is 0123456789abcdef
```

That 16-character hexadecimal string is the value to use.

---

# 5. Update .vstemplate

Example:

```xml
<Assembly>
innovoCAD.Bentley.CONNECT, Version=6.0.1.2, Culture=neutral, PublicKeyToken=0123456789abcdef
</Assembly>
```

If unsigned:

```xml
PublicKeyToken=null
```

---

# 6. PowerShell Fallback (Metadata Extraction)

If `sn -T` fails unexpectedly, use:

```powershell
$p = [System.Reflection.AssemblyName]::GetAssemblyName("FullPathToYourAssembly.dll").GetPublicKeyToken()

if(!$p -or $p.Length -eq 0){
    "PublicKeyToken=null"
}
else{
    ($p | ForEach-Object { $_.ToString("x2") }) -join ""
}
```

---

# 7. Final Verification Checklist

- [ ] `.snk` file created
- [ ] Project signing enabled
- [ ] Delay sign disabled
- [ ] Clean + Rebuild completed
- [ ] `sn -vf` reports valid
- [ ] `sn -T` returns 16-character token
- [ ] `.vstemplate` updated with correct token
- [ ] Version number in `.vstemplate` matches assembly version
