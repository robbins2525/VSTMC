/*--------------------------------------------------------------------------------------+
|   $safeitemname$.h
|
+--------------------------------------------------------------------------------------*/

#pragma once

#include <Bentley/Bentley.h>
#include <Bentley/WString.h>

namespace $safeprojectname$
{
    struct AddinMetadata final
    {
        static constexpr wchar_t kTaskId[] = L"$safeitemname$";
        static constexpr wchar_t kDllName[] = L"$safeitemname$";
    };

    void Initialize();
    void RegisterCommands();
    void RegisterResources();
    void LogInfo(Bentley::WCharCP message);
}