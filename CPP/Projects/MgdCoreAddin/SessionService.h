#pragma once
#include "stdafx.h"

namespace $safeprojectname$
{
    public ref class SessionService abstract sealed
    {
    public:
        static void Touch();
        static System::String^ GetRuntimeSummary();
        static System::String^ GetStartupSummary();
    };
}
