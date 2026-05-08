#pragma once
#include "stdafx.h"

namespace $safeprojectname$
{
    public ref class SessionService abstract sealed
    {
    public:
        static System::String^ GetRuntimeSummary();
        static System::String^ GetStartupSummary();
    };
}
