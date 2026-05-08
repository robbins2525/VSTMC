#include "stdafx.h"
#include "AddinMain.h"
#include "AppHost.h"

namespace $safeprojectname$
{
    int AddinMain::Run(array<System::String^>^ commandLine)
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
}
