using Silk.NET.Input;

namespace RastertekCS.Windows.Tutorial16.Inputs;

public class Input
{
    private readonly HashSet<Key> m_keys = new();
    private int m_mouseX,
        m_mouseY;
    private bool m_mousePressed;

    public void Initialize() => m_keys.Clear();

    public void KeyDown(Key input) => m_keys.Add(input);

    public void KeyUp(Key input) => m_keys.Remove(input);

    public bool IsKeyDown(Key key) => m_keys.Contains(key);

    public void ProcessMouse(int x, int y)
    {
        m_mouseX = x;
        m_mouseY = y;
    }

    public void GetMouseLocation(out int x, out int y)
    {
        x = m_mouseX;
        y = m_mouseY;
    }

    public void MouseDown() => m_mousePressed = true;

    public void MouseUp() => m_mousePressed = false;

    public bool IsMousePressed() => m_mousePressed;
}
