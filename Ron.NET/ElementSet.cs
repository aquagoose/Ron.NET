using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ron.NET;

public class ElementSet : IElement
{
    private Dictionary<string, IElement> _elements;

    public ElementSet(params KeyValuePair<string, IElement>[] elements)
    {
        _elements = new Dictionary<string, IElement>(elements);
    }

    public IElement this[int index]
    {
        get
        {
            if (index < 0 || index >= _elements.Count)
                throw new IndexOutOfRangeException($"Indexer must be between 0 and {_elements.Count - 1}. Actual value was {index}.");

            return _elements.ElementAt(index).Value;
        }
        set
        {
            if (index < 0 || index >= _elements.Count)
            {
                throw new IndexOutOfRangeException(
                    $"Indexer must be between 0 and {_elements.Count - 1}. Actual value was {index}. You cannot add new values to an ElementSet using an integer based indexer. Please use the string indexer overload instead.");
            }

            // EW.
            _elements[_elements.ElementAt(index).Key] = value;
        }
    }

    public IElement this[string elementName]
    {
        get
        {
            if (!_elements.TryGetValue(elementName, out IElement value))
                throw new KeyNotFoundException($"The given key \"{elementName}\" was not found in the set.");

            return value;
        }
        set
        {
            if (!_elements.ContainsKey(elementName))
            {
                _elements.Add(elementName, value);
                return;
            }

            _elements[elementName] = value;
        }
    }

    public T As<T>()
    {
        throw new InvalidOperationException("Cannot get value from an ElementSet.");
    }

    public string Serialize(SerializeOptions options = SerializeOptions.None)
    {
        StringBuilder builder = new StringBuilder();
        int i = 0;
        foreach ((string name, IElement element) in _elements)
        {
            builder.Append($"{name}: ");
            if (element is ElementSet)
                builder.Append($"({element.Serialize(options)})");
            else
                builder.Append(element.Serialize(options));
            if (++i < _elements.Count)
            {
                builder.Append(", ");
                if (options == SerializeOptions.PrettyPrint)
                    builder.AppendLine();
            }
        }

        return builder.ToString();
    }
}