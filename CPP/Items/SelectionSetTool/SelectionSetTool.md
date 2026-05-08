# SelectionSet Processing Tool Implementation Guide ($safeitemname$)

## Overview 

Encapsulates access to and manipulation of the active selection set in MicroStation. It provides utilities for reading, modifying, and iterating selected elements.

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
    $safeitemname$::Run();
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
### Process elements in the selection set agenda `$safeitemname$.cpp`
```cpp
for (uint32_t i = 0; i < agenda.GetCount(); ++i)
{
    ElemAgendaEntry const& eeh = agenda[i];
    if (!eeh.IsValid())
        continue;

    // Process element here
}
```
#### see [Process element examples](#process-element-examples) below for common patterns.
---
## Element Processing Examples

- Report element IDs
- Filter by element type
- Extract model/file info
- Modify elements
- Apply business rules

---
## Process element examples

### Branch by element type
```cpp
static void ProcessElement(ElemAgendaEntry const& eeh)
    for (uint32_t i = 0; i < agenda.GetCount(); ++i)
    {
        ElemAgendaEntry const& eeh = agenda[i];
        if (!eeh.IsValid())
            continue;

        MSElementCP el = eeh.GetElementCP();
        if (nullptr == el)
            continue;

        switch (el->hdr.dhdr.props.b.type)
        {
        case LINE_ELM:
            NotificationManager::OutputPrompt(L"Found a line.");
            break;

        case SHAPE_ELM:
            NotificationManager::OutputPrompt(L"Found a shape.");
            break;

        case ELLIPSE_ELM:
            NotificationManager::OutputPrompt(L"Found an ellipse/arc.");
            break;

        default:
            NotificationManager::OutputPrompt(L"Found another element type.");
            break;
        }
    }
}
```