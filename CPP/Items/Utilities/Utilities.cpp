//////////////////////////////////////////////////////////////////////
//
// Utilities.cpp
//
// Portions of this utility library were inspired by or derived from
// MicroStation utility code published by Jon Summers, Bentley MVP,
// Director of LA Solutions (http://www.la-solutions.co.uk/).
//
// The code has since been adapted, refactored, and updated for
// modern C++ and use within this project.
//
//////////////////////////////////////////////////////////////////////

#pragma warning (push)
#pragma warning (disable: 4635)
#include "Utilities.h"
#include <DgnPlatform/NotificationManager.h>
#include <DgnPlatform/TCB/tcb.r.h>
#include <Bentley/ValueFormat.r.h>
#include <DgnPlatform/DgnFileIO/ModelInfo.h>
#pragma warning( pop )

#if __has_include("ModulVer.h")
#include "ModulVer.h"
#define UTILITIES_HAVE_MODULVER 1
#endif

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#define new DEBUG_NEW
#endif

#include <Declare_ustnTaskId_Import.h>

MicroStation::WindowCursor::WindowCursor(SYSTEMCURSOR cursor) noexcept
{
    mdlWindow_setSystemCursor(cursor);
}

MicroStation::WindowCursor::~WindowCursor() noexcept
{
    mdlWindow_setCursorPrevious();
}

MicroStation::BusyCursor::BusyCursor() noexcept
{
    mdlSystem_startBusyCursor();
}

MicroStation::BusyCursor::~BusyCursor() noexcept
{
    mdlSystem_stopBusyCursor();
}

MicroStation::BusyBar::BusyBar(wchar_t const* title, wchar_t const* msg)
{
    enum { SlidingRectangles, SlidingParallelograms, };
    mdlDialog_busyBarStartProcessing(nullptr, nullptr, nullptr, nullptr, nullptr, SlidingRectangles, msg, title);
}

void MicroStation::BusyBar::UpdateMessage(wchar_t const* msg)
{
    mdlDialog_busyBarUpdateMessage(msg);
}

MicroStation::BusyBar::~BusyBar() noexcept
{
    mdlDialog_busyBarStopProcessing();
}

UInt32 MicroStation::ElementCount(DgnModelRefP modelRef)
{
    _ASSERTE(nullptr != modelRef && "MicroStation::ElementCount modelRef is nullptr");
    return modelRef->GetDgnModelP()->GetElementCount(Bentley::DgnPlatform::DgnModelSections::GraphicElements);
}

MicroStation::SymbologyExt::SymbologyExt(const Bentley::RgbColorDef& colour,
    double transparency,
    UInt32 weight,
    Int32 style,
    Int32 priority)
    : fillColour_(colour),
    fillTransparency_(transparency),
    lineColour_(colour),
    lineTransparency_(transparency),
    lineThickness_(weight),
    lineStyle_(style),
    priority_(priority)
{
}

MicroStation::SymbologyExt::SymbologyExt(const Bentley::RgbColorDef& fillColour,
    double fillTransparency,
    const Bentley::RgbColorDef& lineColour,
    double lineTransparency,
    UInt32 weight,
    Int32 style,
    Int32 priority)
    : fillColour_(fillColour),
    fillTransparency_(fillTransparency),
    lineColour_(lineColour),
    lineTransparency_(lineTransparency),
    lineThickness_(weight),
    lineStyle_(style),
    priority_(priority)
{
}

bool MicroStation::PublishComplexVariable(const void* data, char const* tagName, char const* variableName)
{
    bool rc{ false };
    SymbolSet* symbolSet{ mdlCExpression_initializeSet(VISIBILITY_DIALOG_BOX, 0, FALSE) };
    if (nullptr != symbolSet)
    {
        if (SUCCESS == mdlDialog_publishComplexVariable(symbolSet,
            const_cast<char*>(tagName),
            const_cast<char*>(variableName),
            const_cast<void*>(data)))
        {
            rc = true;
        }
    }

    if (!rc)
    {
        enum StringControl { BufferLength = 256, MdlTaskNameLength = 22, };
        WChar currTaskID[MdlTaskNameLength];
        wcscpy_s(currTaskID, MdlTaskNameLength, mdlSystem_getCurrTaskID());
        WChar msg[BufferLength];
        swprintf_s(msg, BufferLength, L"%s: mdlDialog_publishComplexVariable () failed", currTaskID);
        mdlOutput_messageCenter(Bentley::DgnPlatform::OutputMessagePriority::Debug, msg, msg, DgnPlatform::OutputMessageAlert::Dialog);
    }

    return rc;
}

bool MicroStation::DefaultModelIs3D(Bentley::WCharCP filePath)
{
    DgnPlatform::DgnFileFormatType format(DgnPlatform::DgnFileFormatType::Invalid);
    int major(0);
    int minor(0);
    bool defaultModelIs3D(false);
    int imageSize(0);
    mdlSystem_openFileThumbnail(&format, &major, &minor, &defaultModelIs3D, filePath, nullptr, &imageSize);
    return DgnPlatform::DgnFileFormatType::Invalid != format && defaultModelIs3D;
}

Bentley::DgnPlatform::DgnFileFormatType MicroStation::FileFormat(Bentley::WCharCP filePath, bool* is3D)
{
    Bentley::DgnPlatform::DgnFileFormatType dgnFormat{ Bentley::DgnPlatform::DgnFileFormatType::Invalid };
    Bentley::DgnPlatform::DgnFileFormatType check{ Bentley::DgnPlatform::DgnFileFormatType::Invalid };
    int major{ 0 };
    int minor{ 0 };
    bool defaultModelIs3D{ false };
    int imageSize{ 0 };
    if (mdlSystem_openFileThumbnail(&check, &major, &minor, &defaultModelIs3D, filePath, nullptr, &imageSize))
    {
        dgnFormat = check;
    }
    if (nullptr != is3D)
    {
        *is3D = defaultModelIs3D;
    }
    return dgnFormat;
}

