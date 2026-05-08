# Visual Studio Tools for MicroStation CONNECT Edition ver 12.0.0
### This needs to be updated
## Seamlessly Integrations of Visual Studio and MicroStation - A more reliable, productive, robust and a simply more enjoyable development experience.

#### Supported Bentley Products
+ **[Bentley MicroStation CONNECT Edition](https://www.bentley.com/en/products/brands/microstation/)**
+ **[Bentley OpenRoads Designer CONNECT Edition](https://www.bentley.com/en/products/brands/openroads/)**

#### Features
+ [**C# and VB .Net Project Templates**](#CSharp-and-VB-NET-Project-Templates) 
+ [**C++ Project Templates**](#CPP-Project-Templates) 
+ [**C# and VB .Net Item Templates**](#CSharp-and-VB-NET-Item-Templates) 
+ [**Snippets**](#Snippets)
+ [**Import Native C++ applications to Visual Studio**](#Import-Native-CPP) 
+ [**Search Bentley forums easily within Visual Studio**](#Search-Bentley-Forums) 
+ [**Keyin Command Table (Command.xml) Intellisense**](#Keyin-Command-Table-Intellisense)
+ [**Easily Get Needed Bentley Assembly References**](#Easily-Get-Needed-Bentley-Assembly-References)
+ [**Reference Assemblies** ***Copy Local*** **property set to** ***False*** **by default for MicroStation hosted add-ins**](#Copy-Local-Property) 
+ [**Open Bentley Application Folder and MDLAPPS Folder in Solution Explorer**](#Open-Bentley-Application-Folder-and-MDLAPPS-Folder-in-Solution-Explorer)

## Environment Variables
### Use you app where appropriate

| Variable | Value |
|---|---|
| `Bentley_App` | `MicroStation.exe`  or `OpenRoadsDesigner.exe` |
| `Bentley_AppPath` | `C:\Program Files\Bentley\OpenRoads Designer 2024.00\OpenRoadsDesigner\` |
| `Bentley_IncludePath` | `C:\Program Files\Bentley\OpenRoadsDesigner2024SDK\include\` |
| `Bentley_IncludePathExtra` | `C:\ProgramData\VSTMC\include\` |
| `Bentley_LibraryPath` | `C:\Program Files\Bentley\OpenRoadsDesigner2024SDK\library\` |
| `Bentley_MdlappsPath` | `C:\Program Files\Bentley\OpenRoads Designer 2024.00\OpenRoadsDesigner\Mdlapps\` |
| `Bentley_NativeBuildFile` | `C:\ProgramData\VSTMC\OpenRoadsDesigner.exe.bat` |
| `Bentley_NativeBuildPath` | `C:\ProgramData\VSTMC\` |
| `Bentley_ReferencePaths` | `C:\Program Files\Bentley\OpenRoads Designer 2024.00\OpenRoadsDesigner;C:\Program Files\Bentley\OpenRoads Designer 2024.00\OpenRoadsDesigner\Assemblies;C:\Program Files\Bentley\OpenRoads Designer 2024.00\OpenRoadsDesigner\Assemblies\ECFramework;C:\Program Files\Bentley\OpenRoads Designer 2024.00\OpenRoadsDesigner\Assemblies\ECFramework;C:\Program Files\Bentley\OpenRoads Designer 2024.00\OpenRoadsDesigner\Cif;C:\Program Files\Bentley\OpenRoads Designer 2024.00\OpenRoadsDesigner\Cif\en;C:\Program Files\Bentley\OpenRoads Designer 2024.00\OpenRoadsDesigner\Descartes\Assemblies;C:\Program Files\Bentley\OpenRoads Designer 2024.00\OpenRoadsDesigner\en;C:\Program Files\Bentley\OpenRoads Designer 2024.00\OpenRoadsDesigner\Geotechnical;C:\Program Files\Bentley\OpenRoads Designer 2024.00\OpenRoadsDesigner\OpenRoads;C:\Program Files\Bentley\OpenRoads Designer 2024.00\OpenRoadsDesigner\Subsurface;C:\Program Files\Bentley\OpenRoads Designer 2024.00\OpenRoadsDesigner\Subsurface\SUDA;C:\Program Files\Bentley\OpenRoads Designer 2024.00\OpenRoadsDesigner\Subsurface\SUE;C:\Program Files\Bentley\OpenRoads Designer 2024.00\OpenRoadsDesigner\Site` |
| `Bentley_SDKPath` | `C:\Program Files\Bentley\OpenRoadsDesigner2024SDK\` |

#### No Longer Supported Bentley Products (Since I am no longer actively involved) May work with appropriate variables set but will not guarantee or support.

+ **[Bentley AECOSim Building Designer CONNECT Edition](https://www.bentley.com/en/products/brands/aecosim/)**
+ **[Bentley Descartes CONNECT Edition](https://www.bentley.com/en/products/brands/descartes/)**
+ **[Bentley Map CONNECT Edition](https://www.bentley.com/en/products/brands/map/)**
+ **[Bentley OpenBridge Modeler CONNECT Edition](https://www.bentley.com/en/products/brands/openbridge-modeler/)**
+ **[Bentley OpenPlant CONNECT Edition](https://www.bentley.com/en/products/brands/openplant/)**
+ **[Bentley OpenRail CONNECT Edition](https://www.bentley.com/en/products/brands/openrail/)**
+ **Bentley Substation CONNECT Edition**
