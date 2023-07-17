using System;

namespace Ron.NET;

public class RonGenAttribute : Attribute
{
    public SerializeType SerializeType;

    public RonGenAttribute(SerializeType serializeType = SerializeType.Serialize | SerializeType.Deserialize)
    {
        SerializeType = serializeType;
    }
}