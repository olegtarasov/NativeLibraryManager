#ifndef TESTLIB_LIBRARY_H
#define TESTLIB_LIBRARY_H

#if defined DLL_EXPORTS
    #if defined WIN32
        #define LIB_API(RetType) extern "C" __declspec(dllexport) RetType
    #else
        #define LIB_API(RetType) extern "C" RetType __attribute__((visibility("default")))
    #endif
#else
    #if defined WIN32
        #define LIB_API(RetType) extern "C" __declspec(dllimport) RetType
    #else
        #define LIB_API(RetType) extern "C" RetType
    #endif
#endif

LIB_API(int) hello();

#endif //TESTLIB_LIBRARY_H