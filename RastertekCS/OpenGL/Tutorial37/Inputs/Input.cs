using Silk.NET.Input;

namespace RastertekCS.OpenGL.Tutorial37.Inputs;

public class Input
{
    private readonly HashSet<Key> _keys = new();

    public void Initialize() => _keys.Clear();

    public void KeyDown(Key k) => _keys.Add(k);

    public void KeyUp(Key k) => _keys.Remove(k);

    public bool IsKeyDown(Key k) => _keys.Contains(k);
}
