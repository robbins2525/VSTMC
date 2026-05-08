#include "$safeitemname$.h"

$safeitemname$::$safeitemname$()
    : DgnElementSetTool()
{
}

void $safeitemname$::_GetToolName(WStringR name)
{
    name = L"Select Element";  //Replace with your tool name
}

void $safeitemname$::InstallNewInstance()
{
    $safeitemname$* tool = new $safeitemname$();
    tool->InstallTool();
}

void $safeitemname$::_OnPostInstall()
{
    DgnElementSetTool::_OnPostInstall();
    NotificationManager::OutputPrompt(L"Select element(s). Data point to accept, reset to exit."); // Replace with your tool prompt
}

bool $safeitemname$::_OnResetButton(DgnButtonEventCR ev)
{
    (void)ev;
    _ExitTool();
    return true;
}

StatusInt $safeitemname$::_OnElementModify(EditElementHandleR eeh)
{
    //Replace with your logic

    if (!eeh.IsValid())
        return ERROR;

    ElementId elemId = eeh.GetElementId();

    WString msg;
    msg.Sprintf(L"Selected ElementId: %llu",
        static_cast<unsigned long long>(elemId));

    NotificationManager::OutputPrompt(msg.c_str());

    return SUCCESS;
}

void $safeitemname$::_OnRestartTool()
{
    InstallNewInstance();
}