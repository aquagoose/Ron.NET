using System;
using System.Collections.Generic;

namespace Ron.NET;

public class ElementArray : IElement
{
    public List<IElement> Elements;

    public ElementArray(params IElement[] elements)
    {
        Elements = new List<IElement>(elements);
    }

    public IElement this[int index]
    {
        get
        {
            if (index < 0 || index >= Elements.Count)
                throw new IndexOutOfRangeException($"Indexer must be between 0 and {Elements.Count - 1}. Actual value was {index}.");

            return Elements[index];
        }
    }

    public IElement this[string elementName] => throw new System.NotImplementedException();

    public string Serialize(SerializeOptions options = SerializeOptions.None)
    {
        throw new System.NotImplementedException();
    }
}