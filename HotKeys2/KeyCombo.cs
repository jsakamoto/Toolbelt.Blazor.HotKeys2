namespace Toolbelt.Blazor.HotKeys2;

public readonly struct KeyCombo
{
    public ModKey Modifiers { get; }

    public Key Key { get; }

    public KeyCombo(ModKey modifiers, Key key)
    {
        this.Modifiers = modifiers;
        this.Key = key;
    }

    public static implicit operator KeyCombo(Key key) => new(ModKey.None, key);

    public override string ToString()
    {
        return this.Modifiers == ModKey.None ?
            this.Key.ToString() :
            this.Modifiers.ToString() + "+" + this.Key.ToString();
    }
}
