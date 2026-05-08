#pragma once
#include "Stdafx.h"

namespace $safeprojectname$
{
    public ref class SessionService abstract sealed
    {
    private:
        static bool s_started = false;

    public:
        static void Startup();
        static void Shutdown();
        static property bool IsStarted
        {
            bool get() { return s_started; }
        }

        static System::String^ GetRuntimeSummary();
        static System::String^ GetStartupSummary();
    };
}
