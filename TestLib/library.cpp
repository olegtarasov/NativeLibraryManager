#include "library.h"

#include <iostream>

#if defined(_WIN32)
#define OS "Windows"
#elif defined(__linux__)
#define OS "Linux"
#elif defined(__APPLE__)
#define OS "MacOS"
#else
#define OS "Unknown OS"
#endif

int hello() {
    return 42;
}