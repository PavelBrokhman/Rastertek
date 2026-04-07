////////////////////////////////////////////////////////////////////////////////
// Filename: InputClass.cs
////////////////////////////////////////////////////////////////////////////////
using Silk.NET.Input;

namespace RastertekCS.OpenGL.Tutorial03;

public class InputClass
{
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
