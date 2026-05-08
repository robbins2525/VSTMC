#pragma once
#include "stdafx.h"

namespace $safeprojectname$
{
    ref class StarterForm;

    public ref class AppHost abstract sealed
    {
    private:
        static StarterForm^ s_form = nullptr;
        static System::DateTime s_startTime = System::DateTime::MinValue;
        static array<System::String^>^ s_commandLine = nullptr;

    public:
        static void Initialize(array<System::String^>^ commandLine);
        static void Log(System::String^ message);
        static void ShowMainWindow();

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
