using WindowsInput;

public static class KeyPressSimulator
{
    public static void SimulateKey(params VirtualKeyCode[] virtualKey)
    {
        InputSimulator sim = new InputSimulator();
        sim.Keyboard.KeyDown(VirtualKeyCode.CLEAR).KeyDown(VirtualKeyCode.TAB).KeyDown(VirtualKeyCode.VK_1).Sleep(50).KeyUp(VirtualKeyCode.VK_1).KeyUp(VirtualKeyCode.TAB).KeyUp(VirtualKeyCode.CLEAR);
    }
}
