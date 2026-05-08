#pragma once
#include "stdafx.h"
#include "WpfHostForm.h"

#using <mscorlib.dll>
#using <System.dll>
#using <System.Drawing.dll>
#using <System.Windows.Forms.dll>
#using <WindowsBase.dll>
#using <PresentationCore.dll>
#using <PresentationFramework.dll>
#using <WindowsFormsIntegration.dll>
#using <Bentley.DgnDisplayNet.dll>
#using <Bentley.DgnPlatformNET.dll>
#using <Bentley.General.1.0.dll>
#using <Bentley.GeometryNET.dll>
#using <Bentley.MicroStation.Interfaces.1.0.dll>
#using <Bentley.MicroStation.WPF.dll>
#using <Bentley.Windowing.dll>
#using <ustation.dll>

using namespace System;
using namespace System::Windows::Forms;

namespace $safeprojectname$
{
    [Bentley::MstnPlatformNET::AddInAttribute(MdlTaskID = L"$safeprojectname$")]
    public ref class Program : public Bentley::MstnPlatformNET::AddIn
    {
    internal:
        literal System::String^ DockableWindowId = L"$safeprojectname$.WpfDockable";
        static Bentley::MstnPlatformNET::AddIn^ MSAddin = nullptr;
        static WpfHostForm^ s_mainForm = nullptr;
        static Bentley::Windowing::WindowContent^ s_windowContent = nullptr;

    public:
        Program(System::IntPtr mdlDesc) : Bentley::MstnPlatformNET::AddIn(mdlDesc)
        {
            MSAddin = this;
        }

        virtual int Run(array<String^>^ commandLine) override
        {
            return 0;
        }

        static Bentley::MstnPlatformNET::AddIn^ GetApp()
        {
            return MSAddin;
        }

        static WpfHostForm^ GetOrCreateMainForm();
        static void ShowMainForm(String^ sourceText);
        static void HideMainForm();
        static void CloseMainForm();
        static void ShowAbout(String^ sourceText);

    private:
        static void EnsureDocked(WpfHostForm^ form);
        static void ReleaseDockHost();
        static void OnMainFormClosed(Object^ sender, FormClosedEventArgs^ e);
    };
}
