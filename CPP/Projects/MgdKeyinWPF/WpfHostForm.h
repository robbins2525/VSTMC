#pragma once
#include "stdafx.h"
#include "WpfToolControl.h"

namespace $safeprojectname$
{
    public ref class WpfHostForm : public System::Windows::Forms::Form
    {
    public:
        WpfHostForm();
        void SetLaunchSource(System::String^ sourceText);

    private:
        System::Windows::Forms::Integration::ElementHost^ _elementHost;
        WpfToolControl^ _toolControl;
    };
}
