namespace Toolbelt.Blazor.HotKeys2;

/// <summary>
/// The flags of modifier keys.
/// </summary>
[Flags]
public enum ModCodes
{
    None = 0,
    Shift = 0b0001,
    Ctrl = 0b0010,
    Alt = 0b0100,
    Meta = 0b1000
}

public readonly struct ModCode : IEquatable<ModCode>
{
    public static readonly ModCode None = new(0);
    public static readonly ModCode Shift = new(0b0001);
    public static readonly ModCode Ctrl = new(0b0010);
    public static readonly ModCode Alt = new(0b0100);
    public static readonly ModCode Meta = new(0b1000);

    private readonly int _Value;

    private ModCode(int value) => this._Value = value;

    public static ModCode operator +(ModCode mod1, ModCode mod2) => new(mod1._Value + mod2._Value);

    public static bool operator ==(ModCode left, ModCode right) => left.Equals(right);

    public static bool operator !=(ModCode left, ModCode right) => !(left == right);

    public override bool Equals(object? obj) => obj is ModCode code && this.Equals(code);

    public bool Equals(ModCode other) => this._Value == other._Value;

    public override int GetHashCode() => HashCode.Combine(this._Value);

    public override string ToString()
    {
        var combo = new string[10];
        var n = 0;
        if (this._Value == 0) combo[n++] = nameof(None);
        if ((this._Value & Shift._Value) != 0) combo[n++] = nameof(Shift);
        if ((this._Value & Ctrl._Value) != 0) combo[n++] = nameof(Ctrl);
        if ((this._Value & Alt._Value) != 0) combo[n++] = nameof(Alt);
        if ((this._Value & Meta._Value) != 0) combo[n++] = nameof(Meta);
        return string.Join('+', combo, 0, n);
    }
}
