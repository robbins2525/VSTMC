#pragma once
#include "stdafx.h"

namespace $safeprojectname$
{
    public ref class WpfToolControl : public System::Windows::Controls::UserControl
    {
    public:
        WpfToolControl();
        void SetLaunchSource(System::String^ sourceText);

    private:
        System::Windows::Controls::TextBlock^ _sourceText;
        System::Windows::Controls::TextBox^ _notesBox;
        System::Windows::Controls::Button^ _hideButton;
        System::Windows::Controls::Button^ _closeButton;

        System::Void OnHideClicked(System::Object^ sender, System::Windows::RoutedEventArgs^ e);
        System::Void OnCloseClicked(System::Object^ sender, System::Windows::RoutedEventArgs^ e);
    };
}
