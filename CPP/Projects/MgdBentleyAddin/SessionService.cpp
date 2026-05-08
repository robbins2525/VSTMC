#include "Stdafx.h"
#include "SessionService.h"
#include "AppHost.h"

namespace $safeprojectname$
{
    void SessionService::Startup()
    {
        s_started = true;
    }

    void SessionService::Shutdown()
    {
        s_started = false;
    }

    System::String^ SessionService::GetRuntimeSummary()
    {
        return "CLR: " + System::Environment::Version->ToString();
    }

    System::String^ SessionService::GetStartupSummary()
    {
        return "Addin started: " + AppHost::StartTime.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
