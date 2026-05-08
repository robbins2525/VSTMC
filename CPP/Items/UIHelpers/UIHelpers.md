# UIHelper Implementation Guide

## Overview

Provides utility functions for common user interface interactions within MicroStation, such as message display, prompts, and dialog-related helpers, to ensure consistent UI behavior across commands.

---
## Purpose

**UIHelper** exists to standardize how the project communicates with the user and with the developer during runtime.

Instead of calling `mdlOutput_messageCenter`, `MessageBoxW`, or ad hoc conversion code directly in multiple files, the project uses a single helper surface for common UI messaging tasks.

This gives the project:

- a consistent user experience
- cleaner command and tool code
- fewer repeated UI calls
- a single place to adjust UI behavior later
- a predictable pattern for future contributors

In practical terms, `UIHelper` is the project’s front desk. Commands bring a message, and the helper decides how that message gets presented.

---
### Single UIHelper per Project

If used, there is only one `UIHelper.h` necessary per project.  
The name cannot be changed in the **Add Item** dialog box.

Attempting to add another UIHelper with the same name will result in an error such as:

> *"UIHelper already exists in the project."*
---