using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ron.NET;

public class ElementSet : IElement
{
    public Dictionary<string, IElement> Elements;

    public ElementSet(params KeyValuePair<string, IElement>[] elements)
    {
        Elements = new Dictionary<string, IElement>(elements);
    }

    public IElement this[int index]
    {
        get
        {
            if (index < 0 || index >= Elements.Count)
                throw new IndexOutOfRangeException($"Indexer must be between 0 and {Elements.Count - 1}. Actual value was {index}.");

            return Elements.ElementAt(index).Value;
        }
    }

    public IElement this[string elementName]
    {
        get
        {
            if (!Elements.TryGetValue(elementName, out IElement value))
                throw new KeyNotFoundException($"The given key \"{elementName}\" was not found in the set.");

            return value;
        }
    }

    public string Serialize(SerializeOptions options = SerializeOptions.None)
    {
        StringBuilder builder = new StringBuilder();
        int i = 0;
        foreach ((string name, IElement element) in Elements)
        {
            builder.Append($"{name}: ");
            if (element is ElementSet)
                builder.Append($"({element.Serialize(options)})");
            else
                builder.Append(element.Serialize(options));
            if (++i < Elements.Count)
            {
                builder.Append(", ");
                if (options == SerializeOptions.PrettyPrint)
                    builder.AppendLine();
            }
        }

        return builder.ToString();
    }
}