# ScanCriteriaHelper Implementation Guide

## Overview

Provides helper functions for constructing and configuring scan criteria used to query elements in the active model. It simplifies the setup of filters such as element type, level, and other selection conditions.

---
### Single ScanCriteriaHelper per Project

If used, there is only one `ScanCriteriaHelpers.h` necessary per project**. The helper is designed to be a shared utility for all commands and tools that need to perform element scanning. It should not be duplicated or renamed for different scan types. 
The name cannot be changed in the **Add Item** dialog box.

Attempting to add another helper will result in an error such as:

> *"ScanCriteriaHelpers.h already exists in the project."*

---