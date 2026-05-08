#include "$safeprojectname$ids.h"
#include "app\App.h"
#include "diagnostics\Logger.h"

#include <Mstn\MdlApi\mdlapi.h>
#include <Mstn\ISessionMgr.h>

extern "C" DLLEXPORT void MdlMain(int argc, WCharCP argv[])
{
    (void)argc;
    (void)argv;

    if (!App::Instance().Initialize())
        Diagnostics::Logger::Error(L"$safeprojectname$", L"Initialization failed.");

}