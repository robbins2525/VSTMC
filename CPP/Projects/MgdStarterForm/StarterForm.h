#pragma once
#include "stdafx.h"
#include "AppHost.h"
#include "SessionService.h"

namespace $safeprojectname$
{
    public ref class StarterForm : public System::Windows::Forms::Form
    {
    private:
        System::Windows::Forms::Label^ m_titleLabel;
        System::Windows::Forms::Label^ m_statusLabel;
        System::Windows::Forms::TextBox^ m_logBox;
        System::Windows::Forms::Button^ m_pingButton;
        System::Windows::Forms::Button^ m_runtimeButton;
        System::Windows::Forms::Button^ m_clearButton;
        System::Windows::Forms::Button^ m_closeButton;

    public:
        StarterForm()
        {
            InitializeComponent();
        }

        void AppendLog(System::String^ message)
        {
            if (m_logBox == nullptr)
                return;

            System::String^ line = System::String::Format(
                "[{0}] {1}",
                System::DateTime::Now.ToString("HH:mm:ss"),
                message);

            m_logBox->AppendText(line + System::Environment::NewLine);
        }

    private:
        void InitializeComponent()
        {
            this->Text = "$safeprojectname$";
            this->Width = 560;
            this->Height = 420;
            this->StartPosition = System::Windows::Forms::FormStartPosition::CenterScreen;
            this->FormBorderStyle = System::Windows::Forms::FormBorderStyle::Sizable;

            m_titleLabel = gcnew System::Windows::Forms::Label();
            m_titleLabel->Text = "Managed C++/CLI Starter";
            m_titleLabel->Left = 12;
            m_titleLabel->Top = 12;
            m_titleLabel->Width = 420;
            m_titleLabel->Height = 24;
            m_titleLabel->Font = gcnew System::Drawing::Font("Segoe UI", 11.0f, System::Drawing::FontStyle::Bold);

            m_statusLabel = gcnew System::Windows::Forms::Label();
            m_statusLabel->Text = "Ready";
            m_statusLabel->Left = 12;
            m_statusLabel->Top = 42;
            m_statusLabel->Width = 500;
            m_statusLabel->Height = 22;

            m_logBox = gcnew System::Windows::Forms::TextBox();
            m_logBox->Left = 12;
            m_logBox->Top = 72;
            m_logBox->Width = 520;
            m_logBox->Height = 250;
            m_logBox->Multiline = true;
            m_logBox->ScrollBars = System::Windows::Forms::ScrollBars::Vertical;
            m_logBox->ReadOnly = true;
            m_logBox->Font = gcnew System::Drawing::Font("Consolas", 9.0f);

            m_pingButton = gcnew System::Windows::Forms::Button();
            m_pingButton->Text = "Ping";
            m_pingButton->Left = 12;
            m_pingButton->Top = 332;
            m_pingButton->Width = 100;
            m_pingButton->Click += gcnew System::EventHandler(this, &StarterForm::OnPingClick);

            m_runtimeButton = gcnew System::Windows::Forms::Button();
            m_runtimeButton->Text = "Runtime";
            m_runtimeButton->Left = 122;
            m_runtimeButton->Top = 332;
            m_runtimeButton->Width = 100;
            m_runtimeButton->Click += gcnew System::EventHandler(this, &StarterForm::OnRuntimeClick);

            m_clearButton = gcnew System::Windows::Forms::Button();
            m_clearButton->Text = "Clear Log";
            m_clearButton->Left = 232;
            m_clearButton->Top = 332;
            m_clearButton->Width = 100;
            m_clearButton->Click += gcnew System::EventHandler(this, &StarterForm::OnClearClick);

            m_closeButton = gcnew System::Windows::Forms::Button();
            m_closeButton->Text = "Close";
            m_closeButton->Left = 432;
            m_closeButton->Top = 332;
            m_closeButton->Width = 100;
            m_closeButton->Click += gcnew System::EventHandler(this, &StarterForm::OnCloseClick);

            this->Controls->Add(m_titleLabel);
            this->Controls->Add(m_statusLabel);
            this->Controls->Add(m_logBox);
            this->Controls->Add(m_pingButton);
            this->Controls->Add(m_runtimeButton);
            this->Controls->Add(m_clearButton);
            this->Controls->Add(m_closeButton);

            this->Load += gcnew System::EventHandler(this, &StarterForm::OnFormLoad);
        }

        void OnFormLoad(System::Object^ sender, System::EventArgs^ e)
        {
            m_statusLabel->Text = "UI loaded";
            AppendLog("Starter window loaded.");
        }

        void OnPingClick(System::Object^ sender, System::EventArgs^ e)
        {
            m_statusLabel->Text = "Ping received";
            AppendLog("Ping button clicked.");
            System::Windows::Forms::MessageBox::Show(
                "$safeprojectname$ is alive.",
                "Ping",
                System::Windows::Forms::MessageBoxButtons::OK,
                System::Windows::Forms::MessageBoxIcon::Information);
        }

        void OnRuntimeClick(System::Object^ sender, System::EventArgs^ e)
        {
            m_statusLabel->Text = "Runtime summary shown";
            AppendLog(SessionService::GetRuntimeSummary());
            AppendLog(SessionService::GetStartupSummary());
        }

        void OnClearClick(System::Object^ sender, System::EventArgs^ e)
        {
            m_logBox->Clear();
            m_statusLabel->Text = "Log cleared";
            AppendLog("Log restarted.");
        }

        void OnCloseClick(System::Object^ sender, System::EventArgs^ e)
        {
            this->Close();
        }
    };
}
