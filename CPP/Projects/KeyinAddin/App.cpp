#include "App.h"

#include "diagnostics\Logger.h"
#include "platform\BentleyApi.h"

App& App::Instance()
{
    static App instance;
    return instance;
}

bool App::Initialize()
{
    if (!Platform::BentleyApi::OpenResources())
    {
        Diagnostics::Logger::Error(L"$safeprojectname$", L"Failed to open Bentley resources.");
        return false;
    }

    if (!Platform::BentleyApi::RegisterCommands())
    {
        Diagnostics::Logger::Error(L"$safeprojectname$", L"Failed to register command handlers.");
        return false;
    }

    if (!Platform::BentleyApi::LoadCommandTable())
    {
        Diagnostics::Logger::Error(L"$safeprojectname$", L"Failed to load command table.");
        return false;
    }

    Diagnostics::Logger::Debug(L"$safeprojectname$ initialized.");
    return true;
}

void App::Shutdown()
{
    Diagnostics::Logger::Debug(L"$safeprojectname$ shutdown.");
}