#if defined (IMODEL_API)
bool MicroStation::IsIModel()
{
    USING_NAMESPACE_BENTLEY
        USING_NAMESPACE_BENTLEY_USTN
        ISessionMgrR pSessionMgr = ISessionMgr::GetManager();
    DgnModelRefP pModel = pSessionMgr.GetActiveModel();
    MSDgnFileP pFile = pModel->GetDgnFile();
    return pFile->IsIModel();
}
#endif

std::wstring MicroStation::DescribeElement(UInt32 filePos, DgnModelRefP modelRef)
{
    return DescribeElement(mdlModelRef_getElementRef(modelRef, filePos), modelRef);
}

std::wstring MicroStation::DescribeElement(ElementRefP elemRef, DgnModelRefP modelRef)
{
    Bentley::DgnPlatform::ElementHandle eh{ elemRef, modelRef };
    Bentley::DgnPlatform::Handler& handler{ eh.GetHandler() };
    Bentley::WString wcDescr;
    handler.GetDescription(eh, wcDescr, 128);
    return std::wstring(wcDescr.c_str());
}

std::wstring MicroStation::DescribeFileFormat(Bentley::DgnPlatform::DgnFileFormatType format)
{
    using namespace Bentley::DgnPlatform;
    std::wstring descr{ L"Unknown" };
    switch (format)
    {
    case DgnFileFormatType::Invalid: descr = L"Can't recognize file format"; break;
    case DgnFileFormatType::Current: descr = L"Match the current format"; break;
    case DgnFileFormatType::V8: descr = L"V8"; break;
    case DgnFileFormatType::V7: descr = L"V7"; break;
    case DgnFileFormatType::DWG: descr = L"DWG"; break;
    case DgnFileFormatType::DXF: descr = L"DXF"; break;
    }
    return descr;
}

#ifdef UTILITIES_HAVE_MODULVER
bool MicroStation::CanOpen(LPCWSTR filePath)
{
    bool valid{ false };
    Bentley::DgnPlatform::DgnFileFormatType format{ DgnPlatform::DgnFileFormatType::Invalid };
    int major{ 0 };
    int minor{ 0 };
    bool defaultModelIs3D{ FALSE };
    int imageSize{ 0 };

    if (mdlSystem_openFileThumbnail(&format, &major, &minor, &defaultModelIs3D, filePath, nullptr, &imageSize))
    {
        valid = (DgnPlatform::DgnFileFormatType::V8 == format
            || DgnPlatform::DgnFileFormatType::V7 == format
            || DgnPlatform::DgnFileFormatType::DWG == format
            || DgnPlatform::DgnFileFormatType::DXF == format);
    }
    return valid;
}
#endif

bool MicroStation::IsCellLibrary(DgnModelRefP modelRef)
{
    std::wstring file{ MicroStation::ModelFileName(modelRef) };
    wchar_t ext[MAXEXTENSIONLENGTH];
    mdlFile_parseName(file.c_str(), nullptr, nullptr, nullptr, ext);
    return (SUCCESS == _wcsicmp(ext, L"cel"));
}

Bentley::DgnPlatform::ElementId MicroStation::GetElementID(ElementHandleCR eh)
{
    return eh.GetElementId();
}

bool MicroStation::IsSameModel(DgnModelRefP modelRef, Bentley::WCharCP modelName)
{
    _ASSERTE(nullptr != modelRef && "MicroStation::IsSameModel: nullptr modelRef");
    _ASSERTE(nullptr != modelName && 0 < wcslen(modelName) && "MicroStation::IsSameModel: zero-length modelName");
    if ((nullptr != modelName) && 0 < wcslen(modelName))
        return (0 == _wcsicmp(modelName, ModelName(modelRef).c_str()));
    return false;
}

bool MicroStation::IsSameModel(DgnModelRefP modelRef, const std::wstring& modelName)
{
    return IsSameModel(modelRef, modelName.c_str());
}

bool MicroStation::IsSameModel(DgnModelRefP modelRef1, DgnModelRefP modelRef2)
{
    return mdlModelRef_areSame(modelRef1, modelRef2);
}

bool MicroStation::NewDesignFile(const std::wstring& filePath, const std::wstring& modelName, bool readOnly)
{
    return NewDesignFile(filePath.c_str(), modelName.empty() ? nullptr : modelName.c_str(), readOnly);
}

bool MicroStation::NewDesignFile(const wchar_t* filePath, const wchar_t* modelName, bool readOnly)
{
    wchar_t current[_MAX_PATH];
    mdlModelRef_getFileName(ACTIVEMODEL, current, _MAX_PATH);
    const DgnPlatform::FileCompareMask Mask{ DgnPlatform::FileCompareMask::BaseNameAndExtension };
    if (mdlFile_isSameFile(current, filePath, Mask) && IsSameModel(ACTIVEMODEL, modelName))
        return true;

    if (readOnly)
        mgds_modes.always_read_only = TRUE;
    if (modelName && wcslen(modelName))
        return SUCCESS == mdlSystem_newDesignFileAndModel(filePath, modelName);
    return SUCCESS == mdlSystem_newDesignFile(filePath);
}

std::wstring MicroStation::RefModelName(DgnModelRefP modelRef)
{
    const size_t StringLength{ Bentley::DgnPlatform::MAX_MODEL_NAME_LENGTH + 1 };
    _ASSERTE(mdlModelRef_isReference(modelRef) && "MicroStation::RefModelName not a reference");
    WChar modelName[StringLength];
    mdlRefFile_getStringParameters(modelName, StringLength, REFERENCE_MODELNAME, modelRef);
    return std::wstring(modelName);
}

std::wstring MicroStation::RefDescription(DgnModelRefP modelRef)
{
    _ASSERTE(mdlModelRef_isReference(modelRef) && "MicroStation::RefDescription not a reference");
    const size_t StringLength{ Bentley::DgnPlatform::MAX_MODEL_DESCR_LENGTH + 1 };
    WChar descr[StringLength];
    mdlRefFile_getStringParameters(descr, StringLength, REFERENCE_DESCRIPTION, modelRef);
    return std::wstring(descr);
}

