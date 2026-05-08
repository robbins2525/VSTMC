#pragma once
#include "stdafx.h"

namespace $safeprojectname$
{
    public ref class MainForm : public System::Windows::Forms::Form
    {
    public:
        MainForm();
        void SetLaunchSource(System::String^ sourceText);

    private:
        System::Windows::Forms::Label^ _titleLabel;
        System::Windows::Forms::Label^ _sourceLabel;
        System::Windows::Forms::TextBox^ _notesBox;
        System::Windows::Forms::Button^ _hideButton;
        System::Windows::Forms::Button^ _closeButton;

        System::Void OnHideClicked(System::Object^ sender, System::EventArgs^ e);
        System::Void OnCloseClicked(System::Object^ sender, System::EventArgs^ e);
    };
}
