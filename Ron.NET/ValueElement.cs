using System;

namespace Ron.NET;

public struct ValueElement<T> : IElement
{
    public T Value;
    public ElementType Type;

    public ValueElement(T value, ElementType type)
    {
        Value = value;
        Type = type;
    }

    public IElement this[int index]
    {
        get => throw new NotSupportedException("Value element cannot be indexed.");
        set => throw new NotSupportedException("Value element cannot be indexed.");
    }

    public IElement this[string elementName]
    {
        get => throw new NotSupportedException("Value element cannot be indexed.");
        set => throw new NotSupportedException("Value element cannot be indexed.");
    }

    public T1 As<T1>()
    {
        return (T1) (object) Value;
    }

    public string Serialize(SerializeOptions options = SerializeOptions.None)
    {
        return Type switch
        {
            ElementType.String => $"\"{Value}\"",
            ElementType.Number => Value.ToString(),
            ElementType.Char => $"'{Value}'",
            ElementType.Enum => Value.ToString(),
            ElementType.Bool => As<bool>() ? "true" : "false",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}