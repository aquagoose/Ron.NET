using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ron.NET;

public class Element
{
    internal Dictionary<string, Element> Elements;
    internal object Value;

    internal Element()
    {
        Elements = new Dictionary<string, Element>();
        Value = null;
    }

    public double AsDouble => (double) Value;

    public string AsString => (string) Value;

    public char AsChar => (char) Value;

    public bool AsBool => (bool) Value;

    public Element this[string name] => Elements[name];

    public KeyValuePair<string, Element> this[int index] => Elements.ElementAt(index);

    public int NumSubElements => Elements?.Count ?? 0;

    internal Element(object value)
    {
        Value = value;
    }

    private string Serialize(int indentationLevel)
    {
        StringBuilder builder = new StringBuilder();
        if (Elements == null)
        {
            string value;
            switch (Value)
            {
                case string sVal:
                    value = '"' + sVal + '"';
                    break;
                
                case bool bVal:
                    value = bVal ? "true" : "false";
                    break;
                
                default:
                    value = Value.ToString();
                    break;
            }
            builder.Append(value);
        }
        else
        {
            int i = 0;
            foreach ((string name, Element element) in Elements)
            {
                for (int indent = 0; indent < indentationLevel; indent++)
                    builder.Append("    ");
                
                if (element.Elements == null)
                {
                    builder.Append(name + ": ");
                    builder.Append(element);
                }
                else
                {
                    if (indentationLevel > 0)
                        builder.AppendLine(name + ": (");
                    else
                        builder.AppendLine(name + "(");
                    builder.AppendLine(element.Serialize(indentationLevel + 1));
                    for (int indent = 0; indent < indentationLevel; indent++)
                        builder.Append("    ");
                    builder.Append(')');
                }
                
                if (i < Elements.Count - 1)
                    builder.AppendLine(",");

                i++;
            }
        }

        return builder.ToString();
    }

    public override string ToString()
    {
        return Serialize(0);
    }
}