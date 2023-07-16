using System;

namespace Ron.NET;

[Flags]
public enum SerializeType
{
    Serialize = 0,
    Deserialize = 1 << 0
}