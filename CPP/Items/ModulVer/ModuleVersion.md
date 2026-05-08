# ModuleVersion Implementation Guide 

## Overview

Provides a utility for retrieving version information from a module (DLL or executable) using Windows version resources. It enables access to file version data and optional exported version functions for diagnostics and compatibility checks.

---
### Single ModuleVersion per Project

If used, there is only one `ModuleVersion.h` and `ModuleVersion.cpp` necessary per project.  
The name cannot be changed in the **Add Item** dialog box.

Attempting to add another ModuleVersions will result in an error such as:

> *"ModuleVersion already exists in the project."*
---
