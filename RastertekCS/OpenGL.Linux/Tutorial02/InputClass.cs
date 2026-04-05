////////////////////////////////////////////////////////////////////////////////
// Filename: InputClass.cs
////////////////////////////////////////////////////////////////////////////////
using Silk.NET.Input;

namespace RastertekCS.OpenGL.Tutorial02;

public class InputClass
{
    // В отличие от C++ (массив m_keys[256]), мы используем HashSet,
    // так как значения Silk.NET Key — это не плотные индексы.
    private readonly HashSet<Key> m_keys = new();

    public void Initialize()
    {
        m_keys.Clear();
    }

    public void KeyDown(Key input)
    {
        m_keys.Add(input);
    }

    public void KeyUp(Key input)
    {
        m_keys.Remove(input);
    }

    public bool IsKeyDown(Key key)
    {
        return m_keys.Contains(key);
    }
}
