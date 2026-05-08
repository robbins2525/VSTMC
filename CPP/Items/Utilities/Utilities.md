# Utilities Implementation Guide

## Overview

Provides a collection of shared helper functions and common logic used across the add-in. It centralizes reusable operations that are not tied to a specific tool, service, or domain, reducing duplication and promoting consistency in implementation.

---
### Single Utilities per Project

If used, there is only one `Utilities.h` and `Utilities.cpp` necessary per project.  
The name cannot be changed in the **Add Item** dialog box.

Attempting to add another utilities will result in an error such as:

> *"Utilities already exists in the project."*
---