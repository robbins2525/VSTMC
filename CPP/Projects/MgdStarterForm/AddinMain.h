#pragma once
#include "stdafx.h"

namespace $safeprojectname$
{
    [Bentley::MstnPlatformNET::AddInAttribute(MdlTaskID = L"$safeprojectname$")]
    public ref class AddinMain : public Bentley::MstnPlatformNET::AddIn
    {
    private:
        static Bentley::MstnPlatformNET::AddIn^ s_addin = nullptr;

    public:
        AddinMain(System::IntPtr mdlDesc)
            : Bentley::MstnPlatformNET::AddIn(mdlDesc)
        {
            s_addin = this;
        }

        static property Bentley::MstnPlatformNET::AddIn^ Current
        {
            Bentley::MstnPlatformNET::AddIn^ get() { return s_addin; }
        }

    protected:
        virtual int Run(array<System::String^>^ commandLine) override;
    };
}
