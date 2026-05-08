/*--------------------------------------------------------------------------------------+
|   $safeitemname$.cpp
|
+--------------------------------------------------------------------------------------*/

#include <Mstn\MdlApi\dlogitem.h>
#include "$safeprojectname$.h"

$AddinAttribute$

namespace
{
    void SafeStartup() noexcept
    {
        $safeprojectname$::RegisterResources();
        $safeprojectname$::RegisterCommands();
    }
}

namespace $safeprojectname$
{
    void LogInfo(Bentley::WCharCP message)
    {
        // Keep this boundary portable across Bentley SDK variants.
        // Some SDK layouts move or omit higher-level output headers.
        (void)message;
    }

    void RegisterCommands()
    {
        // Intentionally empty.
    }

    void RegisterResources()
    {
        // Intentionally empty.
    }

    void Initialize()
    {
        SafeStartup();
    }
}

extern "C" DLLEXPORT void MdlMain(int argc, WCharCP argv[])
{
    (void)argc;
    (void)argv;

    try
    {
        $safeprojectname$::Initialize();
    }
    catch (...)
    {
        $safeprojectname$::LogInfo(L"$safeprojectname$ failed during startup.");
    }
}
