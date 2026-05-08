#pragma once

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

#if !defined(winNT)
#define winNT
#endif

#if !defined(BENTLEY)
#define BENTLEY
#endif

#define NO_BOOLEAN_TYPE

#if !defined(NORECTANGLE)
#define NORECTANGLE
#endif

// Standard Library
#include <vector>
#include <string>
#include <clocale>

#pragma region MicroStationAPI
#include <Mstn/MdlApi/MdlApi.h>
#include <DgnPlatform/DgnPlatform.h>
#include <DgnPlatform/DgnPlatform.r.h>
#include <DgnPlatform/DgnFileIO/ElementRefBase.h>
#include <DgnPlatform/DgnFileIO/DgnElements.h>
#include <DgnPlatform/DgnFile.h>
#include <Mstn/MdlApi/elementref.h>
#pragma endregion

#ifdef DialogBox
#undef DialogBox
#endif

#include <std_collection_typedefs.h>

namespace MicroStation
{
    enum ShapeControl
    {
        TriangleVertexCount = 3 + 1,
        RectangleVertexCount = 4 + 1,
    };

    enum FillMode
    {
        FillModeActive = -1,
        FillModeNone = 0,
        FillModeFilled = 1,
    };

    enum Misc
    {
        MaxLineWeight = 31,
    };

    UInt32 CapLineWeight(UInt32 weight) noexcept;

    struct WindowCursor
    {
        explicit WindowCursor(SYSTEMCURSOR cursor) noexcept;
        ~WindowCursor() noexcept;

        WindowCursor(WindowCursor const&) = delete;
        WindowCursor& operator=(WindowCursor const&) = delete;
    };

    void UnloadCurrentTask();

    struct BusyCursor
    {
        BusyCursor() noexcept;
        ~BusyCursor() noexcept;

        BusyCursor(BusyCursor const&) = delete;
        BusyCursor& operator=(BusyCursor const&) = delete;
    };

    struct BusyBar
    {
        BusyBar(wchar_t const* title, wchar_t const* msg);
        ~BusyBar() noexcept;

        BusyBar(BusyBar const&) = delete;
        BusyBar& operator=(BusyBar const&) = delete;

        void UpdateMessage(wchar_t const* msg);
    };

    void QueueMdlUnload();
    bool SetVersion(Bentley::WCharCP dllName);
    Bentley::WString VersionString(Bentley::WCharCP dllName);
    RscFileHandle MdlMain(MdlCommandNumber cmdNumbers[], long commands, long prompts);

    bool DefaultModelIs3D(Bentley::WCharCP filePath);
    Bentley::DgnPlatform::DgnFileFormatType FileFormat(Bentley::WCharCP filePath, bool* is3D = nullptr);

#if defined(IMODEL_API)
    bool IsIModel();
#endif

    std::wstring DescribeFileFormat(Bentley::DgnPlatform::DgnFileFormatType format);
    std::wstring DescribeElement(UInt32 filePos, DgnModelRefP modelRef);
    std::wstring DescribeElement(ElementRefP elemRef, DgnModelRefP modelRef);
    bool CanOpen(Bentley::WCharCP filePath);
    bool PublishComplexVariable(const void* data, char const* tagName, char const* variableName);
    std::wstring RefModelName(DgnModelRefP modelRef);
    std::wstring RefDescription(DgnModelRefP modelRef);
    std::wstring RefLogicalName(DgnModelRefP modelRef);
    std::wstring RefAttachName(DgnModelRefP modelRef);
    std::wstring RefFileName(DgnModelRefP modelRef);
    UInt32 ElementCount(DgnModelRefP modelRef);
    std::wstring MasterUnitName(DgnModelRefP modelRef = ACTIVEMODEL);
    std::wstring SubUnitName(DgnModelRefP modelRef = ACTIVEMODEL);
    UInt32 ModelCount(DgnModelRefP modelRef = ACTIVEMODEL);
    std::wstring LevelDescr(DgnModelRefP modelRef, UInt32 levelId);
    std::wstring LevelName(ElementHandleCR eh);
    std::wstring LevelName(DgnModelRefP modelRef, UInt32 levelId);
    std::wstring ModelFileName(DgnModelRefP modelRef);
    std::wstring ModelName(DgnModelRefP modelRef);
    bool IsCellLibrary(DgnModelRefP modelRef);
    bool IsSameModel(DgnModelRefP modelRef, Bentley::WCharCP modelName);
    bool IsSameModel(DgnModelRefP modelRef, const std::wstring& modelName);
    bool IsSameModel(DgnModelRefP modelRef1, DgnModelRefP modelRef2);
    bool NewDesignFile(const wchar_t* filePath, const wchar_t* modelName = nullptr, bool readOnly = false);
    bool NewDesignFile(const std::wstring& filePath, const std::wstring& modelName = L"", bool readOnly = false);
    void TraceListModel(ListModel const* pListModel);

    bool CreateCellHeader(MSElementDescrP* header, const wchar_t* name);
    inline bool CreateCellHeader(MSElementDescrP* header, const std::wstring& name)
    {
        return CreateCellHeader(header, name.c_str());
    }
    inline bool CreateCellHeader(MSElementDescrP* header, const Bentley::WString& name)
    {
        return CreateCellHeader(header, name.GetWCharCP());
    }

