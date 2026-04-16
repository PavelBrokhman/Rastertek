////////////////////////////////////////////////////////////////////////////////
// Filename: InputClass.cs
////////////////////////////////////////////////////////////////////////////////
using Silk.NET.Input;

namespace RastertekCS.OpenGL.Tutorial03;

public class InputClass
{
    private readonly HashSet<Key> _keys = new();

    public void Initialize()
    {
        _keys.Clear();
    }

    public void KeyDown(Key input)
    {
        _keys.Add(input);
    }

    public void KeyUp(Key input)
    {
        _keys.Remove(input);
    }

    public bool IsKeyDown(Key key)
    {
        return _keys.Contains(key);
    }
}
