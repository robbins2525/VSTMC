#include "stdafx.h"
#include "$safeprojectname$.h"

namespace $safeprojectname$
{
    MainForm^ Program::GetOrCreateMainForm()
    {
        if (nullptr == s_mainForm || s_mainForm->IsDisposed)
        {
            s_mainForm = gcnew MainForm();
            s_mainForm->FormClosed += gcnew FormClosedEventHandler(&Program::OnMainFormClosed);
            s_windowContent = nullptr;
        }

        return s_mainForm;
    }

    void Program::EnsureDocked(MainForm^ form)
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
        MainForm^ form = GetOrCreateMainForm();
        form->SetLaunchSource(sourceText);

        EnsureDocked(form);

        if (nullptr != s_windowContent)
            s_windowContent->Show();

        if (!form->Visible)
            form->Show();

        form->BringToFront();
        form->Activate();
        form->Refresh();
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
        if (nullptr != s_mainForm)
        {
            MainForm^ form = s_mainForm;
            Bentley::Windowing::WindowContent^ host = s_windowContent;

            s_mainForm = nullptr;
            s_windowContent = nullptr;

            try
            {
                if (nullptr != host)
                    host->Close();
            }
            catch (...)
            {
            }

            try
            {
                if (!form->IsDisposed)
                    form->Close();
            }
            catch (...)
            {
            }

            try
            {
                if (!form->IsDisposed)
                    delete form;   // explicit Dispose()
            }
            catch (...)
            {
            }
        }
    }

    void Program::ShowAbout(String^ sourceText)
    {
        MessageBox::Show(
            String::Format(
                L"$safeprojectname$\r\n\r\nSource: {0}\r\n\r\nThis starter demonstrates:\r\n- Bentley-style managed key-ins\r\n- short alias key-ins\r\n- command table wiring\r\n- reopening a docked window from a key-in",
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
