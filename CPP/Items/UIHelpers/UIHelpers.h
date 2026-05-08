#pragma once

#include <string>
#include <windows.h>

#include <Mstn/MdlApi/mdlOutput.h>
#include <DgnPlatform/DgnPlatformApi.h>

namespace $rootnamespace$
{
    namespace BD = Bentley::DgnPlatform;

    inline std::wstring ToWString(std::string const& text)
    {
        return std::wstring(text.begin(), text.end());
    }

    inline void ShowInfo(std::wstring const& message, std::wstring const& title = L"Information")
    {
        mdlOutput_messageCenter(
            BD::OutputMessagePriority::Info,
            title.c_str(),
            message.c_str(),
            BD::OutputMessageAlert::Dialog
        );
    }

    inline void ShowWarning(std::wstring const& message, std::wstring const& title = L"Warning")
    {
        mdlOutput_messageCenter(
            BD::OutputMessagePriority::Warning,
            title.c_str(),
            message.c_str(),
            BD::OutputMessageAlert::Dialog
        );
    }

    inline void ShowError(std::wstring const& message, std::wstring const& title = L"Error")
    {
        mdlOutput_messageCenter(
            BD::OutputMessagePriority::Error,
            title.c_str(),
            message.c_str(),
            BD::OutputMessageAlert::Dialog
        );
    }

    inline void ShowError(std::string const& message, std::wstring const& title = L"Error")
    {
        ShowError(ToWString(message), title);
    }

    inline void ShowException(std::exception const& ex, std::wstring const& prefix = L"Operation failed")
    {
        std::wstring msg = prefix + L": " + ToWString(ex.what());
        ShowError(msg);
    }

    inline void ShowUnknownException(std::wstring const& prefix = L"Operation failed")
    {
        ShowError(prefix + L": unknown exception");
    }

    inline void ShowInfoBox(std::wstring const& message, std::wstring const& title = L"Information")
    {
        ::MessageBoxW(nullptr, message.c_str(), title.c_str(), MB_OK | MB_ICONINFORMATION);
    }

    inline void ShowErrorBox(std::wstring const& message, std::wstring const& title = L"Error")
    {
        ::MessageBoxW(nullptr, message.c_str(), title.c_str(), MB_OK | MB_ICONERROR);
    }
}