std::wstring MicroStation::RefLogicalName(DgnModelRefP modelRef)
{
    _ASSERTE(mdlModelRef_isReference(modelRef) && "MicroStation::RefLogicalName not a reference");
    const size_t StringLength{ Bentley::DgnPlatform::MAX_REFLOGICALNAME + 1 };
    WChar logical[StringLength];
    mdlRefFile_getStringParameters(logical, StringLength, REFERENCE_LOGICAL, modelRef);
    return std::wstring(logical);
}

std::wstring MicroStation::RefAttachName(DgnModelRefP modelRef)
{
    _ASSERTE(mdlModelRef_isReference(modelRef) && "MicroStation::RefAttachName not a reference");
    const size_t StringLength{ Bentley::DgnPlatform::MAX_MODEL_NAME_LENGTH + 1 };
    WChar attach[StringLength];
    mdlRefFile_getStringParameters(attach, StringLength, REFERENCE_ATTACHNAME, modelRef);
    return std::wstring(attach);
}

std::wstring MicroStation::RefFileName(DgnModelRefP modelRef)
{
    _ASSERTE(mdlModelRef_isReference(modelRef) && "MicroStation::RefFileName not a reference");
    const size_t StringLength{ _MAX_PATH + 1 };
    WChar file[StringLength];
    mdlRefFile_getStringParameters(file, StringLength, REFERENCE_FILENAME, modelRef);
    return std::wstring(file);
}

std::wstring MicroStation::LevelName(DgnModelRefP modelRef, UInt32 levelId)
{
    std::wstring name{ L"" };
    wchar_t levelName[Bentley::DgnPlatform::MAX_LEVEL_NAME_LENGTH];
    if (0 != levelId && SUCCESS == mdlLevel_getName(levelName, Bentley::DgnPlatform::MAX_LEVEL_NAME_LENGTH, modelRef, levelId))
    {
        name = levelName;
    }
    else
    {
        enum StringControl { BufferLength = 256, };
        wchar_t msg[BufferLength];
        swprintf_s(msg, BufferLength, L"MicroStation::LevelName invalid level ID %d", levelId);
        mdlOutput_messageCenter(Bentley::DgnPlatform::OutputMessagePriority::Warning, msg, msg, Bentley::DgnPlatform::OutputMessageAlert::Dialog);
    }
    return name;
}

std::wstring MicroStation::LevelDescr(DgnModelRefP modelRef, UInt32 levelId)
{
    std::wstring level;
    WChar levelDescr[Bentley::DgnPlatform::MAX_LEVEL_DESCRIPTION_LENGTH];
    if (SUCCESS == mdlLevel_getDescription(levelDescr, Bentley::DgnPlatform::MAX_LEVEL_DESCRIPTION_LENGTH, modelRef, levelId))
    {
        level = levelDescr;
    }
    else
    {
        enum StringControl { BufferLength = 256, };
        WChar msg[BufferLength];
        swprintf_s(msg, BufferLength, L"MicroStation::LevelDescr invalid level ID %d", levelId);
        mdlOutput_messageCenter(Bentley::DgnPlatform::OutputMessagePriority::Error, msg, msg, Bentley::DgnPlatform::OutputMessageAlert::Dialog);
    }
    return level;
}

std::wstring MicroStation::ModelName(DgnModelRefP modelRef)
{
    std::wstring modelName;
    _ASSERTE(nullptr != modelRef);
    WChar name[Bentley::DgnPlatform::MAX_MODEL_NAME_LENGTH]{ 0 };
    if (SUCCESS == mdlModelRef_getModelName(modelRef, name))
        modelName = name;
    return modelName;
}

std::wstring MicroStation::ModelFileName(DgnModelRefP modelRef)
{
    _ASSERTE(nullptr != modelRef);
    WChar fileName[MAXQUOTEDFILELENGTH] = { 0 };
    mdlModelRef_getFileName(modelRef, fileName, MAXQUOTEDFILELENGTH);
    return std::wstring(fileName);
}

std::wstring MicroStation::MasterUnitName(DgnModelRefP modelRef)
{
    WChar label[12];
    mdlModelRef_getMasterUnitLabel(modelRef, label);
    return std::wstring(label);
}

std::wstring MicroStation::SubUnitName(DgnModelRefP modelRef)
{
    WChar label[12];
    mdlModelRef_getSubUnitLabel(modelRef, label);
    return std::wstring(label);
}

UInt32 MicroStation::ModelCount(DgnModelRefP modelRef)
{
    DgnFileP dgnFile = mdlModelRef_getDgnFile(modelRef);
    return mdlDgnFileObj_getModelCount(dgnFile);
}

UInt32 MicroStation::AddToModel(MSElementDescrP pElm, DgnModelRefP modelRef)
{
    UInt32 filePos{ mdlElmdscr_addByModelRef(pElm, modelRef) };
    if (0 < filePos)
        mdlElmdscr_display(pElm, modelRef, Bentley::DgnPlatform::DRAW_MODE_Normal);
    return filePos;
}

bool MicroStation::HasArea(MSElementCP el)
{
    const int elemType{ mdlElement_getType(el) };
    const bool hasArea = (Bentley::DgnPlatform::CMPLX_SHAPE_ELM == elemType
        || Bentley::DgnPlatform::SHAPE_ELM == elemType
        || Bentley::DgnPlatform::ELLIPSE_ELM == elemType);
    return hasArea;
}

bool MicroStation::HasArea(ElementRefP elemRef)
{
    const int elemType{ elementRef_getElemType(elemRef) };
    const bool hasArea = (Bentley::DgnPlatform::CMPLX_SHAPE_ELM == elemType
        || Bentley::DgnPlatform::SHAPE_ELM == elemType
        || Bentley::DgnPlatform::ELLIPSE_ELM == elemType);
    return hasArea;
}

bool MicroStation::HasArea(MSElementDescrCP pElm)
{
    return HasArea(&pElm->el);
}

bool MicroStation::IsCellComponent(MSElementDescrCP pElm)
{
    return (nullptr != elementRef_getParent(pElm->h.elementRef));
}

UInt32 MicroStation::LevelId(DgnModelRefP modelRef)
{
    _ASSERTE(mdlModelRef_isReference(modelRef) && "MicroStation::LevelId modelRef must be a reference attachment");
    UInt32 levelId{ LEVEL_NULL_ID };
    mdlRefFile_getIntegerParameters(&levelId, REFERENCE_LEVEL, modelRef);
    return levelId;
}

