using Silk.NET.Input;

namespace RastertekCS.OpenGL.Tutorial47.Inputs;

public class Input
{
    private readonly HashSet<Key> _keys = new();
    private int _mouseX, _mouseY;
    public void Initialize() => _keys.Clear();
    public void KeyDown(Key k) => _keys.Add(k);
    public void KeyUp(Key k) => _keys.Remove(k);
    public bool IsKeyDown(Key k) => _keys.Contains(k);
    public void SetMouseLocation(int x, int y) { _mouseX = x; _mouseY = y; }
    public void GetMouseLocation(out int x, out int y) { x = _mouseX; y = _mouseY; }
}
