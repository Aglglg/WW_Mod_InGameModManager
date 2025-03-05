using System;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

public class KeyToggleWindow : MonoBehaviour
{
    private Thread keyThread;
    private bool running = true;

    // Windows API function for key state detection
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    private WindowManager windowManager;

    private void Start()
    {
        windowManager = GetComponent<WindowManager>();
        keyThread = new Thread(DetectKeys) { IsBackground = true };
        keyThread.Start();
    }

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

            Thread.Sleep(100); // Check key every 100ms
        }
    }

    private void OnDestroy()
    {
        running = false;
        keyThread.Join(); // Ensure thread stops properly
    }
}