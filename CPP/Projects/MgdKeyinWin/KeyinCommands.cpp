#include "StdAfx.h"
#include "$safeprojectname$.h"

#using <mscorlib.dll>
#using <System.dll>
#using <System.Drawing.dll>
#using <System.Windows.Forms.dll>
#using <Bentley.DgnDisplayNet.dll>
#using <Bentley.DgnPlatformNET.dll>
#using <Bentley.General.1.0.dll>
#using <Bentley.GeometryNET.dll>
#using <Bentley.Interop.MicroStationDGN.dll>
#using <Bentley.MicroStation.Interfaces.1.0.dll>
#using <Bentley.MicroStation.dll>
#using <Bentley.MicroStation.WinForms.Docking.dll>
#using <Bentley.MicroStation.WPF.dll>
#using <Bentley.Windowing.dll>
#using <ustation.dll>

using namespace System;

namespace $safeprojectname$
{
    public ref class KeyinCommands
    {
    private:
        static String^ BuildSourceLabel(String^ route, String^ unparsed)
        {
            if (String::IsNullOrWhiteSpace(unparsed))
                return route;

            return String::Format(L"{0} ({1})", route, unparsed->Trim());
        }

    public:
        static void OpenBentleyStyle(String^ unparsed)
        {
            Program::ShowMainForm(BuildSourceLabel(L"$safeprojectname$ OPEN", unparsed));
        }

        static void ShowBentleyStyle(String^ unparsed)
        {
            Program::ShowMainForm(BuildSourceLabel(L"$safeprojectname$ SHOW", unparsed));
        }

        static void HideBentleyStyle(String^ unparsed)
        {
            Program::HideMainForm();
        }

        static void AboutBentleyStyle(String^ unparsed)
        {
            Program::ShowAbout(BuildSourceLabel(L"$safeprojectname$ ABOUT", unparsed));
        }

        static void OpenSimple(String^ unparsed)
        {
            Program::ShowMainForm(BuildSourceLabel(L"MKA OPEN", unparsed));
        }

        static void ShowSimple(String^ unparsed)
        {
            Program::ShowMainForm(BuildSourceLabel(L"MKA SHOW", unparsed));
        }

        static void HideSimple(String^ unparsed)
        {
            Program::HideMainForm();
        }

        static void AboutSimple(String^ unparsed)
        {
            Program::ShowAbout(BuildSourceLabel(L"MKA ABOUT", unparsed));
        }
    };
}
