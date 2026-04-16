using Silk.NET.Input;

namespace RastertekCS.OpenGL.Tutorial16.Inputs;

public class Input
{
    private readonly HashSet<Key> _keys = new();
    private int _mouseX,
        _mouseY;
    private bool _mousePressed;

    public void Initialize() => _keys.Clear();

    public void KeyDown(Key input) => _keys.Add(input);

    public void KeyUp(Key input) => _keys.Remove(input);

    public bool IsKeyDown(Key key) => _keys.Contains(key);

    public void ProcessMouse(int x, int y)
    {
        _mouseX = x;
        _mouseY = y;
    }

    public void GetMouseLocation(out int x, out int y)
    {
        x = _mouseX;
        y = _mouseY;
    }

    public void MouseDown() => _mousePressed = true;

    public void MouseUp() => _mousePressed = false;

    public bool IsMousePressed() => _mousePressed;
}
