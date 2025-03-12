using System;
using System.Collections.Generic;
using System.Diagnostics;
using WindowsInput;

public static class KeyPressSimulator
{
    static readonly InputSimulator sim = new InputSimulator();
    public static void SimulateKey(params VirtualKeyCode[] virtualKeys)
    {
        int delayMs = 50;

        // Press all keys
        foreach (var key in virtualKeys)
        {
            sim.Keyboard.KeyDown(key);
        }

        //Delay make sure key registered
        sim.Keyboard.Sleep(delayMs);

        // Release all keys
        foreach (var key in virtualKeys)
        {
            sim.Keyboard.KeyUp(key);
        }

    }    
}
