////////////////////////////////////////////////////////////////////////////////
// Filename: InputClass.cs
////////////////////////////////////////////////////////////////////////////////
using Silk.NET.Input;

namespace RastertekCS.OpenGL.Tutorial02;

public class InputClass
{
    // В отличие от C++ (массив _keys[256]), мы используем HashSet,
    // так как значения Silk.NET Key — это не плотные индексы.
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
