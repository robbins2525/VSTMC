////////////////////////////////////////////////////////////////
//
//  ModuleVersion utility
//
//  Original concept based on the CModuleVersion class from
//  Paul DiLascia, Microsoft Systems Journal (1998).
//
//  Adapted for MicroStation development with contributions by
//  Jon Summers, Bentley MVP, Director, LA Solutions
//  https://www.la-solutions.co.uk/
//
//  This version has been modernized for contemporary C++
//  (memory safety improvements, RAII cleanup, etc.).
//
////////////////////////////////////////////////////////////////

#pragma once

#include <string>
#include <vector>
#include <windows.h>
#include <shlwapi.h>

// tell linker to link with version.lib for VerQueryValue, etc.
#pragma comment(linker, "/defaultlib:version.lib")

class CModuleVersion
{
public:
    CModuleVersion() = default;
    ~CModuleVersion() = default;

    CModuleVersion(const CModuleVersion&) = delete;
    CModuleVersion& operator=(const CModuleVersion&) = delete;
    CModuleVersion(CModuleVersion&&) = default;
    CModuleVersion& operator=(CModuleVersion&&) = default;

    BOOL GetFileVersionInfo(const wchar_t* moduleName);
    std::wstring GetValue(const wchar_t* keyName) const;

    static BOOL DllGetVersion(const wchar_t* moduleName, DLLVERSIONINFO& dvi);

    const VS_FIXEDFILEINFO& FixedInfo() const noexcept { return m_fixedInfo; }

private:
    struct Translation
    {
        WORD langID{ 0 };
        WORD charset{ 1252 };
    };

    std::vector<BYTE> m_versionInfo;
    VS_FIXEDFILEINFO m_fixedInfo{};
    Translation m_translation{};
};