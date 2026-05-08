/*--------------------------------------------------------------------------------------+
|   $safeitemname$.cs
|
+--------------------------------------------------------------------------------------*/

#region Bentley Namespaces
using BDPN = Bentley.DgnPlatformNET;
using BG = Bentley.GeometryNET;
using BM = Bentley.MstnPlatformNET;
#endregion

namespace $rootnamespace$
{
    class $safeitemname$(int toolName, int toolPrompt) : BDPN.DgnPrimitiveTool(toolName, toolPrompt)
    {
    //public $safeitemname$(int toolName, int toolPrompt) : base(toolName, toolPrompt) { }

#region DgnPrimitiveTool Members
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

public static void InstallNewInstance(string unparsed = "")
        {
            _ = unparsed;
            $safeitemname$ tool = new(0, 0);
            tool.InstallTool();
        }
        #endregion
    }
}
