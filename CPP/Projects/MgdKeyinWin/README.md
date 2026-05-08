# $safeprojectname$ starter

This updated starter adds:

- Bentley-style managed key-ins through `CommandTable.xml`
- a short alias root (`MKA`) for simpler testing
- key-in handler plumbing in `KeyinCommands.cpp`
- a reusable modeless WinForms UI in `MainForm.*`
- reopen/hide/about behavior that can be triggered from key-ins
- Visual Studio template packaging files in the `VS-Template` folder

## Test key-ins

- `mdl load $safeprojectname$`
- `$safeprojectname$ OPEN`
- `$safeprojectname$ HIDE`
- `MKA OPEN`
- `MKA ABOUT`

## Notes

The project now embeds `CommandTable.xml` directly. Bentley's documentation notes that the logical name must be `CommandTable.xml` for the command table to load correctly.


This starter now uses Bentley WinForms docking so the main UI can dock inside MicroStation.
