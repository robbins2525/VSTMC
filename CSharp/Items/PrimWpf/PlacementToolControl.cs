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
    class $safeitemname$ : BDPN.DgnPrimitiveTool
    {
    //Use _userControl.Property, _userControl.Method(), or _userControl.Field
    //to access public properties, methods, or fields in $safeitemname$.xaml.cs.

     #region ToolSettings
        private static $safeitemname$? _commandInstance;
private static BMW.ToolSettingsHost? _toolSettingsHost;
private static $safeitemname$UC? _userControl;

class MyToolSettings : BMW.ToolSettingsHost
{
    private $safeitemname$ m_command;

    public MyToolSettings($safeitemname$ command)
    {
        m_command = command;

        _userControl = new $safeitemname$UC();
        this.Content = _userControl;
        this.Title = "$safeitemname$ Tool Settings";
    }
};
#endregion

#region DgnPrimitiveTool members
public $safeitemname$(int toolName, int toolPrompt) : base(toolName, toolPrompt)
    {
        _toolSettingsHost = new MyToolSettings(this);
    }

        protected override bool OnDataButton(BDPN.DgnButtonEvent ev)
{

    return true;
}

protected override void OnRestartTool()
{
    InstallNewInstance();
}

protected override bool OnResetButton(BDPN.DgnButtonEvent ev)
{
    ExitTool();
    return true;
}

protected override void OnCleanup()
{
    if (null != _commandInstance)
    {
        _toolSettingsHost?.Detach();
        _toolSettingsHost?.Dispose();
        _toolSettingsHost = null!;
        _commandInstance = null!;
    }
    base.OnCleanup();
}

protected override void ExitTool()
{

    base.ExitTool();
}

protected override void OnDynamicFrame(BDPN.DgnButtonEvent ev)
{

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
    if (null == _commandInstance)
    {
        _commandInstance = new $safeitemname$(0, 0);
        _commandInstance.InstallTool();
        _toolSettingsHost?.Attach(Program.Instance);
    }
    else
        _toolSettingsHost?.Focus();
}
        #endregion
    }
}