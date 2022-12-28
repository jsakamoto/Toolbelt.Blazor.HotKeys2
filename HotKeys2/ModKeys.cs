namespace Toolbelt.Blazor.HotKeys2;

/// <summary>
/// The flags of modifier keys.
/// </summary>
[Flags]
public enum ModKeys
{
    None = 0,
    //Shift = 0b0001,
    Ctrl = 0b0010,
    Alt = 0b0100,
    Meta = 0b1000
}

public readonly struct ModKey : IEquatable<ModKey>
{
    public static readonly ModKey None = new(0);
    public static readonly ModKey Ctrl = new(0b0010);
    public static readonly ModKey Alt = new(0b0100);
    public static readonly ModKey Meta = new(0b1000);

    private readonly int _Value;

    private ModKey(int value) => this._Value = value;

    public static ModKey operator +(ModKey mod1, ModKey mod2) => new(mod1._Value + mod2._Value);

    public static bool operator ==(ModKey left, ModKey right) => left.Equals(right);

    public static bool operator !=(ModKey left, ModKey right) => !(left == right);

    public override bool Equals(object? obj) => obj is ModKey code && this.Equals(code);

    public bool Equals(ModKey other) => this._Value == other._Value;

    public override int GetHashCode() => HashCode.Combine(this._Value);

    public override string ToString()
    {
        var combo = new string[10];
        var n = 0;
        if (this._Value == 0) combo[n++] = nameof(None);
        if ((this._Value & Ctrl._Value) != 0) combo[n++] = nameof(Ctrl);
        if ((this._Value & Alt._Value) != 0) combo[n++] = nameof(Alt);
        if ((this._Value & Meta._Value) != 0) combo[n++] = nameof(Meta);
        return string.Join('+', combo, 0, n);
    }
}
