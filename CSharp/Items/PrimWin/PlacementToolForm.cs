/*--------------------------------------------------------------------------------------+
|   $safeitemname$.cs
|
+--------------------------------------------------------------------------------------*/

#nullable enable

#region Bentley Namespaces
using BDPN = Bentley.DgnPlatformNET;
using BMW = Bentley.MstnPlatformNET.WinForms;
#endregion

namespace $rootnamespace$
{
    internal class $safeitemname$ : BDPN.DgnPrimitiveTool
    {
        // Use _toolSettingsForm.Property, _toolSettingsForm.Method(), or
        // _toolSettingsForm.Field to access public properties, methods, or
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

public $safeitemname$(int toolName, int toolPrompt)
            : base(toolName, toolPrompt)
        {
    _toolSettingsAdapter = new MyToolSettings();
}

#region PrimitiveTool Members

protected override bool OnDataButton(BDPN.DgnButtonEvent ev)
{
    return true;
}

protected override void OnRestartTool()
{
    InstallNewInstance();
}

protected override void OnCleanup()
{
    if (_toolSettingsForm != null)
    {
        _toolSettingsForm.DetachFromMicroStation();
        _toolSettingsForm.Dispose();
        _toolSettingsForm = null!;
    }

    _commandInstance = null!;

    base.OnCleanup();
}

protected override bool OnResetButton(BDPN.DgnButtonEvent ev)
{
    ExitTool();
    return true;
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
    _ = unparsed; // Avoid compiler warning for unused parameter.

    if (_commandInstance == null)
    {
        _commandInstance = new $safeitemname$(0, 0);
        _commandInstance.InstallTool();

        _toolSettingsForm?.AttachToToolSettings(Program.Instance);
        _toolSettingsForm?.Text = "$safeitemname$ Tool Settings";
    }
    else
    {
        _toolSettingsForm?.Focus();
    }
}

        #endregion
    }
}