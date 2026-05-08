#include "stdafx.h"
#include "WpfToolControl.h"
#include "$safeprojectname$.h"

namespace $safeprojectname$
{
    WpfToolControl::WpfToolControl()
    {
        System::Windows::Controls::DockPanel^ root = gcnew System::Windows::Controls::DockPanel();
        root->LastChildFill = true;
        root->Margin = System::Windows::Thickness(12.0);

        System::Windows::Controls::StackPanel^ topPanel = gcnew System::Windows::Controls::StackPanel();
        topPanel->Orientation = System::Windows::Controls::Orientation::Vertical;
        System::Windows::Controls::DockPanel::SetDock(topPanel, System::Windows::Controls::Dock::Top);

        System::Windows::Controls::TextBlock^ title = gcnew System::Windows::Controls::TextBlock();
        title->Text = L"Managed key-in starter (WPF host)";
        title->FontSize = 18.0;
        title->FontWeight = System::Windows::FontWeights::Bold;
        title->Margin = System::Windows::Thickness(0.0, 0.0, 0.0, 8.0);
        topPanel->Children->Add(title);

        _sourceText = gcnew System::Windows::Controls::TextBlock();
        _sourceText->Text = L"Opened from: startup";
        _sourceText->Margin = System::Windows::Thickness(0.0, 0.0, 0.0, 12.0);
        topPanel->Children->Add(_sourceText);

        root->Children->Add(topPanel);

        System::Windows::Controls::StackPanel^ buttonPanel = gcnew System::Windows::Controls::StackPanel();
        buttonPanel->Orientation = System::Windows::Controls::Orientation::Horizontal;
        buttonPanel->HorizontalAlignment = System::Windows::HorizontalAlignment::Right;
        buttonPanel->Margin = System::Windows::Thickness(0.0, 12.0, 0.0, 0.0);
        System::Windows::Controls::DockPanel::SetDock(buttonPanel, System::Windows::Controls::Dock::Bottom);

        _hideButton = gcnew System::Windows::Controls::Button();
        _hideButton->Content = L"Hide";
        _hideButton->MinWidth = 90.0;
        _hideButton->Margin = System::Windows::Thickness(0.0, 0.0, 8.0, 0.0);
        _hideButton->Click += gcnew System::Windows::RoutedEventHandler(this, &WpfToolControl::OnHideClicked);
        buttonPanel->Children->Add(_hideButton);

        _closeButton = gcnew System::Windows::Controls::Button();
        _closeButton->Content = L"Close";
        _closeButton->MinWidth = 90.0;
        _closeButton->Click += gcnew System::Windows::RoutedEventHandler(this, &WpfToolControl::OnCloseClicked);
        buttonPanel->Children->Add(_closeButton);

        root->Children->Add(buttonPanel);

        _notesBox = gcnew System::Windows::Controls::TextBox();
        _notesBox->AcceptsReturn = true;
        _notesBox->VerticalScrollBarVisibility = System::Windows::Controls::ScrollBarVisibility::Auto;
        _notesBox->HorizontalScrollBarVisibility = System::Windows::Controls::ScrollBarVisibility::Disabled;
        _notesBox->TextWrapping = System::Windows::TextWrapping::Wrap;
        _notesBox->MinHeight = 220.0;
        _notesBox->Text =
            L"This starter uses a WPF UserControl hosted inside a WinForms ElementHost, and the host form is docked through Bentley.Windowing.WindowManager.DockPanel(...).\r\n\r\n"
            L"Try these key-ins in MicroStation:\r\n"
            L"  $safeprojectname$ OPEN\r\n"
            L"  $safeprojectname$ HIDE\r\n"
            L"  MKA OPEN\r\n"
            L"  MKA ABOUT\r\n\r\n"
            L"OPEN creates or reuses the docked WPF tool and shows it again if it was hidden.";

        root->Children->Add(_notesBox);

        this->MinWidth = 420.0;
        this->MinHeight = 280.0;
        this->Content = root;
    }

    void WpfToolControl::SetLaunchSource(System::String^ sourceText)
    {
        _sourceText->Text = System::String::Format(L"Opened from: {0}", sourceText);
    }

    System::Void WpfToolControl::OnHideClicked(System::Object^ sender, System::Windows::RoutedEventArgs^ e)
    {
        Program::HideMainForm();
    }

    System::Void WpfToolControl::OnCloseClicked(System::Object^ sender, System::Windows::RoutedEventArgs^ e)
    {
        Program::CloseMainForm();
    }
}
