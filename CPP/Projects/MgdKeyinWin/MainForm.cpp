#include "stdafx.h"
#include "MainForm.h"
#include "$safeprojectname$.h"

namespace $safeprojectname$
{
    MainForm::MainForm()
    {
        this->Text = L"Managed Key-in AddIn";
        this->StartPosition = System::Windows::Forms::FormStartPosition::CenterScreen;
        this->Size = System::Drawing::Size(520, 340);
        this->MinimumSize = System::Drawing::Size(520, 340);
        this->MaximizeBox = false;

        _titleLabel = gcnew System::Windows::Forms::Label();
        _titleLabel->AutoSize = true;
        _titleLabel->Location = System::Drawing::Point(16, 16);
        _titleLabel->Font = gcnew System::Drawing::Font(L"Segoe UI", 12.0f, System::Drawing::FontStyle::Bold);
        _titleLabel->Text = L"Managed key-in starter (dockable host)";

        _sourceLabel = gcnew System::Windows::Forms::Label();
        _sourceLabel->AutoSize = true;
        _sourceLabel->Location = System::Drawing::Point(18, 50);
        _sourceLabel->Text = L"Opened from: startup";

        _notesBox = gcnew System::Windows::Forms::TextBox();
        _notesBox->Multiline = true;
        _notesBox->ScrollBars = System::Windows::Forms::ScrollBars::Vertical;
        _notesBox->Location = System::Drawing::Point(20, 82);
        _notesBox->Size = System::Drawing::Size(462, 170);
        _notesBox->Text =
            L"This starter window is hosted through Bentley.Windowing.WindowManager.DockPanel(...).\r\n\r\n"
            L"Try these key-ins in MicroStation:\r\n"
            L"  $safeprojectname$ OPEN\r\n"
            L"  $safeprojectname$ HIDE\r\n"
            L"  MKA OPEN\r\n"
            L"  MKA ABOUT\r\n\r\n"
            L"OPEN creates or reuses the form and re-docks or shows it if it was hidden.";

        _hideButton = gcnew System::Windows::Forms::Button();
        _hideButton->Text = L"Hide";
        _hideButton->Location = System::Drawing::Point(316, 266);
        _hideButton->Click += gcnew System::EventHandler(this, &MainForm::OnHideClicked);

        _closeButton = gcnew System::Windows::Forms::Button();
        _closeButton->Text = L"Close";
        _closeButton->Location = System::Drawing::Point(407, 266);
        _closeButton->Click += gcnew System::EventHandler(this, &MainForm::OnCloseClicked);

        this->Controls->Add(_titleLabel);
        this->Controls->Add(_sourceLabel);
        this->Controls->Add(_notesBox);
        this->Controls->Add(_hideButton);
        this->Controls->Add(_closeButton);
    }

    void MainForm::SetLaunchSource(System::String^ sourceText)
    {
        _sourceLabel->Text = System::String::Format(L"Opened from: {0}", sourceText);
    }

    System::Void MainForm::OnHideClicked(System::Object^ sender, System::EventArgs^ e)
    {
        Program::HideMainForm();
    }

    System::Void MainForm::OnCloseClicked(System::Object^ sender, System::EventArgs^ e)
    {
        Program::CloseMainForm();
    }
}
