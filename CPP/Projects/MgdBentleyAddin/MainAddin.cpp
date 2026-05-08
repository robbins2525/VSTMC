#include "Stdafx.h"
#include "MainAddin.h"
#include "AppHost.h"

namespace $safeprojectname$
{
    MainAddin::MainAddin(System::IntPtr mdlDescriptor) : Base(mdlDescriptor)
    {
        s_Instance = this;
    }

    MainAddin^ MainAddin::GetInstance()
    {
        return s_Instance;
    }

    int MainAddin::Run(array<System::String^>^ commandLine)
    {
        try
        {
            AppHost::Initialize(commandLine);
            return 0;
        }
        catch (System::Exception^)
        {
            return 1;
        }
    }

    void MainAddin::OnUnloaded(Bentley::MstnPlatformNET::AddIn::UnloadedEventArgs^ eventArgs)
    {
        AppHost::Shutdown();
        Base::OnUnloaded(eventArgs);
    }
}