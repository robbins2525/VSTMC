/*--------------------------------------------------------------------------------------+
|   $safeitemname$.cs
|
+--------------------------------------------------------------------------------------*/

#nullable enable

#region Bentley Namespaces
using BDPN = Bentley.DgnPlatformNET;
using BG = Bentley.GeometryNET;
using BM = Bentley.MstnPlatformNET;
using BMW = Bentley.MstnPlatformNET.WPF;

#endregion

namespace $rootnamespace$
{
    internal class $safeitemname$ : BDPN.DgnElementSetTool
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

public $safeitemname$(int toolID, int toolName) : base()
        {
            _ = toolID;
            _ = toolName;

            _toolSettingsHostReference = new MyToolSettings();
_toolSettingsHost = _toolSettingsHostReference;
        }

#region DgnElementSetTool Members

protected override void OnRestartTool()
{
    InstallNewInstance();
}

protected override void OnCleanup()
{
    if (_toolSettingsHost != null)
    {
        _toolSettingsHost.Detach();
        _toolSettingsHost.Dispose();
        _toolSettingsHost = null!;
    }

    _userControl = null!;
    _commandInstance = null!;

    base.OnCleanup();
}

public override BDPN.StatusInt OnElementModify(BDPN.Elements.Element element)
{
    _ = element;
    return BDPN.StatusInt.Success;
}

protected override bool OnResetButton(BDPN.DgnButtonEvent ev)
{
    _ = ev;
    ExitTool();
    return true;
}

protected override void ExitTool()
{
    base.ExitTool();
}

protected override void OnDynamicFrame(BDPN.DgnButtonEvent ev)
{
    _ = ev;
}

protected override bool OnInstall()
{
    return true;
}

protected override void OnPostInstall()
{
    base.OnPostInstall();
}

public void InstallNewInstance(string unparsed = "")
{
    _ = unparsed; // Avoid unused parameter warning.

    if (_commandInstance == null)
    {
        _commandInstance = new $safeitemname$(0, 0);
        _commandInstance.InstallTool();
        _toolSettingsHost?.Attach(Program.Instance);
    }
    else
    {
        _toolSettingsHost?.Focus();
    }
}

        #endregion
    }
}