using Silk.NET.Input;

namespace RastertekCS.Windows.Tutorial47.Inputs;

public class Input
{
    private readonly HashSet<Key> _keys = new();
    private int _mouseX, _mouseY;

    public void Initialize() => _keys.Clear();

    public void KeyDown(Key input) => _keys.Add(input);

    public void KeyUp(Key input) => _keys.Remove(input);

    public bool IsKeyDown(Key key) => _keys.Contains(key);

    public void SetMouseLocation(int x, int y) { _mouseX = x; _mouseY = y; }

    public void GetMouseLocation(out int x, out int y) { x = _mouseX; y = _mouseY; }
}
