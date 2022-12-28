namespace Toolbelt.Blazor.HotKeys2;

public readonly struct CodeCombo
{
    public ModCode Modifiers { get; }

    public Code Code { get; }

    public CodeCombo(ModCode modifiers, Code code)
    {
        this.Modifiers = modifiers;
        this.Code = code;
    }

    public static implicit operator CodeCombo(Code code) => new(ModCode.None, code);

    public override string ToString()
    {
        return this.Modifiers == ModCode.None ?
            this.Code.ToString() :
            this.Modifiers.ToString() + "+" + this.Code.ToString();
    }
}
