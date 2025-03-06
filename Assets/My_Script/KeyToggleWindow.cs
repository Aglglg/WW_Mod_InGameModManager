using System;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

public class KeyToggleWindow : MonoBehaviour
{
    private Thread keyThread;
    private Thread targetGameCheckerThread;
    private bool running = true;

    // Windows API function for key state detection
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);
    [DllImport("user32.dll")]
    private static extern bool IsWindow(IntPtr hWnd);

    private WindowManager windowManager;

    private const int msDelayForChecking = 100;

    private void Start()
    {
        windowManager = GetComponent<WindowManager>();
        keyThread = new Thread(DetectKeys) { IsBackground = true };
        keyThread.Start();
        targetGameCheckerThread = new Thread(CheckTargetGameIsRunning) { IsBackground = true };
        targetGameCheckerThread.Start();
    }

    //Check for F5 even if it's in background
    private void DetectKeys()
    {
        int lastKeyPressTime = 0; // Tracks the last time the key was pressed

        while (running)
        {
            bool isKeyPressed = (GetAsyncKeyState(0x74) & 0x8000) != 0; // F5 key

            if (isKeyPressed && Environment.TickCount - lastKeyPressTime > 200) // 200ms cooldown
            {
                lastKeyPressTime = Environment.TickCount;

                // Execute ToggleWindow on the main thread
                UnityMainThreadDispatcher.Instance.Enqueue(() => windowManager.ToggleWindow());
            }

            Thread.Sleep(msDelayForChecking); // Check key every 100ms
        }
    }

    //Check if target game window is still opened, otherwise close this app automatically
    private void CheckTargetGameIsRunning()
    {
        while (running)
        {
            if(WindowManager.targetGamehWndFound)
            {
                if(!IsWindow(WindowManager.targetGamehWnd))
                {
                    #if !UNITY_EDITOR
                    Application.Quit();
                    #endif
                    Debug.Log("Target game not running, quitting");
                }
            }

            Thread.Sleep(msDelayForChecking); // Check key every 100ms
        }
    }

    private void OnDestroy()
    {
        running = false;
        keyThread.Join(); // Ensure thread stops properly
        targetGameCheckerThread.Join();
    }
}