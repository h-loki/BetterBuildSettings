using System;
using Newtonsoft.Json;
using Sirenix.OdinInspector;


[Serializable]
[HideReferenceObjectPicker]
[JsonConverter(typeof(DefineEntryJsonConverter))]
public sealed class DefineEntry : IEquatable<DefineEntry>
{
    [ReadOnly] [HideLabel] public string value;

    public DefineEntry(string value)
    {
        this.value = value;
    }

    public bool Equals(DefineEntry other)
    {
        if (other == null)
            return false;

        return string.Equals(value, other.value, StringComparison.Ordinal);
    }

    public override bool Equals(object obj)
    {
        return obj is DefineEntry other && Equals(other);
    }

    public override int GetHashCode()
    {
        return StringComparer.Ordinal.GetHashCode(value ?? string.Empty);
    }

    public override string ToString()
    {
        return value;
    }
}