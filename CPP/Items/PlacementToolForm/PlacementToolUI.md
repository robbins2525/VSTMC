# DgnPrimitiveTool Placement Tool Implementation Guide ($safeitemname$)

## Overview

Defines a tool framework for placing elements within the MicroStation / OpenRoads environment. It manages user input, dynamics, and element creation, providing a structured pattern for interactive placement commands.

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
### Add handler declaration `Commands/KeyinHandlers.h`

```c
static void $safeitemname$(Bentley::WCharCP unparsed);
```
---
### Implement handler `Commands/KeyinHandlers.cpp`

- Include the `$safeitemname$.h` header in KeyinHandlers.cpp.
```c
#include <$safeitemname$.h>
```
- Add the tool launch call in the handler implementation:

```cpp
void KeyinHandlers::$safeitemname$(Bentley::WCharCP unparsed)
{
    UNUSED(unparsed);
    start$safeitemname$(unparsed);
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