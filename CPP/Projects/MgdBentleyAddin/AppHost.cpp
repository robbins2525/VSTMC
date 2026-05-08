#include "Stdafx.h"
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

        // Bentley-style core addin hook point:
        // - register event handlers
        // - initialize shared services
        // - prepare ribbon or docking integrations
        // - connect to other managed modules

        SessionService::Startup();
    }

    void AppHost::Shutdown()
    {
        if (!s_initialized)
            return;

        SessionService::Shutdown();
        s_initialized = false;
        s_commandLine = nullptr;
    }
}
