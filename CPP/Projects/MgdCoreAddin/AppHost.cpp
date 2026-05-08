#include "stdafx.h"
#include "AppHost.h"
#include "SessionService.h"

namespace $safeprojectname$
{
    void AppHost::Initialize(array<System::String^>^ commandLine)
    {
        if (s_initialized)
            return;

        s_startTime = System::DateTime::Now;
        s_commandLine = commandLine;
        s_initialized = true;

        // Starter hook point:
        // - register event handlers
        // - initialize shared services
        // - prepare ribbon/docking integrations
        // - connect to other managed modules

        SessionService::Touch();
    }

    void AppHost::Shutdown()
    {
        s_initialized = false;
    }
}
