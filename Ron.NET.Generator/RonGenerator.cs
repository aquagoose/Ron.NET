using System.Collections;
using System.Reflection;
using System.Text;
using Ron.NET.Elements;
using BindingFlags = System.Reflection.BindingFlags;

namespace Ron.NET.Generator;

public static class RonGenerator
{
    private const string Namespace = "Ron.NET";
    private const string Elements = $"{Namespace}.Elements";

    private const string IElement = $"{Elements}.IElement";
    private const string ValueElement = $"{Elements}.ValueElement";
    private const string ElementArray = $"{Elements}.ElementArray";

    /// <summary>
    /// Generate a deserializer for a given type.
    /// </summary>
    /// <param name="type">The type to generate the deserializer for.</param>
    /// <param name="elementName">The name of the <see cref="IElement"/> that data will be gotten from.</param>
    /// <param name="objName">The name of the object to set values for. This value can contain dots, for example, as a field name.</param>
    /// <returns></returns>
    public static string GenerateDeserializerForType(Type type, string elementName, string objName)
    {
        StringBuilder builder = new StringBuilder();

        foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
        {
            Type fieldType = field.FieldType;
            string fieldTypeName = fieldType.FullName;
            string fieldName = field.Name;

            string fieldAccessor = objName + "." + fieldName;
            string fieldAccessorPrintable = GetPrintableName(fieldAccessor);

            builder.AppendLine(
                $"if ({elementName}.TryGet(\"{fieldName}\", out {IElement} {fieldAccessorPrintable})) {{");

            builder.Append("    " + fieldAccessor + " = ");

            if (fieldType == typeof(string))
                builder.AppendLine($"(({ValueElement}<string>) {fieldAccessorPrintable}).Value;");
            else if (fieldType == typeof(char))
                builder.AppendLine($"(({ValueElement}<char>) {fieldAccessorPrintable}).Value;");
            else if (fieldType == typeof(bool))
                builder.AppendLine($"(({ValueElement}<bool>) {fieldAccessorPrintable}).Value;");
            else if (fieldType.IsEnum)
                builder.AppendLine($"System.Enum.Parse<{fieldTypeName}>((({ValueElement}<string>) {fieldAccessorPrintable}).Value);");
            else if (fieldType.IsPrimitive)
                builder.AppendLine($"({fieldTypeName}) (({ValueElement}<double>) {fieldAccessorPrintable}).Value;");
            else
            {
                builder.AppendLine("default;");
            }

            builder.AppendLine("}");
        }

        return builder.ToString();
    }

    private static string Indent(string text, int indentLevel = 1)
    {
        string spaces = '\n' + new string(' ', indentLevel * 4);

        return text.Replace("\n", spaces);
    }

    private static string GetPrintableName(string name)
    {
        return name.Replace('.', '_');
    }
}