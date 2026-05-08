#include "Logger.h"

#include <windows.h>
#include <cwchar>

namespace
{
    template <typename T, size_t N>
    constexpr size_t ArrayCount(T const (&)[N]) noexcept
    {
        return N;
    }

    void WriteDebugLine(Bentley::WCharCP text)
    {
        if (nullptr == text)
            return;

        ::OutputDebugStringW(text);
        ::OutputDebugStringW(L"\r\n");
    }

    void BuildMessage(wchar_t* buffer, size_t bufferCount, Bentley::WCharCP prefix, Bentley::WCharCP message)
    {
        if (nullptr == buffer || 0 == bufferCount)
            return;

        _snwprintf_s(
            buffer,
            bufferCount,
            _TRUNCATE,
            L"%ls%ls",
            (nullptr != prefix ? prefix : L""),
            (nullptr != message ? message : L"")
        );
    }

    void ShowMessageBox(UINT flags, Bentley::WCharCP title, Bentley::WCharCP message, Bentley::WCharCP prefix)
    {
        wchar_t buffer[1024] = { 0 };
        BuildMessage(buffer, ArrayCount(buffer), prefix, message);
        WriteDebugLine(buffer);
        ::MessageBoxW(nullptr, buffer, (nullptr != title ? title : L"$safeprojectname$"), flags);
    }
}

namespace Diagnostics
{
    void Logger::Debug(Bentley::WCharCP message)
    {
        WriteDebugLine(message);
    }

    void Logger::Info(Bentley::WCharCP title, Bentley::WCharCP message)
    {
        ShowMessageBox(MB_OK | MB_ICONINFORMATION, title, message, L"");
    }

    void Logger::Warning(Bentley::WCharCP title, Bentley::WCharCP message)
    {
        ShowMessageBox(MB_OK | MB_ICONWARNING, title, message, L"");
    }

    void Logger::Error(Bentley::WCharCP title, Bentley::WCharCP message)
    {
        ShowMessageBox(MB_OK | MB_ICONERROR, title, message, L"");
    }
}