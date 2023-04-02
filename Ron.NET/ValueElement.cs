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

    public IElement this[int index] => throw new NotSupportedException("Value element cannot be indexed.");

    public IElement this[string elementName] => throw new NotSupportedException("Value element cannot be indexed.");

    public string Serialize(SerializeOptions options = SerializeOptions.None)
    {
        return Type switch
        {
            ElementType.String => $"\"{Value}\"",
            ElementType.Number => Value.ToString(),
            ElementType.Char => $"'{Value}'",
            ElementType.Enum => Value.ToString(),
            ElementType.Bool => (bool) (object) Value ? "true" : "false",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}