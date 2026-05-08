# Selection Tool Implementation Guide ($safeitemname$)

## Purpose

An interactive tool for selecting elements within the model. It handles user input, hit detection, and selection logic, providing a structured pattern for implementing custom selection workflows.

---
## Cammand Table Implementation
### Add a command ID for the `$safeitemname$` selection set tool in `Resource Files/$rootnamespace$cmd.r`.
```c
#define CMD_$rootnamespace_UPPER$_$safeitemname_UPPER$  n
```
---
#### where *n* is the next available command ID number in the project. Command IDs must be unique within the project and typically start from 10.
### Add the key-in to the command table

```c
{ n, CMD_$rootnamespace_UPPER$_$safeitemname_UPPER$, INHERIT, NONE, "$safeitemname_UPPER$" }
```
---
#### where *n* is the next available Keyin number for a command table in the project.
#### Command numbering best practices
- Command Tables : 0–9
- Commands       : 10+
#### This avoids collisions where MicroStation interprets a command number as another command table, which can cause recursive key-in levels.
---
### Add handler declaration `Command/KeyinHandlers.h`

```c
static void $safeitemname$(Bentley::WCharCP unparsed);
```
---
### Implement handler `Command/KeyinHandlers.cpp`

- Include the `$safeitemname$.h` header in KeyinHandlers.cpp.
```c
#include <$safeitemname$.h>
```
- Add the tool launch call in the handler implementation:

```cpp
void KeyinHandlers::$safeitemname$(Bentley::WCharCP unparsed)
{
    UNUSED(unparsed);
    $safeitemname$::InstallNewInstance();
}
```
---
### Register command `Platform/BentleyApi.cpp`

Add a wrapper function for the new command:

```cpp
extern "C" void cmd_$safeitemname$(WCharCP unparsed)
{
    Commands::KeyinHandlers::$safeitemname$(unparsed);
}
```
Then add it to the command registration array:
```CPP
{ (CmdHandler)cmd_$safeitemname$, CMD_$rootnamespace_UPPER$_$safeitemname_UPPER$ },

```
---
### Expected behavior

- the prompt appears
- the user clicks one element
- the tool processes that one element
- reset exits the tool, unless restart behavior is intentionally implemented

---

## Why this is documented as single-selection

Although the tool can remain active across repeated picks, the current implementation handles only **one accepted element at a time** through `_OnElementModify(EditElementHandleR eeh)`.

That means:

- no collected selection set
- no multi-element batch processing
- no drag window selection behavior in the current implementation

This is why the tool should be described to developers as a **single-selection tool**.

---

