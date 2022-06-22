#pragma once

#include "targetver.h"

#define WIN32_LEAN_AND_MEAN

#include <stdio.h>
#include <windows.h>
#include <iostream>
#include <cmath>
#include <thread>
#include <future>
#include <functional>
#include <array>
#include <mutex> 
#include <iterator>
#include <memory>
#include <sstream>
#include <regex>
#include <algorithm>
#include <set>
#include <winsock.h>
#include <filesystem>


using namespace std;

#define DllExport_int extern "C" int __declspec( dllexport )__cdecl
#define DllExport extern "C" void __declspec( dllexport )__cdecl
