#include "stdafx.h"
#include "AppHost.h"
#include "StarterForm.h"

namespace $safeprojectname$
{
    void AppHost::Initialize(array<System::String^>^ commandLine)
    {
        s_startTime = System::DateTime::Now;
        s_commandLine = commandLine;

        ShowMainWindow();
        Log("$safeprojectname$ initialized.");
        Log("Startup time: " + s_startTime.ToString("yyyy-MM-dd HH:mm:ss"));

        if (s_commandLine != nullptr && s_commandLine->Length > 0)
        {
            Log("Command line arguments:");
            for each (System::String ^ arg in s_commandLine)
                Log("  - " + arg);
        }
        else
        {
            Log("No command line arguments.");
        }
    }

    void AppHost::ShowMainWindow()
    {
        if (s_form == nullptr || s_form->IsDisposed)
            s_form = gcnew StarterForm();

        s_form->Show();
        s_form->BringToFront();
    }

    void AppHost::Log(System::String^ message)
    {
        if (s_form != nullptr && !s_form->IsDisposed)
            s_form->AppendLog(message);
    }
}
