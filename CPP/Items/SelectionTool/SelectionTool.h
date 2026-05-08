#pragma once

#include <DgnView/DgnElementSetTool.h>
#include <DgnPlatform/ElementHandle.h>
#include <DgnPlatform/NotificationManager.h>
#include <Mstn/MstnDefs.h>

USING_NAMESPACE_BENTLEY
USING_NAMESPACE_BENTLEY_DGNPLATFORM
USING_NAMESPACE_BENTLEY_MSTNPLATFORM

class $safeitemname$ : public DgnElementSetTool
{
private:
    $safeitemname$();

protected:
    void _GetToolName(WStringR name) override;
    virtual void _OnPostInstall() override;
    virtual bool _OnResetButton(DgnButtonEventCR ev) override;
    virtual StatusInt _OnElementModify(EditElementHandleR eeh) override;
    virtual void _OnRestartTool() override;

public:
    static void InstallNewInstance();
};