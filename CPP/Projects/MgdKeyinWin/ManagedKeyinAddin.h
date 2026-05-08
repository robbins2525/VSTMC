#pragma once
#include "stdafx.h"
#include "MainForm.h"

#using <mscorlib.dll>
#using <System.dll>
#using <System.Drawing.dll>
#using <System.Windows.Forms.dll>
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
        literal System::String^ DockableWindowId = L"$safeprojectname$.MainFormDockable";
        static Bentley::MstnPlatformNET::AddIn^ MSAddin = nullptr;
        static MainForm^ s_mainForm = nullptr;
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

        static MainForm^ GetOrCreateMainForm();
        static void ShowMainForm(String^ sourceText);
        static void HideMainForm();
		static void CloseMainForm();
        static void ShowAbout(String^ sourceText);

    private:
        static void EnsureDocked(MainForm^ form);
        static void ReleaseDockHost();
        static void OnMainFormClosed(Object^ sender, FormClosedEventArgs^ e);
    };
}
