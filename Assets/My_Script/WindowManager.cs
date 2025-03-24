using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WindowsInput;

public class WindowManager : MonoBehaviour
{
    [Header("This by default disabled, enabled automatically after Initialization script")]
    [SerializeField] private bool unusedBool;

    [SerializeField] private Button dummyButtonBlockRaycast;


    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
    
    [DllImport("Dwmapi.dll")]
    private static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    private static extern int ShowWindow(IntPtr hWnd, int nCmdShow);

    //Set window focus
    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentThreadId();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

    [DllImport("user32.dll")]
    private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern IntPtr SetFocus(IntPtr hWnd);

    //custom from rust
    [DllImport("rust_get_process_pid_from_modmanager", CallingConvention = CallingConvention.Cdecl)]
    private static extern int get_pid_by_name(string process_name);


    private const int GWL_EXSTYLE = -20;
    private const uint WS_EX_LAYERED = 0x00080000;
    private const uint WS_EX_TRANSPARENT = 0x00000020;
    private const int SW_HIDE = 0;
    private const int SW_SHOW = 5;
    static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

    private static IntPtr thisApphWnd;
    public static IntPtr targetGamehWnd; //Also used in KeyToggleWindow
    public static bool targetGamehWndFound = false;
    private bool windowIsHidden;

    [SerializeField] private Camera mainCam;

    //Activated after done initialization
    private void Start()
    {
        GetThisAppWindow();
        SetWindowTransparent();
        ToggleWindow();
    }

    public void ToggleWindow()
    {
        #if !UNITY_EDITOR
        if (windowIsHidden)
        {
            if(!targetGamehWndFound)
            {
                int targetGamePid = get_pid_by_name(Initialization.gameName);
                if(targetGamePid != -1)
                {
                    targetGamehWnd = Process.GetProcessById(targetGamePid).MainWindowHandle;
                }
            }
            
            if(GetForegroundWindow() == targetGamehWnd)
            {
                QualitySettings.vSyncCount = 1;
                mainCam.enabled = true;
                windowIsHidden = false;

                ShowWindow(thisApphWnd, SW_SHOW);

                SetWindowLong(thisApphWnd, GWL_EXSTYLE, WS_EX_LAYERED);
                var sim = new InputSimulator();
                sim.Mouse.LeftButtonClick();

                StartCoroutine(CheckCurrentActiveWindow());
                StartCoroutine(SetClickthrough());
                if(!targetGamehWndFound) StartCoroutine(SetTargetWindowIsFound());
                StartCoroutine(DisableDummyButtons(true));
            }
        }
        else
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 10;
            mainCam.enabled = false;
            windowIsHidden = true;

            ShowWindow(thisApphWnd, SW_HIDE);
            StartCoroutine(DisableDummyButtons(false));
        }
        #endif
    }

    // public static void ForceFocusWindow(bool isThisApp)//Used when simulating input, currently not used
    // {
    //     IntPtr selectedHwnd = isThisApp ? thisApphWnd : targetGamehWnd;

    //     // Get the current thread ID
    //     uint currentThreadId = GetCurrentThreadId();

    //     // Get the thread ID of the window
    //     uint windowThreadId = GetWindowThreadProcessId(selectedHwnd, IntPtr.Zero);

    //     // Attach the input processing mechanism of the current thread to the window's thread
    //     if (currentThreadId != windowThreadId)
    //     {
    //         AttachThreadInput(currentThreadId, windowThreadId, true);
    //     }

    //     // Set the window to the foreground
    //     SetForegroundWindow(selectedHwnd);

    //     // Set focus to the window
    //     SetFocus(selectedHwnd);

    //     // Detach the input processing mechanism
    //     if (currentThreadId != windowThreadId)
    //     {
    //         AttachThreadInput(currentThreadId, windowThreadId, false);
    //     }
    // }



    private void GetThisAppWindow()
    {
        thisApphWnd = Process.GetCurrentProcess().MainWindowHandle;
    }

    private void SetWindowTransparent()
    {
        #if !UNITY_EDITOR
        MARGINS margins = new MARGINS { cxLeftWidth = -1 };
        DwmExtendFrameIntoClientArea(thisApphWnd, ref margins);
        SetWindowLong(thisApphWnd, GWL_EXSTYLE, WS_EX_LAYERED);
        SetWindowPos(thisApphWnd, HWND_TOPMOST, 0, 0, 0, 0, 0);
        #endif
    }

    // private IntPtr CheckActiveWindow()
    // {
    //     IntPtr hWnd = GetForegroundWindow();
    //     return 
    // }

    private IEnumerator DisableDummyButtons(bool isShowWindow)
    {
        //If window activated
        if(isShowWindow)
        {
            yield return null;
            dummyButtonBlockRaycast.gameObject.SetActive(false);
            EventSystem.current.SetSelectedGameObject(null);
        }
        else
        {
            dummyButtonBlockRaycast.gameObject.SetActive(true);
        }
    }

    private IEnumerator CheckCurrentActiveWindow()
    {
        while (!windowIsHidden)
        {
            IntPtr currentActiveWindowHandle = GetForegroundWindow();
            if(currentActiveWindowHandle == targetGamehWnd || currentActiveWindowHandle == thisApphWnd)
            {

            }
            else
            {
                ToggleWindow();
            }
            yield return null;
        }
    }

    private IEnumerator SetClickthrough()
    {
        while(!windowIsHidden)
        {
            if (EventSystem.current.IsPointerOverGameObject() /*&& keysimulator not simulating key*/)
            {
                SetWindowLong(thisApphWnd, GWL_EXSTYLE, WS_EX_LAYERED);
            }
            else
            {
                SetWindowLong(thisApphWnd, GWL_EXSTYLE, WS_EX_LAYERED | WS_EX_TRANSPARENT);
            }
            yield return null;
        }
    }

    private IEnumerator SetTargetWindowIsFound()
    {
        yield return new WaitForSeconds(1);
        targetGamehWndFound = true;
    }
}

public struct MARGINS
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    }
