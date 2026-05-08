#pragma once
#include "Stdafx.h"

namespace $safeprojectname$
{
    [Bentley::MstnPlatformNET::AddInAttribute(MdlTaskID = L"$safeprojectname$")]
    public ref class MainAddin sealed : public Bentley::MstnPlatformNET::AddIn
    {
    private:
        typedef Bentley::MstnPlatformNET::AddIn Base;
        static MainAddin^ s_Instance = nullptr;

        MainAddin(System::IntPtr mdlDescriptor);

    public:
        static MainAddin^ GetInstance();

    protected:
        virtual int Run(array<System::String^>^ commandLine) override;
        virtual void OnUnloaded(Bentley::MstnPlatformNET::AddIn::UnloadedEventArgs^ eventArgs) override;
    };
}