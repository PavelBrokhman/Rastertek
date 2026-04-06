using Silk.NET.Input;
namespace RastertekCS.OpenGL.Tutorial35.Inputs;
public class Input { private readonly HashSet<Key> m_keys = new(); public void Initialize() => m_keys.Clear(); public void KeyDown(Key k) => m_keys.Add(k); public void KeyUp(Key k) => m_keys.Remove(k); public bool IsKeyDown(Key k) => m_keys.Contains(k); }
