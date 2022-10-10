using System;
using System.Collections.Generic;
using Toolbelt.Blazor.HotKeys2;

namespace SampleSite.Components.Shared
{
    public class EnteredKey : IEquatable<EnteredKey>
    {
        public readonly Guid Id = Guid.NewGuid();
        public readonly ModCodes Modifiers;
        public readonly string Key;
        public readonly string Code;
        public int RepeatCount = 1;

        public EnteredKey(ModKeys modifiers, string key = "", string code = "")
            : this((ModCodes)(int)modifiers, key, code) { }

        public EnteredKey(ModCodes modifiers, string key = "", string code = "")
        {
            this.Modifiers = modifiers;
            this.Key = key;
            this.Code = code;
        }

        public override bool Equals(object obj) => this.Equals(obj as EnteredKey);

        public bool Equals(EnteredKey other)
        {
            return other != null &&
                   this.Modifiers == other.Modifiers &&
                   this.Key == other.Key &&
                   this.Code == other.Code;
        }

        public override int GetHashCode() => HashCode.Combine(this.Modifiers, this.Key, this.Code);

        public static bool operator ==(EnteredKey left, EnteredKey right) => EqualityComparer<EnteredKey>.Default.Equals(left, right);

        public static bool operator !=(EnteredKey left, EnteredKey right) => !(left == right);
    }
}
