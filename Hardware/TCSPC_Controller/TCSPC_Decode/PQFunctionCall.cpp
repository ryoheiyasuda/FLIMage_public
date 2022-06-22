#include "PQFunctionCall.h"

typedef int(*Msg)(HWND, LPCTSTR, LPCTSTR, UINT);

PQFunctionCall::PQFunctionCall();

PQFunctionCall::~PQFunctionCall();

int PQFunctionCall::LoadPQLibrary()
{
	hPQ = LoadLibrary("User32.dll");
	if (hPQ == NULL)
		return -1;
	else
		return 0;
}


//int CallLibraryExample()
//{
//	HINSTANCE hDLL = LoadLibrary("User32.dll");
//	if (hDLL == NULL) {
//		std::cout << "Failed to load the library.\n";
//	}
//	else {
//		std::cout << "Library loaded.\n";
//		Msg MsgBox = (Msg)GetProcAddress(hDLL, "MessageBoxA");
//		MsgBox(0, "https://HelloACM.com", "Hello, World!", 0);
//	}
//	FreeLibrary(hDLL);
//	return 0;
//}