UInt32 MicroStation::LevelId(Bentley::WCharCP name, DgnModelRefP modelRef)
{
    UInt32 levelId{ LEVEL_NULL_ID };
    if (SUCCESS != mdlLevel_getIdFromName(&levelId, modelRef, LEVEL_NULL_ID, name))
    {
        enum StringControl { BufferLength = 256, DisplayNameLength = 512, };
        WChar msg[BufferLength];
        if (mdlModelRef_isActiveModel(modelRef))
        {
            swprintf_s(msg, BufferLength, L"MicroStation::LevelId: unable to get level ID for '%s' in active model", name);
        }
        else if (mdlModelRef_isReference(modelRef))
        {
            WChar displayName[DisplayNameLength];
            mdlModelRef_getDisplayName(modelRef, displayName, DisplayNameLength, nullptr);
            swprintf_s(msg, DisplayNameLength, L"MicroStation::LevelId: unable to get level ID for '%s' in reference model %s", name, displayName);
        }
        else
        {
            swprintf_s(msg, BufferLength, L"MicroStation::LevelId: unable to get level ID for '%s'", name);
        }
        mdlOutput_messageCenter(Bentley::DgnPlatform::OutputMessagePriority::Error, msg, msg, Bentley::DgnPlatform::OutputMessageAlert::Dialog);
    }
    return levelId;
}

UInt32 MicroStation::CapLineWeight(UInt32 weight) noexcept
{
    return weight > MicroStation::MaxLineWeight ? MicroStation::MaxLineWeight : weight;
}

UInt32 MicroStation::AddOrDisplay(MSElementDescrP pElm, Bentley::DgnPlatform::DgnDrawMode drawMode, DgnModelRefP modelRef)
{
    UInt32 filePos{ 0 };
    if (Bentley::DgnPlatform::DRAW_MODE_Normal == drawMode)
        filePos = mdlElmdscr_add(pElm);
    else
        mdlElmdscr_display(pElm, modelRef, drawMode);
    return filePos;
}

UInt32 MicroStation::AddOrDisplay(MSElementP el, Bentley::DgnPlatform::DgnDrawMode drawMode, DgnModelRefP)
{
    UInt32 filePos{ 0 };
    if (Bentley::DgnPlatform::DRAW_MODE_Normal == drawMode)
        filePos = mdlElement_add(el);
    else
        mdlElement_display(el, drawMode);
    return filePos;
}

bool MicroStation::CreateCellHeader(MSElementDescrP* header, const WChar* name)
{
    bool created{ false };
    DPoint3d origin;
    origin.x = 0.0;
    origin.y = 0.0;
    origin.z = 0.0;

    if (header)
        mdlElmdscr_freeAll(header);

    Bentley::DgnPlatform::MSElement element;
    if (SUCCESS == mdlCell_create(&element, name, &origin, FALSE)
        && SUCCESS == mdlElmdscr_new(header, nullptr, &element))
    {
        created = true;
    }
    else
    {
        enum StringControl { BufferLength = 256, };
        wchar_t error[BufferLength];
        swprintf_s(error, BufferLength, L"MicroStation::CreateCellHeader (%s) failed", name);
        mdlOutput_messageCenter(Bentley::DgnPlatform::OutputMessagePriority::Error, error, error, Bentley::DgnPlatform::OutputMessageAlert::None);
    }
    return created;
}

bool MicroStation::CreateMarker(DPoint3d const& location, MSElementP buffer, double size, UInt32 colour, bool permanent)
{
    bool rc{ false };
    if (SUCCESS == mdlEllipse_create(buffer, nullptr, const_cast<DPoint3d*>(&location), size, size, nullptr, FillModeNone))
    {
        SetColour(buffer, colour);
        if (permanent)
        {
            rc = 0 < mdlElement_add(buffer);
            mdlElement_display(buffer, Bentley::DgnPlatform::DRAW_MODE_Normal);
        }
        else
        {
            const int viewMask{ 0xFF };
            msTransientElmP = (TransDescrP)mdlTransient_addElement(msTransientElmP, buffer, FALSE, viewMask, Bentley::DgnPlatform::DRAW_MODE_Normal, FALSE, FALSE, TRUE);
            rc = nullptr != msTransientElmP;
        }
    }
    return rc;
}

bool MicroStation::HatchStatus(int status, const std::wstring& patternName, const Bentley::DgnPlatform::ElementId& id)
{
    enum StringControl { BufferLength = 256, };
    bool succeeded{ SUCCESS == status };
    wchar_t comment[BufferLength];

    switch (status)
    {
    case SUCCESS:
        if (0 < id)
            swprintf_s(comment, BufferLength, L"Applied pattern '%s' to element ID %I64d", patternName.c_str(), id);
        else
            swprintf_s(comment, BufferLength, L"Applied pattern '%s'", patternName.c_str());
        break;
    case MDLERR_CELLNOTFOUND:
        swprintf_s(comment, BufferLength, L"Pattern cell cannot be found for pattern '%s'", patternName.c_str());
        break;
    case MDLERR_NONCLOSEDPATELM:
        swprintf_s(comment, BufferLength, L"Shape element ID %I64d is not closed", id);
        break;
    case MDLERR_NONSOLIDPATELM:
        swprintf_s(comment, BufferLength, L"Shape element ID %I64d is marked as a hole and cannot be patterned", id);
        break;
    case MDLERR_INVALIDPATSPACE:
        swprintf_s(comment, BufferLength, L"Pattern '%s': spacing is not greater than zero", patternName.c_str());
        break;
    default:
        swprintf_s(comment, BufferLength, L"Undefined error %d when attempting to apply pattern '%s' to element ID %I64d", status, patternName.c_str(), id);
        break;
    }

    mdlOutput_messageCenter(Bentley::DgnPlatform::OutputMessagePriority::Debug, comment, comment,
        succeeded ? Bentley::DgnPlatform::OutputMessageAlert::None : Bentley::DgnPlatform::OutputMessageAlert::Dialog);
    return succeeded;
}

