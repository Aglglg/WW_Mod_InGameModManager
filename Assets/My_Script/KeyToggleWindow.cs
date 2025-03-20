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

    #region GAMEPAD Xinput
    [DllImport("xinput1_4.dll")]
    private static extern int XInputGetState(int dwUserIndex, out XINPUT_STATE pState);

    [StructLayout(LayoutKind.Sequential)]
    private struct XINPUT_STATE
    {
        public uint dwPacketNumber;
        public XINPUT_GAMEPAD Gamepad;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct XINPUT_GAMEPAD
    {
        public ushort wButtons;
        public byte bLeftTrigger;
        public byte bRightTrigger;
        public short sThumbLX;
        public short sThumbLY;
        public short sThumbRX;
        public short sThumbRY;
    }

    private const int XINPUT_GAMEPAD_LEFT_THUMB = 0x0040;
    private const int XINPUT_GAMEPAD_B = 0x2000;
    #endregion

    private WindowManager windowManager;

    private const int msDelayForChecking = 100;

    public static int keyToggleWindow = 0;
    public static int gamepadXKeyToggleWindow = 0;

    private void Start()
    {
        windowManager = GetComponent<WindowManager>();
        keyThread = new Thread(DetectKeys) { IsBackground = true };
        keyThread.Start();
        targetGameCheckerThread = new Thread(CheckTargetGameIsRunning) { IsBackground = true };
        targetGameCheckerThread.Start();
    }

    private void DetectKeys()
    {
        int lastKeyPressTime = 0; // Tracks the last time the key was pressed

        while (running)
        {
            bool isKeyToggleWindowPressed;

            switch (keyToggleWindow)
            {
                case 0:
                    isKeyToggleWindowPressed = (GetAsyncKeyState(0x74) & 0x8000) != 0; // F5 key
                    break;
                case 1:
                    isKeyToggleWindowPressed = (GetAsyncKeyState(0x14) & 0x8000) != 0; // Capslock key
                    break;
                case 2:
                    isKeyToggleWindowPressed = (GetAsyncKeyState(0xA4) & 0x8000) != 0; // Left Alt key
                    break;
                default:
                    isKeyToggleWindowPressed = (GetAsyncKeyState(0x74) & 0x8000) != 0; // F5 key
                    break;
            }

            switch (gamepadXKeyToggleWindow)
            {
                case 0:
                    break;
                case 1:
                    // Check gamepad (XInput) buttons LeftThumb + B
                    if (!isKeyToggleWindowPressed)
                    {
                        XINPUT_STATE state;
                        if (XInputGetState(0, out state) == 0) // Check player 1 (index 0)
                        {
                            bool isAButtonPressed = (state.Gamepad.wButtons & XINPUT_GAMEPAD_LEFT_THUMB) != 0;
                            bool isLeftDpadPressed = (state.Gamepad.wButtons & XINPUT_GAMEPAD_B) != 0;

                            if (isLeftDpadPressed && isAButtonPressed)
                            {
                                isKeyToggleWindowPressed = true;
                            }
                        }
                    }
                    break;
                default:
                    break;
            }

            if (isKeyToggleWindowPressed && Environment.TickCount - lastKeyPressTime > 200) // 200ms cooldown
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