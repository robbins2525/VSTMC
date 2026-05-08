#include "stdafx.h"
#include "$safeprojectname$.h"

namespace $safeprojectname$
{
    WpfHostForm^ Program::GetOrCreateMainForm()
    {
        if (nullptr == s_mainForm || s_mainForm->IsDisposed)
        {
            s_mainForm = gcnew WpfHostForm();
            s_mainForm->FormClosed += gcnew FormClosedEventHandler(&Program::OnMainFormClosed);
            s_windowContent = nullptr;
        }

        return s_mainForm;
    }

    void Program::EnsureDocked(WpfHostForm^ form)
    {
        if (nullptr != s_windowContent)
            return;

        Bentley::Windowing::WindowManager^ manager = Bentley::Windowing::WindowManager::GetForMicroStation();
        s_windowContent = manager->DockPanel(
            form,
            DockableWindowId,
            form->Text,
            Bentley::Windowing::DockLocation::Floating);
    }

    void Program::ShowMainForm(String^ sourceText)
    {
        WpfHostForm^ form = GetOrCreateMainForm();
        form->SetLaunchSource(sourceText);
        EnsureDocked(form);

        if (nullptr != s_windowContent)
            s_windowContent->Show();

        if (!form->Visible)
            form->Show();

        form->BringToFront();
        form->Activate();
    }

    void Program::HideMainForm()
    {
        if (nullptr != s_windowContent)
            s_windowContent->Hide();
        else
            if (nullptr != s_mainForm && !s_mainForm->IsDisposed)
                s_mainForm->Hide();
    }

    void Program::CloseMainForm()
    {
        if (nullptr != s_mainForm && !s_mainForm->IsDisposed)
        {
            WpfHostForm^ form = s_mainForm;

            s_mainForm = nullptr;
            s_windowContent->Close();
            s_windowContent = nullptr;

            form->Close();
        }
    }



    void Program::ShowAbout(String^ sourceText)
    {
        MessageBox::Show(
            String::Format(
                L"$safeprojectname$\r\n\r\nSource: {0}\r\n\r\nThis starter demonstrates:\r\n- Bentley-style managed key-ins\r\n- short alias key-ins\r\n- command table wiring\r\n- reopening a docked WPF tool from a key-in\r\n- WPF content hosted through ElementHost inside a dockable form",
                sourceText),
            L"$safeprojectname$",
            MessageBoxButtons::OK,
            MessageBoxIcon::Information);
    }

    void Program::ReleaseDockHost()
    {
        s_windowContent = nullptr;
    }

    void Program::OnMainFormClosed(Object^ sender, FormClosedEventArgs^ e)
    {
        s_windowContent = nullptr;
        s_mainForm = nullptr;
    }
}