void MicroStation::SendKeyin(Bentley::WCharCP keyin)
{
    mdlInput_sendKeyin(keyin, 0, MSInputQueuePos::INPUTQ_EOQ, ustnTaskId);
}

void MicroStation::ActiveColour(WChar const* book, WChar const* colour)
{
    enum StringControl { BufferLength = 256, };
    wchar_t keyin[BufferLength];
    swprintf_s(keyin, BufferLength, L"ACTIVE COLOR %s:%s", book, colour);
    SendKeyin(keyin);
}

void MicroStation::Cnv2MasterUnits(DPoint3d& out_master_units, const DPoint3d& in_uors, DgnModelRefP modelRef)
{
    const double UORs{ 1.0 / mdlModelRef_getUorPerMaster(modelRef) };
    DPoint3d go;
    mdlModelRef_getGlobalOrigin(modelRef, &go);
    DVec3d mu;
    mdlVec_subtractDPoint3dDPoint3d(&mu, &in_uors, &go);
    mdlVec_scaleInPlace(&mu, UORs);
    out_master_units.x = mu.x; out_master_units.y = mu.y; out_master_units.z = mu.z;
}

void MicroStation::Cnv2UORs(DPoint3d& out_uors, const DPoint3d& in_master_units, DgnModelRefP modelRef)
{
    const double UORs{ mdlModelRef_getUorPerMaster(modelRef) };
    DVec3d go;
    DVec3d in;
    in.x = in_master_units.x;
    in.y = in_master_units.y;
    in.z = in_master_units.z;
    DVec3d out;
    mdlModelRef_getGlobalOrigin(modelRef, &go);
    mdlVec_scale(&out, &in, UORs);
    mdlVec_addPoint(&out, &out, &go);
    out_uors.x = out.x; out_uors.y = out.y; out_uors.z = out.z;
}

bool MicroStation::IsGraphicalElement(ElementHandleCR eh)
{
    return nullptr != eh.GetDisplayHandler();
}

bool MicroStation::IsZeroLengthLine(MSElementDescrCP pElm)
{
    return IsZeroLengthLine(&pElm->el, pElm->h.dgnModelRef);
}

bool MicroStation::IsZeroLengthLine(MSElementCP el, DgnModelRefP modelRef)
{
    bool zeroLengthLine{ false };
    if (Bentley::DgnPlatform::MSElementTypes::LINE_ELM == mdlElement_getType(el))
    {
        DPoint3d points[2];
        int nPoints{ 0 };
        if (SUCCESS == mdlLinear_extract(points, &nPoints, el, modelRef))
        {
            _ASSERTE(2 == nPoints && "mdlLinear_extract: unexpected no. points from line element");
            DVec3d vecs[2];
            vecs[0].x = points[0].x; vecs[0].y = points[0].y; vecs[0].z = points[0].z;
            vecs[1].x = points[1].x; vecs[1].y = points[1].y; vecs[1].z = points[1].z;
            zeroLengthLine = mdlVec_equal(0 + vecs, 1 + vecs) ? true : false;
        }
    }
    return zeroLengthLine;
}

bool MicroStation::IsRectangle(MSElementDescrCP shape)
{
    bool rectangular{ false };
    DPoint3d points[RectangleVertexCount];
    int nPoints{ 0 };
    _ASSERTE(Bentley::DgnPlatform::MSElementTypes::SHAPE_ELM == mdlElement_getType(&shape->el) && "MicroStation::IsRectangle: expected SHAPE_ELM");
    if (Bentley::DgnPlatform::MSElementTypes::SHAPE_ELM == mdlElement_getType(&shape->el)
        && RectangleVertexCount == mdlLinear_getPointCount(&shape->el)
        && SUCCESS == mdlLinear_extract(points, &nPoints, &shape->el, shape->h.dgnModelRef))
    {
        DVec3d v[4];
        mdlVec_subtractDPoint3dDPoint3d(0 + v, 0 + points, 1 + points);
        mdlVec_subtractDPoint3dDPoint3d(1 + v, 1 + points, 2 + points);
        mdlVec_subtractDPoint3dDPoint3d(2 + v, 2 + points, 3 + points);
        mdlVec_subtractDPoint3dDPoint3d(3 + v, 3 + points, 4 + points);
        if (mdlVec_arePerpendicular(0 + v, 1 + v)
            && mdlVec_arePerpendicular(1 + v, 2 + v)
            && mdlVec_equal(0 + v, 2 + v))
            rectangular = true;
    }
    return rectangular;
}

void MicroStation::AssociativePatternLock(bool b)
{
    tcb->ext_locks.associativePattern = b ? 1 : 0;
    mdlDialog_synonymsSynch((RscFileHandle)nullptr, SYNONYMID_PatternLockSettings, nullptr);
}

void MicroStation::ActiveParamStatus(int status, std::wstring const& paramName)
{
    switch (status)
    {
    default:
    {
        enum StringControl { BufferLength = 256, };
        wchar_t warning[BufferLength];
        swprintf_s(warning, BufferLength, L"mdlParams_setActive %s failed with error %d", paramName.c_str(), status);
        mdlOutput_messageCenter(Bentley::DgnPlatform::OutputMessagePriority::Error, warning, warning, Bentley::DgnPlatform::OutputMessageAlert::Dialog);
        break;
    }
    case SUCCESS:
        break;
    }
}

bool MicroStation::IsModelWritable(DgnModelRefP modelRef, UInt32 message_list_ID, UInt32 message_ID, bool alert)
{
    const bool writable{ mdlModelRef_isReadOnly(modelRef) ? false : true };
    if (!writable && alert)
    {
        WChar modelName[Bentley::DgnPlatform::MAX_MODEL_NAME_LENGTH];
        mdlModelRef_getModelName(modelRef, modelName);
        Bentley::WString warning;
        mdlOutput_rscsPrintf(warning, 0, message_list_ID, message_ID, modelName);
        mdlOutput_messageCenter(Bentley::DgnPlatform::OutputMessagePriority::Error, warning.c_str(), warning.c_str(), Bentley::DgnPlatform::OutputMessageAlert::None);
    }
    return writable;
}

