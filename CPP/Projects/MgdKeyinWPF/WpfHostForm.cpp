#include "stdafx.h"
#include "WpfHostForm.h"

namespace $safeprojectname$
{
    WpfHostForm::WpfHostForm()
    {
        this->Text = L"Managed Key-in AddIn";
        this->StartPosition = System::Windows::Forms::FormStartPosition::CenterScreen;
        this->Size = System::Drawing::Size(560, 420);
        this->MinimumSize = System::Drawing::Size(500, 340);
        this->MaximizeBox = true;

        _elementHost = gcnew System::Windows::Forms::Integration::ElementHost();
        _elementHost->Dock = System::Windows::Forms::DockStyle::Fill;
        _toolControl = gcnew WpfToolControl();
        _elementHost->Child = _toolControl;

        this->Controls->Add(_elementHost);
    }

    void WpfHostForm::SetLaunchSource(System::String^ sourceText)
    {
        _toolControl->SetLaunchSource(sourceText);
    }
}
