#include "stdafx.h"
#include "SessionService.h"
#include "AppHost.h"

namespace $safeprojectname$
{
    void SessionService::Touch()
    {
        // Placeholder for lightweight startup wiring.
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