bool MicroStation::RemoveElementPattern(const Bentley::DgnPlatform::ElementId& id, DgnModelRefP modelRef)
{
    bool removed{ false };
    Bentley::DgnPlatform::EditElementHandle eeh{ id, modelRef };
    return removed;
}

DgnFileP MicroStation::AttachCellLibrary(const std::wstring& library, Bentley::WCharCP cfgVar)
{
    DgnFileP oLibrary{ nullptr };
    bool attached{ false };
    BeFileName lib;
    if (SUCCESS == mdlFile_find(&lib, library.c_str(), cfgVar, L".cel"))
    {
        BeFileName actualLib;
        attached = (SUCCESS == mdlCell_attachLibrary(actualLib, &lib, nullptr, TRUE));
    }

    enum StringControl { BufferLength = 256, };
    wchar_t msg[BufferLength];
    if (attached && SUCCESS == mdlCell_getLibraryObject(&oLibrary, library.c_str(), FALSE))
    {
        swprintf_s(msg, BufferLength, L"Attached cell library '%s'", library.c_str());
        mdlOutput_messageCenter(Bentley::DgnPlatform::OutputMessagePriority::Debug, msg, msg, Bentley::DgnPlatform::OutputMessageAlert::None);
    }
    else
    {
        swprintf_s(msg, BufferLength, L"Failed to attach cell library '%s'", library.c_str());
        mdlOutput_messageCenter(Bentley::DgnPlatform::OutputMessagePriority::Warning, msg, msg, Bentley::DgnPlatform::OutputMessageAlert::None);
    }
    return oLibrary;
}

std::wstring MicroStation::GetCellName(MSElementDescrCP pCell)
{
    wchar_t cellName[Bentley::DgnPlatform::MAX_MODEL_NAME_LENGTH] = { 0 };
    const int elemType{ mdlElement_getType(&pCell->el) };
    switch (elemType)
    {
    case Bentley::DgnPlatform::MSElementTypes::CELL_HEADER_ELM:
        mdlCell_extractName(cellName, Bentley::DgnPlatform::MAX_MODEL_NAME_LENGTH, &pCell->el);
        break;
    case Bentley::DgnPlatform::MSElementTypes::SHARED_CELL_ELM:
        mdlSharedCell_extract(nullptr, nullptr, nullptr, nullptr, nullptr, cellName, Bentley::DgnPlatform::MAX_MODEL_NAME_LENGTH, &pCell->el, pCell->h.dgnModelRef);
        break;
    default:
    {
        enum StringControl { BufferLength = 256, };
        wchar_t msg[BufferLength];
        swprintf_s(msg, BufferLength, L"MicroStation::GetCellName: element type %d ID %I64d is not a cell", elemType, mdlElement_getID(&pCell->el));
        mdlOutput_messageCenter(Bentley::DgnPlatform::OutputMessagePriority::Error, msg, msg, Bentley::DgnPlatform::OutputMessageAlert::None);
    }
    break;
    }
    return std::wstring(cellName);
}

UInt32 MicroStation::GetCellNames(WideStringCollection& cellNames, DgnFileP oLibrary)
{
    _ASSERTE(nullptr != oLibrary && "MicroStation::GetCellNames: DgnFileObjP may not be nullptr");
    UInt32 nCells{ 0 };
    DgnIndexIteratorP iterator{ mdlModelIterator_create(oLibrary) };
    mdlModelIterator_setAcceptCellsOnly(iterator, TRUE);
    DgnIndexItemP item{ mdlModelIterator_getFirst(iterator) };
    WChar cellName[Bentley::DgnPlatform::MAX_CELLNAME_LENGTH];
    while (item)
    {
        mdlModelItem_getName(item, cellName, Bentley::DgnPlatform::MAX_CELLNAME_LENGTH);
        cellNames.push_back(std::wstring(cellName));
        ++nCells;
        item = mdlModelIterator_getNext(iterator);
    }
    mdlModelIterator_free(iterator);
    return nCells;
}

UInt32 MicroStation::AreaStroker::Stroke(DPoint3dCollection& points, MSElementDescrCP shape)
{
    DPoint3d* stroked{ nullptr };
    int nPoints{ 0 };
    if (SUCCESS == mdlElmdscr_stroke(&stroked, &nPoints, const_cast<MSElementDescrP>(shape), tolerance_))
    {
        points.reserve(nPoints);
        for (int i = 0; i < nPoints; ++i)
            points.push_back(*(i + stroked));
        _ASSERTE((points.size() == static_cast<size_t>(nPoints)) && "AreaStroker::StrokeArea point count mismatch");
    }
    else
    {
        enum StringControl { BufferLength = 256, };
        wchar_t msg[BufferLength];
        swprintf_s(msg, BufferLength, L"AreaStroker::Stroke invalid element ID %I64d type %d", mdlElement_getID(&shape->el), mdlElement_getType(&shape->el));
        mdlOutput_messageCenter(Bentley::DgnPlatform::OutputMessagePriority::Error, msg, msg, Bentley::DgnPlatform::OutputMessageAlert::Dialog);
    }

    dlmSystem_mdlFree(stroked);
    return static_cast<UInt32>(nPoints);
}

UInt32 MicroStation::AreaStroker::Stroke()
{
    if (mdlElmdscr_isGroupedHole(pArea_))
    {
        MSElementDescrCP pComponent = pArea_->h.firstElem;
        while (pComponent)
        {
            if (mdlElmdscr_isClosed(pComponent))
            {
                DPoint3dCollection points;
                UInt32 nPoints{ Stroke(points, pComponent) };
                if (0 < nPoints)
                    polygons_.push_back(points);
            }
            pComponent = pComponent->h.next;
        }
    }
    else
    {
        DPoint3dCollection points;
        UInt32 nPoints{ Stroke(points, pArea_) };
        if (0 < nPoints)
            polygons_.push_back(points);
    }
    return static_cast<UInt32>(polygons_.size());
}

bool MicroStation::AreaStroker::PointInside(DPoint3d const* test) const
{
    const StrokedArrayConstIterator boundary{ polygons_.begin() };
    bool inside{ PointInside(test, *boundary) };
    if (inside && 1 < polygons_.size())
    {
        const StrokedArrayConstIterator end{ polygons_.end() };
        StrokedArrayConstIterator polygon;
        for (polygon = 1 + boundary; polygon != end; ++polygon)
        {
            if (PointInside(test, *polygon))
            {
                inside = false;
                break;
            }
        }
    }
    return inside;
}

