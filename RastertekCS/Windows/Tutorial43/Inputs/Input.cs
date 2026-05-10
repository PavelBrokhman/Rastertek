using Silk.NET.Input;

namespace RastertekCS.Windows.Tutorial43.Inputs;

public class Input
{
    private readonly HashSet<Key> _keys = new();

    public void Initialize() => _keys.Clear();

    public void KeyDown(Key input) => _keys.Add(input);

    public void KeyUp(Key input) => _keys.Remove(input);

    public bool IsKeyDown(Key key) => _keys.Contains(key);
}
