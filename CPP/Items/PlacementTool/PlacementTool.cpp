#include <Mstn/MdlApi/MdlApi.h>
#include <DgnPlatform/DgnPlatformApi.h>
#include <DgnPlatform/LinearHandlers.h>
#include <DgnView/DgnElementSetTool.h>

USING_NAMESPACE_BENTLEY_DGNPLATFORM
USING_NAMESPACE_BENTLEY_MSTNPLATFORM
USING_NAMESPACE_BENTLEY_MSTNPLATFORM_ELEMENT

struct $safeitemname$ : DgnPrimitiveTool
{
private:
    DPoint3d m_startPoint;
    bool     m_haveStart = false;

public:
    $safeitemname$(int toolId, int promptId)
        : DgnPrimitiveTool(toolId, promptId),
        m_startPoint(),
        m_haveStart(false)
    {
    }

    void _GetToolName(WStringR name) override
    {
        name = L"Place Line"; //Replace with your tool name
    }

    static void InstallNewInstance(int toolId, int promptId)
    {
        $safeitemname$* pTool = new $safeitemname$(toolId, promptId);
        pTool->InstallTool();
    }

protected:
    virtual void _OnPostInstall() override
    {
        AccuSnap::GetInstance().EnableSnap(true);
        __super::_OnPostInstall();

        NotificationManager::OutputPrompt(L"Identify start of line"); // Replace with your tool prompt
    }

    virtual void _OnRestartTool() override
    {
        $safeitemname$* pTool = new $safeitemname$(GetToolId(), GetToolPrompt());
        pTool->InstallTool();
    }

    virtual bool _OnDataButton(DgnButtonEventCR ev) override
    {
        if (!m_haveStart)
        {
            m_startPoint = *ev.GetPoint();
            m_haveStart = true;
            _BeginDynamics();
            NotificationManager::OutputPrompt(L"Identify end of line"); //Replace with your tool prompt
            return true;
        }

        EditElementHandle eeh;
        if ($safeitemname$Element(eeh, m_startPoint, *ev.GetPoint()))
            eeh.AddToModel();

        m_haveStart = false;
        _OnReinitialize();
        return true;
    }

    virtual void _OnDynamicFrame(DgnButtonEventCR ev) override
    {
        if (!m_haveStart)
            return;

        EditElementHandle eeh;
        if (!$safeitemname$Element(eeh, m_startPoint, *ev.GetPoint()))
            return;

        RedrawElems redrawElems;
        redrawElems.SetDynamicsViews(IViewManager::GetActiveViewSet(), ev.GetViewport());
        redrawElems.SetDrawMode(DRAW_MODE_TempDraw);
        redrawElems.SetDrawPurpose(DrawPurpose::Dynamics);
        redrawElems.DoRedraw(eeh);
    }

    virtual bool _OnResetButton(DgnButtonEventCR) override
    {
        m_haveStart = false;
        _EndDynamics();
        _ExitTool();
        return true;
    }

private:
    bool $safeitemname$Element(EditElementHandleR eeh, DPoint3dCR startPt, DPoint3dCR endPt)
    {
        DSegment3d seg;
        seg.point[0] = startPt;
        seg.point[1] = endPt;

        DgnModelRefP modelRef = ACTIVEMODEL;
        if (nullptr == modelRef)
            return false;

        BentleyStatus status = LineHandler::CreateLineElement(
            eeh,
            nullptr,
            seg,
            modelRef->Is3d(),
            *modelRef
        );

        return (SUCCESS == status);
    }
};

extern "C" void start$safeitemname$(WCharCP)
{
    $safeitemname$::InstallNewInstance(0, 0);
}