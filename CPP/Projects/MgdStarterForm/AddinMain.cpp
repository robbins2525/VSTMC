#include "stdafx.h"
#include "AddinMain.h"
#include "AppHost.h"

namespace $safeprojectname$
{
    int AddinMain::Run(array<System::String^>^ commandLine)
    {
        try
        {
            AppHost::Initialize(commandLine);
            return 0;
        }
        catch (System::Exception^ ex)
        {
            System::Windows::Forms::MessageBox::Show(
                ex->ToString(),
                "$safeprojectname$",
                System::Windows::Forms::MessageBoxButtons::OK,
                System::Windows::Forms::MessageBoxIcon::Error);

            return 1;
        }
    }
}