bool MicroStation::AreaStroker::PointInside(DPoint3d const* test, const DPoint3dCollection& polygon) const
{
    bool inside{ false };
    if (1 == mdlPolygon_pointInsideXY(const_cast<DPoint3d*>(test), const_cast<DPoint3d*>(&polygon[0]), static_cast<int>(polygon.size()), tolerance_))
        inside = true;
    return inside;
}

DPoint3dCollection const& MicroStation::AreaStroker::Array(UInt32 index) const
{
    _ASSERTE(index < polygons_.size() && "AreaStroker::Array invalid index");
    return polygons_[index];
}

MicroStation::AreaStroker::AreaStroker(MSElementDescrCP pArea, double tolerance)
    : pArea_(pArea),
    tolerance_(tolerance)
{
    _ASSERTE((mdlElmdscr_isClosed(pArea_)
        || mdlElmdscr_isGroupedHole(pArea_))
        && "MicroStationUtilities::AreaStroker: area is not closed or Grouped Hole");
    _ASSERTE((0. <= tolerance_) && "AreaStroker::StrokeArea tolerance must be positive number");
}

MicroStation::AreaStroker::~AreaStroker()
{
}

void MicroStation::SetColour(MSElementP el, UInt32 colour)
{
    mdlElement_setSymbology(el, &colour, nullptr, nullptr);
}

void MicroStation::SetColour(MSElementDescrP el, UInt32 colour)
{
    const bool NotOwned{ false };
    const bool Modified{ true };
    Bentley::DgnPlatform::EditElementHandle eeh{ el, NotOwned, Modified };
    Bentley::DgnPlatform::ElementPropertiesSetterPtr remapper{ Bentley::DgnPlatform::ElementPropertiesSetter::Create() };
    remapper->SetColor(colour);
    remapper->Apply(eeh);
}

UInt32 MicroStation::LevelColour(UInt32 levelId, DgnModelRefP modelRef)
{
    UInt32 colour{ 99 };
    bool override{ false };
    mdlLevel_getColor(&colour, &override, modelRef, levelId);
    return colour;
}

UInt32 MicroStation::ActiveColour()
{
    UInt32 c{ 0 };
    if (SUCCESS != ActiveParams::GetValue(c, ActiveUInt32Params::ACTIVEPARAM_COLOR))
    {
        enum StringControl { BufferLength = 256, };
        wchar_t msg[BufferLength];
        swprintf_s(msg, BufferLength, L"MicroStation::ActiveColour failed");
        mdlOutput_messageCenter(Bentley::DgnPlatform::OutputMessagePriority::Error, msg, msg, Bentley::DgnPlatform::OutputMessageAlert::None);
    }
    return c;
}

int VersionPart(size_t& pos0, size_t& pos1, std::wstring const& s, WCharCP separator, WCharCP)
{
    pos1 = s.find(separator, pos0);
    const int v{ _wtoi(s.substr(pos0, pos1 - pos0).c_str()) };
    pos0 = pos1 + 1;
    return v;
}

#ifdef UTILITIES_HAVE_MODULVER
bool WindowsVersionToMicroStationVersion(VersionNumber& msVersion, Bentley::WCharCP dllName)
{
    bool rc{ false };
    CModuleVersion ver;
    if (ver.GetFileVersionInfo(dllName))
    {
        std::wstring s(ver.GetValue(L"FileVersion"));
        WCharCP Comma{ L"," };
        WCharCP Period{ L"." };
        WCharCP separator{ Comma };
        size_t pos0{ 0 };
        size_t pos1{ s.find(separator, pos0) };
        if (std::wstring::npos == pos1)
        {
            separator = Period;
            pos1 = s.find(separator, pos0);
        }
        msVersion.release = VersionPart(pos0, pos1, s, separator, L"msVersion.release");
        msVersion.major = VersionPart(pos0, pos1, s, separator, L"msVersion.major");
        msVersion.minor = VersionPart(pos0, pos1, s, separator, L"msVersion.minor");
        msVersion.subMinor = VersionPart(pos0, pos1, s, separator, L"msVersion.subMinor");
        rc = true;
    }
    else
    {
        Bentley::WString error;
        error.Sprintf(L"MicroStationUtilities: GetFileVersionInfo failed for DLL %s", dllName);
        mdlOutput_messageCenter(Bentley::DgnPlatform::OutputMessagePriority::Error, error.c_str(), error.c_str(), Bentley::DgnPlatform::OutputMessageAlert::Dialog);
    }
    return rc;
}

bool MicroStation::SetVersion(Bentley::WCharCP dllName)
{
    bool rc{ false };
    VersionNumber msVersion;
    if (WindowsVersionToMicroStationVersion(msVersion, dllName))
    {
        mdlSystem_setMdlAppVersionNumber(nullptr, &msVersion);
        rc = true;
    }
    return rc;
}

Bentley::WString MicroStation::VersionString(Bentley::WCharCP dllName)
{
    Bentley::WString version(L"unknown");
    VersionNumber msVersion;
    if (WindowsVersionToMicroStationVersion(msVersion, dllName))
        version.Sprintf(L"%d.%d.%d.%d", msVersion.release, msVersion.major, msVersion.minor, msVersion.subMinor);
    return version;
}
#endif 

void MicroStation::InitDPoint3dMin(DPoint3d& pt) noexcept
{
    pt.x = pt.y = pt.z = RMINDESIGNRANGE;
}

void MicroStation::InitDPoint3dMax(DPoint3d& pt) noexcept
{
    pt.x = pt.y = pt.z = RMAXDESIGNRANGE;
}

void MicroStation::InitDPoint3dZero(DPoint3d& pt) noexcept
{
    pt.x = pt.y = pt.z = 0.;
}

void MicroStation::GetViewRotation(RotMatrix& r, int viewNum)
{
    mdlRMatrix_fromView(&r, viewNum, FALSE);
    mdlRMatrix_normalize(&r, &r);
    mdlRMatrix_transpose(&r, &r);
}

