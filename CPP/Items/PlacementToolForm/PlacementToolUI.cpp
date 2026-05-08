#include "$safeitemname$.h"

#include <Mstn\MdlApi\MdlApi.h>
#include <Mstn\ISessionMgr.h>

#include <DgnPlatform\LinearHandlers.h>
#include <DgnView\DgnElementSetTool.h>

USING_NAMESPACE_BENTLEY_DGNPLATFORM
USING_NAMESPACE_BENTLEY_MSTNPLATFORM
USING_NAMESPACE_BENTLEY_MSTNPLATFORM_ELEMENT

extern UInt32 g_lineColor;
extern UInt32 GetSelectedLineColor();
extern int GetSelectedLineWeight();
extern int GetSelectedLineStyle();

extern MSDialogP g_lineLevelDialog;
extern int g_lineLevelItemIndex;

extern MSDialogP g_lineColorDialog;
extern int g_lineColorItemIndex;
extern MSDialogP g_lineColorDialog;

extern bool g_useByLevel;

namespace
{
    class $safeitemname$ : public DgnPrimitiveTool
    {
    public:
        static void InstallNewInstance(int toolId, int promptId)
        {
            (new $safeitemname$(toolId, promptId))->InstallTool();
        }

    private:
        $safeitemname$(int toolId, int promptId);

        void _GetToolName(WStringR name) override;
        void _OnPostInstall() override;
        void _OnRestartTool() override;
        bool _OnDataButton(DgnButtonEventCR ev) override;
        void _OnDynamicFrame(DgnButtonEventCR ev) override;
        bool _OnResetButton(DgnButtonEventCR) override;
        bool CreateLineElement(EditElementHandleR eeh, DPoint3dCR startPt, DPoint3dCR endPt);

    private:
        DPoint3d m_startPoint;
        bool m_haveStart;
        UInt32 m_lineColor;
    };

    $safeitemname$::$safeitemname$(int toolId, int promptId)
        : DgnPrimitiveTool(toolId, promptId),
        m_startPoint(),
        m_haveStart(false),
        m_lineColor(2)
    {
    }

    void $safeitemname$::_GetToolName(WStringR name)
    {
        name = L"Place Line";
    }

    void $safeitemname$::_OnPostInstall()
    {
        AccuSnap::GetInstance().EnableSnap(true);
        __super::_OnPostInstall();
        NotificationManager::OutputPrompt(L"Identify start of line");
    }

    void $safeitemname$::_OnRestartTool()
    {
        InstallNewInstance(GetToolId(), GetToolPrompt());
    }

    bool $safeitemname$::_OnDataButton(DgnButtonEventCR ev)
    {
        if (!m_haveStart)
        {
            m_startPoint = *ev.GetPoint();
            m_haveStart = true;
            _BeginDynamics();
            NotificationManager::OutputPrompt(L"Identify end of line");
            return true;
        }

        EditElementHandle eeh;
        if (CreateLineElement(eeh, m_startPoint, *ev.GetPoint()))
            eeh.AddToModel();

        m_haveStart = false;
        _EndDynamics();
        _OnReinitialize();
        return true;
    }

    void $safeitemname$::_OnDynamicFrame(DgnButtonEventCR ev)
    {
        if (!m_haveStart)
            return;

        EditElementHandle eeh;
        if (!CreateLineElement(eeh, m_startPoint, *ev.GetPoint()))
            return;

        RedrawElems redrawElems;
        redrawElems.SetDynamicsViews(IViewManager::GetActiveViewSet(), ev.GetViewport());
        redrawElems.SetDrawMode(DRAW_MODE_TempDraw);
        redrawElems.SetDrawPurpose(DrawPurpose::Dynamics);
        redrawElems.DoRedraw(eeh);
    }

    bool $safeitemname$::_OnResetButton(DgnButtonEventCR)
    {
        m_haveStart = false;
        _EndDynamics();
        _ExitTool();
        return true;
    }

    bool $safeitemname$::CreateLineElement(EditElementHandleR eeh, DPoint3dCR startPt, DPoint3dCR endPt)
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
            *modelRef);

        if (SUCCESS != status)
            return false;

        UInt32 color = g_useByLevel ? COLOR_BYLEVEL : GetSelectedLineColor();
        UInt32 weight = g_useByLevel ? WEIGHT_BYLEVEL : (UInt32)GetSelectedLineWeight();
        UInt32 style = g_useByLevel ? STYLE_BYLEVEL : (UInt32)GetSelectedLineStyle();

        WString levelName;
        if (nullptr != g_lineLevelDialog && g_lineLevelItemIndex >= 0)
            mdlDialog_itemGetStringValue(levelName, g_lineLevelDialog, g_lineLevelItemIndex);

        FileLevelCacheR levelCache = ISessionMgr::GetActiveDgnFile()->GetLevelCacheR();
        LevelHandle levelHandle = levelCache.GetLevelByName(levelName.GetWCharCP());

        ElementPropertiesSetterPtr setter = ElementPropertiesSetter::Create();
        if (!setter.IsValid())
            return false;

        if (levelHandle.IsValid())
            setter->SetLevel(levelHandle.GetLevelId());

        setter->SetColor(color);
        setter->SetWeight(weight);
        setter->SetLinestyle(style, nullptr);

        setter->Apply(eeh);

        return true;
    }
}

namespace Tools
{
    void Start$safeitemname$(Bentley::WCharCP unparsed)
    {
        (void)unparsed; // Reserved for future key-in arguments
        $safeitemname$::InstallNewInstance(CMDNAME_$safeitemname$, PROMPT_$safeitemname$);
    }
}