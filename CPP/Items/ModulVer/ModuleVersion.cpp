////////////////////////////////////////////////////////////////
//
//  ModuleVersion utility
//
//  Original concept based on the CModuleVersion class from
//  Paul DiLascia, Microsoft Systems Journal (1998).
//
//  Adapted for MicroStation development with contributions by
//  Jon Summers, Bentley MVP, Director, LA Solutions
//  https://www.la-solutions.co.uk/
//
//  This version has been modernized for contemporary C++
//  (memory safety improvements, RAII cleanup, etc.).
//
////////////////////////////////////////////////////////////////

#include "ModuleVersion.h"

#include <cwchar>

BOOL CModuleVersion::GetFileVersionInfo(const wchar_t* moduleName)
{
	m_translation.charset = 1252;
	m_translation.langID = 0;
	std::memset(&m_fixedInfo, 0, sizeof(m_fixedInfo));
	m_versionInfo.clear();

	wchar_t filename[_MAX_PATH] = {};
	HMODULE hModule = ::GetModuleHandleW(moduleName);

	if (hModule == nullptr && moduleName != nullptr)
		return FALSE;

	const DWORD nameLen = ::GetModuleFileNameW(hModule, filename, _countof(filename));
	if (nameLen == 0 || nameLen >= _countof(filename))
		return FALSE;

	DWORD dummyHandle = 0;
	const DWORD versionInfoSize = ::GetFileVersionInfoSizeW(filename, &dummyHandle);
	if (versionInfoSize == 0)
		return FALSE;

	m_versionInfo.resize(versionInfoSize);

	if (!::GetFileVersionInfoW(filename, 0, versionInfoSize, m_versionInfo.data()))
		return FALSE;

	LPVOID fixedInfoPtr = nullptr;
	UINT fixedInfoLen = 0;
	if (!::VerQueryValueW(m_versionInfo.data(), L"\\", &fixedInfoPtr, &fixedInfoLen))
		return FALSE;

	if (fixedInfoLen < sizeof(VS_FIXEDFILEINFO) || fixedInfoPtr == nullptr)
		return FALSE;

	m_fixedInfo = *static_cast<VS_FIXEDFILEINFO*>(fixedInfoPtr);

	LPVOID translationPtr = nullptr;
	UINT translationLen = 0;
	if (::VerQueryValueW(m_versionInfo.data(),
		L"\\VarFileInfo\\Translation",
		&translationPtr,
		&translationLen) &&
		translationPtr != nullptr &&
		translationLen >= sizeof(Translation))
	{
		m_translation = *static_cast<Translation*>(translationPtr);
	}

	return m_fixedInfo.dwSignature == VS_FFI_SIGNATURE;
}

std::wstring CModuleVersion::GetValue(const wchar_t* keyName) const
{
	if (m_versionInfo.empty() || keyName == nullptr)
		return {};

	wchar_t query[64] = {};
	swprintf_s(
		query,
		L"\\StringFileInfo\\%04x%04x\\%s",
		m_translation.langID,
		m_translation.charset,
		keyName);

	LPVOID valueBuffer = nullptr;
	UINT valueLen = 0;

	if (::VerQueryValueW(
		m_versionInfo.data(),
		query,
		&valueBuffer,
		&valueLen) &&
		valueBuffer != nullptr)
	{
		return static_cast<const wchar_t*>(valueBuffer);
	}

	return {};
}

using DllGetVersionProc = HRESULT(CALLBACK*)(DLLVERSIONINFO*);

BOOL CModuleVersion::DllGetVersion(const wchar_t* moduleName, DLLVERSIONINFO& dvi)
{
	HINSTANCE hinst = ::LoadLibraryW(moduleName);
	if (hinst == nullptr)
		return FALSE;

	const auto freeLibrary = [&]() { ::FreeLibrary(hinst); };

	DllGetVersionProc dllGetVersion =
		reinterpret_cast<DllGetVersionProc>(::GetProcAddress(hinst, "DllGetVersion"));

	if (dllGetVersion == nullptr)
	{
		freeLibrary();
		return FALSE;
	}

	std::memset(&dvi, 0, sizeof(dvi));
	dvi.cbSize = sizeof(dvi);

	const BOOL succeeded = SUCCEEDED(dllGetVersion(&dvi)) ? TRUE : FALSE;
	freeLibrary();
	return succeeded;
}