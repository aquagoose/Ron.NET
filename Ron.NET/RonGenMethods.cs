using System;
using System.Collections.Generic;

namespace Ron.NET;

public abstract class RonGenMethods
{
    protected internal Dictionary<Type, (Func<object, Element> serialize, Func<string, object> deserialize)> Methods;
}