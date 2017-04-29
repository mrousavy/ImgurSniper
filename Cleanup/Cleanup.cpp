// Cleanup.cpp : Defines the entry point for the console application.
//

#include <string>
#include <iostream>
#include "stdafx.h"
#include <windows.h>
#include <conio.h>
#include <shlobj.h>
#include <string.h>

#pragma comment(lib, "shell32.lib")

LPWSTR getDocuments() {
	LPWSTR my_documents = L"";
	HRESULT result = SHGetFolderPath(NULL, CSIDL_PERSONAL, NULL, SHGFP_TYPE_CURRENT, my_documents);

	if (result != S_OK)
		return NULL;
	else
		return my_documents;
}


int main()
{
	LPWSTR documents = getDocuments();
	if (documents == NULL) {
		printf("Could not detect Documents Folder!");
		return 1;
	}

	/*std::wstring documentswstring(documents);
	std::wstring configwstring = documentswstring + L"config.json";

	const WCHAR * configChar = configwstring.c_str();

	char* dest;
	wcstombs(dest, configChar, 100);

	if (remove(dest) != 0)
		printf("Could not remove config.json!");
	else
		printf("Successfully removed config.json!");*/

	// Wait for keystroke
	_getch();

	return 0;
}
