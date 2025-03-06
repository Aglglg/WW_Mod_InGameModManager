using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WindowManager : MonoBehaviour
{
    [Header("This by default disabled, enabled automatically after Initialization script")]
    [SerializeField] private bool unusedBool;

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

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
            string activeWindow = GetActiveWindowTitle();
            if (!string.IsNullOrEmpty(activeWindow))
            {
                if(activeWindow.Contains(Initialization.gameName))
                {
                    targetGamehWnd = GetForegroundWindow();
                    QualitySettings.vSyncCount = 1;
                    windowIsHidden = false;

                    ShowWindow(thisApphWnd, SW_SHOW);

                    ForceFocusWindow(true);
                    StartCoroutine(CheckCurrentActiveWindow());
                    StartCoroutine(SetClickthrough());
                    if(!targetGamehWndFound) StartCoroutine(SetTargetWindowIsFound());
                }
            }
        }
        else
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 10;
            windowIsHidden = true;
            ShowWindow(thisApphWnd, SW_HIDE);
        }
        #endif
    }

    public static void ForceFocusWindow(bool isThisApp)
    {
        IntPtr selectedHwnd = isThisApp ? thisApphWnd : targetGamehWnd;

        // Get the current thread ID
        uint currentThreadId = GetCurrentThreadId();

        // Get the thread ID of the window
        uint windowThreadId = GetWindowThreadProcessId(selectedHwnd, IntPtr.Zero);

        // Attach the input processing mechanism of the current thread to the window's thread
        if (currentThreadId != windowThreadId)
        {
            AttachThreadInput(currentThreadId, windowThreadId, true);
        }

        // Set the window to the foreground
        SetForegroundWindow(selectedHwnd);

        // Set focus to the window
        SetFocus(selectedHwnd);

        // Detach the input processing mechanism
        if (currentThreadId != windowThreadId)
        {
            AttachThreadInput(currentThreadId, windowThreadId, false);
        }
    }



    private void GetThisAppWindow()
    {
        thisApphWnd = GetActiveWindow();
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

    private string GetActiveWindowTitle()
    {
        IntPtr hWnd = GetForegroundWindow();
        if (hWnd == IntPtr.Zero)
            return null;

        StringBuilder title = new StringBuilder(256);
        GetWindowText(hWnd, title, title.Capacity);
        
        return title.Length > 0 ? title.ToString() : null;
    }

    private IEnumerator CheckCurrentActiveWindow()
    {
        while (!windowIsHidden)
        {
            string activeWindow = GetActiveWindowTitle();
            if (!string.IsNullOrEmpty(activeWindow))
            {
                if(activeWindow.Contains(Initialization.gameName) || activeWindow.Contains(Application.productName))
                {

                }
                else
                {
                    ToggleWindow();
                }
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
