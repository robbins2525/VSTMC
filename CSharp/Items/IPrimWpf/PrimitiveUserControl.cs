/*--------------------------------------------------------------------------------------+
|   $safeitemname$.cs
|
+--------------------------------------------------------------------------------------*/

#nullable enable

#region Bentley Namespaces
using BCOM = Bentley.Interop.MicroStationDGN;
using BMI = Bentley.MstnPlatformNET.InteropServices;
using BMW = Bentley.MstnPlatformNET.WPF;

#endregion

namespace $rootnamespace$
{
    internal class $safeitemname$ : BCOM.IPrimitiveCommandEvents
    {
        //Use _userControl.Property, _userControl.Method(), or _userControl.Field
        //to access public properties, methods, or fields in $safeitemname$.xaml.cs.

    #region ToolSettings
        private static $safeitemname$? _commandInstance;
        private static BMW.ToolSettingsHost? _toolSettingsHost;
private static $safeitemname$UC? _userControl;
private readonly MyToolSettings _toolSettingsHostReference;

private sealed class MyToolSettings : BMW.ToolSettingsHost
{
    public MyToolSettings()
    {
        _userControl = new $safeitemname$UC();
        Content = _userControl;
        Title = "$safeitemname$ Tool Settings";
    }
}
    #endregion

public $safeitemname$()
        {
            _toolSettingsHostReference = new MyToolSettings();
_toolSettingsHost = _toolSettingsHostReference;
        }

#region IPrimitiveCommandEvents Members

public void Start()
{
    _toolSettingsHost?.Attach(Program.Instance);
}

public void Cleanup()
{
    if (_toolSettingsHost != null)
    {
        _toolSettingsHost.Detach();
        _toolSettingsHost.Dispose();
        _toolSettingsHost = null!;
    }

    _userControl = null!;
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
/// Open $safeitemname$ WPF user control.
/// </summary>
/// <param name="unparsed"></param>
public void Run(string unparsed = "")
{
    _ = unparsed; // Avoid unused parameter warning.

    if (_commandInstance == null)
    {
        _commandInstance = new $safeitemname$();
        BMI.Utilities.ComApp.CommandState.StartPrimitive(_commandInstance, false);
    }
    else
    {
        _toolSettingsHost?.Focus();
    }
}
    }
}