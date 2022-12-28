namespace Toolbelt.Blazor.HotKeys2;

public readonly partial struct Key
{
    private readonly string _Velus;

    public Key(string value) => this._Velus = value;

    public override string ToString() => this._Velus;

    public static implicit operator string(Key key) => key._Velus;

    public static KeyCombo operator +(ModKey mod, Key key) => new(mod, key);
}
