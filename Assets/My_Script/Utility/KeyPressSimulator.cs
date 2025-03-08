using WindowsInput;

public static class KeyPressSimulator
{
    public static void SimulateKey(int delayMs = 50, params VirtualKeyCode[] virtualKeys)
    {
        InputSimulator sim = new InputSimulator();
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
