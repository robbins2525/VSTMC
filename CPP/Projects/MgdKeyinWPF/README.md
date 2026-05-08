# $safeprojectname$ WPF starter

This starter demonstrates a Bentley managed add-in with:

- `CommandTable.xml` command registration
- Bentley-style key-ins and short alias key-ins
- reopenable dockable UI
- WPF content hosted inside a WinForms `ElementHost`
- docking through `Bentley.Windowing.WindowManager.DockPanel(...)`

## Included key-ins

- `$safeprojectname$ OPEN`
- `$safeprojectname$ SHOW`
- `$safeprojectname$ HIDE`
- `$safeprojectname$ ABOUT`
- `MKA OPEN`
- `MKA SHOW`
- `MKA HIDE`
- `MKA ABOUT`

## Why this WPF starter uses ElementHost

Pure XAML build tooling inside Bentley's managed C++ add-in template can be finicky. This starter keeps the project shape close to the working C++/CLI template by:

1. building the WPF tool in code as a `System::Windows::Controls::UserControl`
2. hosting it in `System::Windows::Forms::Integration::ElementHost`
3. docking the containing form with `WindowManager.DockPanel(...)`

That gives you a real WPF UI surface without depending on a XAML build pipeline in the template.

## Notes

- Target framework is `.NET Framework 4.8`
- References include `PresentationCore`, `PresentationFramework`, `WindowsBase`, and `WindowsFormsIntegration`
- Update the AddIn task ID, dock ID, and key-in strings before production use