    bool CreateMarker(DPoint3d const& location, MSElementP marker, double size, UInt32 colour, bool permanent);
    bool IsGraphicalElement(ElementHandleCR eh);
    Bentley::DgnPlatform::ElementId GetElementID(ElementHandleCR eh);
    UInt32 LevelId(Bentley::WCharCP name, DgnModelRefP modelRef);
    UInt32 LevelId(DgnModelRefP modelRef);
    bool IsCellComponent(MSElementDescrCP pElm);
    void SetColour(MSElementDescrP el, UInt32 colour);
    void SetColour(MSElementP el, UInt32 colour);
    void ActiveColour(wchar_t const* book, wchar_t const* colour);
    Bentley::RgbColorDef ConvertString2ColorDef(Bentley::WCharCP hex);
    Bentley::RgbColorDef ConvertString2ColorDef(const std::wstring& hex);
    void Cnv2MasterUnits(DPoint3d& out_master_units, const DPoint3d& in_uors, DgnModelRefP modelRef);
    void Cnv2UORs(DPoint3d& out_uors, const DPoint3d& in_master_units, DgnModelRefP modelRef);
    bool HasArea(MSElementCP el);
    bool HasArea(MSElementDescrCP pElm);
    bool HasArea(ElementRefP elemRef);
    bool IsZeroLengthLine(MSElementDescrCP el);
    bool IsZeroLengthLine(MSElementCP el, DgnModelRefP modelRef);
    bool IsRectangle(MSElementDescrCP shape);
    void InitDPoint3dMin(DPoint3d& pt) noexcept;
    void InitDPoint3dMax(DPoint3d& pt) noexcept;
    void InitDPoint3dZero(DPoint3d& pt) noexcept;
    UInt32 AddToModel(MSElementDescrP pDescr, DgnModelRefP modelRef);
    UInt32 AddOrDisplay(MSElementP el, Bentley::DgnPlatform::DgnDrawMode drawMode, DgnModelRefP modelRef = ACTIVEMODEL);
    UInt32 AddOrDisplay(MSElementDescrP pDescr, Bentley::DgnPlatform::DgnDrawMode drawMode, DgnModelRefP modelRef = ACTIVEMODEL);
    bool HatchStatus(int status, const std::wstring& patternName, const Bentley::DgnPlatform::ElementId& id);
    void SendKeyin(Bentley::WCharCP keyin);
    bool ViewSettingsFill();
    bool RemoveElementPattern(const Bentley::DgnPlatform::ElementId& id, DgnModelRefP modelRef);
    Bentley::PrecisionFormat DecimalPrecision(DgnModelRefP modelRef);
    void GetViewRotation(RotMatrix& r, int viewNum);
    bool IsModelWritable(DgnModelRefP modelRef, UInt32 message_list_ID, UInt32 message_ID, bool alert = true);
    DgnFileP AttachCellLibrary(const std::wstring& library, Bentley::WCharCP cfgVar = L"MS_CELL");
    UInt32 StringLength(MSElementDescrCP pText);
    Bentley::WString GetText(MSElementDescrCP pText);
    std::wstring GetCellName(MSElementDescrCP pCell);
    UInt32 GetCellNames(WideStringCollection& cellNames, DgnFileP oLibObj);
    std::string DgnFileObjectPath(DgnFileP o);
    UInt32 LevelColour(UInt32 levelId, DgnModelRefP modelRef);
    UInt32 ColourRGB(int red, int green, int blue, DgnModelRefP modelRef);
    UInt32 ActiveColour();
    void ActiveParamStatus(int status, std::wstring const& paramName);
    void AssociativePatternLock(bool b);

    class AreaStroker
    {
    public:
        AreaStroker(MSElementDescrCP pArea, double tolerance);
        ~AreaStroker();

        UInt32 Stroke(DPoint3dCollection& points, MSElementDescrCP shape);
        UInt32 Stroke();
        DPoint3dCollection const& Array(UInt32 index = 0) const;
        bool PointInside(DPoint3d const* test) const;
        bool PointInside(DPoint3d const* test, DPoint3dCollection const& polygon) const;
        UInt32 ShapeCount() const
        {
            return static_cast<UInt32>(polygons_.size());
        }

    private:
        MSElementDescrCP pArea_;
        double tolerance_;
        StrokedArrays polygons_;
    };

    class MdlTaskController
    {
    public:
        explicit MdlTaskController(const wchar_t* taskName);
        ~MdlTaskController();

        MdlTaskController(MdlTaskController const&) = delete;
        MdlTaskController& operator=(MdlTaskController const&) = delete;

    private:
        bool alreadyLoaded_;
        const wchar_t* taskName_;
    };

    struct SymbologyExt
    {
        SymbologyExt(const Bentley::RgbColorDef& lineColour,
            double lineTransparency = 0.0,
            UInt32 lineThickness = 1,
            Int32 lineStyle = 0,
            Int32 priority = 0);
        SymbologyExt(const Bentley::RgbColorDef& fillColour,
            double fillTransparency,
            const Bentley::RgbColorDef& lineColour,
            double lineTransparency = 0.0,
            UInt32 lineThickness = 1,
            Int32 lineStyle = 0,
            Int32 priority = 0);

        Bentley::RgbColorDef fillColour_;
        double fillTransparency_;
        Bentley::RgbColorDef lineColour_;
        double lineTransparency_;
        UInt32 lineThickness_;
        Int32 lineStyle_;
        mutable Int32 priority_;
    };
}
