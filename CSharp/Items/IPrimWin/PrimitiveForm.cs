/*--------------------------------------------------------------------------------------+
|   $safeitemname$.cs
|
+--------------------------------------------------------------------------------------*/

#nullable enable

#region Bentley Namespaces
using BCOM = Bentley.Interop.MicroStationDGN;
using BMI = Bentley.MstnPlatformNET.InteropServices;
using BMW = Bentley.MstnPlatformNET.WinForms;
#endregion

namespace $rootnamespace$
{
    internal class $safeitemname$ : BCOM.IPrimitiveCommandEvents
    {
        // Use $safeitemname$MSForm.Property, $safeitemname$MSForm.Method(), or
        // $safeitemname$MSForm.Field to access public properties, methods, or
        // fields in $safeitemname$Form.cs.

    #region ToolSettings
        private static $safeitemname$? _commandInstance;
        private static $safeitemname$Form? _toolSettingsForm;
private readonly BMW.Adapter? _toolSettingsAdapter;

private sealed class MyToolSettings : BMW.Adapter
{
    public MyToolSettings()
    {
        _toolSettingsForm = new $safeitemname$Form();
    }
}
    #endregion

public $safeitemname$()
        {
            _toolSettingsAdapter = new MyToolSettings();
}

#region IPrimitiveCommandEvents Members

public void Start()
{
    _toolSettingsForm?.AttachToToolSettings(Program.Instance);
    _toolSettingsForm?.Text = "$safeitemname$ Tool Settings";
}

public void Cleanup()
{
    if (_toolSettingsForm != null)
    {
        _toolSettingsForm.DetachFromMicroStation();
        _toolSettingsForm.Dispose();
        _toolSettingsForm = null!;
    }

    _commandInstance = null!;
}

public void DataPoint(ref BCOM.Point3d Point, BCOM.View View)
{
}

public void Dynamics(ref BCOM.Point3d Point, BCOM.View View, BCOM.MsdDrawingMode DrawMode)
{
}

public void Keyin(string Keyin)
{
}

public void Reset()
{
    BMI.Utilities.ComApp.CommandState.StartDefaultCommand();
}

#endregion

/// <summary>
/// Open $safeitemname$ Form.
/// </summary>
/// <param name="unparsed"></param>
public void Run(string unparsed = "")
{
    _ = unparsed; // Avoid compiler warning for unused parameter.

    if (_commandInstance == null)
    {
        _commandInstance = new $safeitemname$();
        BMI.Utilities.ComApp.CommandState.StartPrimitive(_commandInstance, false);
    }
    else
    {
        _toolSettingsForm?.Focus();
    }
}
    }
}