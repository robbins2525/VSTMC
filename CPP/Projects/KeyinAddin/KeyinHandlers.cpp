#include "KeyinHandlers.h"

#include "shared\Common.h"
#include <windows.h>

namespace Commands
{
    void KeyinHandlers::Hello(Bentley::WCharCP unparsed)
    {
        UNUSED(unparsed);
        
        ::MessageBoxW(
            nullptr,
            L"Hello from $safeprojectname$.",
            L"$safeprojectname$",
            MB_OK | MB_ICONINFORMATION
        );
    }

    void KeyinHandlers::About(Bentley::WCharCP unparsed)
    {
        UNUSED(unparsed);

        ::MessageBoxW(
            nullptr,
            L"$safeprojectname$ native Bentley add-in loaded successfully.",
            L"$safeprojectname$",
            MB_OK | MB_ICONINFORMATION
        );
    }
}