RscFileHandle MicroStation::MdlMain(MdlCommandNumber cmdNumbers[], long commands, long prompts)
{
    enum StringControl { BufferLength = 256, };
    WChar msg[BufferLength];
    Bentley::WString currTaskId{ mdlSystem_getCurrTaskID() };
    RscFileHandle hRscFile{ 0 };
    if (SUCCESS != mdlResource_openFile(&hRscFile, nullptr, RSC_READONLY))
    {
        swprintf_s(msg, BufferLength, L"%s: unable to open MDL resource file\n", currTaskId.c_str());
        return 0;
    }

    if (nullptr == mdlParse_loadCommandTable(nullptr))
    {
        swprintf_s(msg, BufferLength, L"%s: unable to load command table", currTaskId.c_str());
        mdlOutput_messageCenter(Bentley::DgnPlatform::OutputMessagePriority::Error, msg, msg, Bentley::DgnPlatform::OutputMessageAlert::None);
        return 0;
    }
    else
    {
        swprintf_s(msg, BufferLength, L"loaded command table");
        mdlOutput_messageCenter(Bentley::DgnPlatform::OutputMessagePriority::Debug, msg, msg, Bentley::DgnPlatform::OutputMessageAlert::None);
    }

    int status = mdlSystem_registerCommandNumbers(cmdNumbers);
    if (SUCCESS != status)
    {
        swprintf_s(msg, BufferLength, L"%s: mdlSystem_registerCommandNumbers (status=%d)", currTaskId.c_str(), status);
        mdlOutput_messageCenter(Bentley::DgnPlatform::OutputMessagePriority::Error, msg, msg, Bentley::DgnPlatform::OutputMessageAlert::None);
        return 0;
    }
    else
    {
        swprintf_s(msg, BufferLength, L"registered CommandNumbers");
        mdlOutput_messageCenter(Bentley::DgnPlatform::OutputMessagePriority::Debug, msg, msg, Bentley::DgnPlatform::OutputMessageAlert::None);
    }

    mdlState_registerStringIds(commands, prompts);
    swprintf_s(msg, BufferLength, L"registered String IDs");
    mdlOutput_messageCenter(Bentley::DgnPlatform::OutputMessagePriority::Debug, msg, msg, Bentley::DgnPlatform::OutputMessageAlert::None);

    ::setlocale(LC_ALL, "");

    swprintf_s(msg, BufferLength, L"MdlMain done");
    mdlOutput_messageCenter(Bentley::DgnPlatform::OutputMessagePriority::Debug, msg, msg, Bentley::DgnPlatform::OutputMessageAlert::None);
    return hRscFile;
}

Bentley::RgbColorDef MicroStation::ConvertString2ColorDef(const std::wstring& s)
{
    return ConvertString2ColorDef(s.c_str());
}

Bentley::RgbColorDef MicroStation::ConvertString2ColorDef(Bentley::WCharCP s)
{
    Int32 colour{ wcstol(s, nullptr, 16) };
    const Int32 Mask{ 0x0000FF };
    Bentley::RgbColorDef rgb{ (UChar)((colour >> 16) & Mask),
                              (UChar)((colour >> 8) & Mask),
                              (UChar)((colour >> 0) & Mask) };
    return rgb;
}

Bentley::PrecisionFormat MicroStation::DecimalPrecision(DgnModelRefP modelRef)
{
    ModelInfoCP info{ modelRef->GetModelInfoCP() };
    Bentley::PrecisionFormat precision{ info->GetLinearPrecision() };
    return precision;
}

bool MicroStation::ViewSettingsFill()
{
    int nFilled{ 0 };
    Bentley::DgnPlatform::ViewFlags flags;
    for (int view = 0; view < Bentley::DgnPlatform::MAX_VIEWS; ++view)
    {
        if (mdlView_isActive(view))
        {
            mdlView_getFlags(&flags, view);
            if (flags.fill)
                ++nFilled;
        }
    }
    return (0 < nFilled);
}

void MicroStation::TraceListModel(ListModel const* pListModel)
{
    enum StringControl { BufferLength = 512, };
    WChar msg[BufferLength];
    swprintf_s(msg, BufferLength, L"List Model rowsXcolumns=%d X %d", mdlListModel_getRowCount(pListModel), mdlListModel_getColumnCount(pListModel));
    mdlOutput_messageCenter(Bentley::DgnPlatform::OutputMessagePriority::Debug, msg, msg, Bentley::DgnPlatform::OutputMessageAlert::None);

    for (int row = 0; row < mdlListModel_getRowCount(pListModel); ++row)
    {
        swprintf_s(msg, BufferLength, L"Row %d of %d", row, mdlListModel_getRowCount(pListModel));
        mdlOutput_messageCenter(Bentley::DgnPlatform::OutputMessagePriority::Debug, msg, msg, Bentley::DgnPlatform::OutputMessageAlert::None);
        ListRow* pRow = mdlListModel_getRowAtIndex(pListModel, row);
        for (int col = 0; col < mdlListModel_getColumnCount(pListModel); ++col)
        {
            ListCell* pCell = mdlListRow_getCellAtIndex(pRow, col);
            WCharCP s = nullptr;
            mdlListCell_getDisplayText(pCell, &s);
            swprintf_s(msg, BufferLength, L"cell [%d,%d] '%s'", row, col, s);
            mdlOutput_messageCenter(Bentley::DgnPlatform::OutputMessagePriority::Debug, msg, msg, Bentley::DgnPlatform::OutputMessageAlert::None);
        }
    }
}

MicroStation::MdlTaskController::MdlTaskController(const wchar_t* taskName)
    : alreadyLoaded_(false),
    taskName_(taskName)
{
    if (SUCCESS == mdlSystem_getTaskStatistics(nullptr, taskName_))
    {
        alreadyLoaded_ = true;
    }
    else
    {
        mdlSystem_loadMdlProgram(taskName_, nullptr, nullptr);
        alreadyLoaded_ = false;
    }
}

MicroStation::MdlTaskController::~MdlTaskController()
{
    if (!alreadyLoaded_)
        mdlSystem_unloadMdlProgram(taskName_);
}
