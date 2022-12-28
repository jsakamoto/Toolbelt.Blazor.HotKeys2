namespace Toolbelt.Blazor.HotKeys2;

public readonly partial struct Code
{
    private readonly string _Velus;

    public Code(string value) => this._Velus = value;

    public override string ToString() => this._Velus;

    public static implicit operator string(Code code) => code._Velus;

    public static CodeCombo operator +(ModCode mod, Code code) => new(mod, code);
}
