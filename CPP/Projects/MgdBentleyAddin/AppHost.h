#pragma once
#include "Stdafx.h"

namespace $safeprojectname$
{
    public ref class AppHost abstract sealed
    {
    private:
        static System::DateTime s_startTime = System::DateTime::MinValue;
        static array<System::String^>^ s_commandLine = nullptr;
        static bool s_initialized = false;

    public:
        static void Initialize(array<System::String^>^ commandLine);
        static void Shutdown();

        static property bool IsInitialized
        {
            bool get() { return s_initialized; }
        }

        static property System::DateTime StartTime
        {
            System::DateTime get() { return s_startTime; }
        }

        static property array<System::String^>^ CommandLine
        {
            array<System::String^>^ get() { return s_commandLine; }
        }
    };
}
