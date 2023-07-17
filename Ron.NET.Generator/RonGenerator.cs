using System.Collections;
using System.Reflection;
using System.Text;
using Ron.NET.Elements;
using BindingFlags = System.Reflection.BindingFlags;

namespace Ron.NET.Generator;

public static class RonGenerator
{
    private static string _iElementFullName;
    private static string _valueElementFullName;
    
    static RonGenerator()
    {
        _iElementFullName = "Ron.NET.Elements.IElement";
        _valueElementFullName = "Ron.NET.Elements.ValueElement";
    }
    
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

        // Loop through fields. Properties aren't supported right now because effort.
        foreach (FieldInfo info in type.GetFields())
        {
            // Get the field name + type.
            string fName = info.Name;
            Type fType = info.FieldType;

            // Get the generated field name that the value will be assigned to.
            string fieldName = objName + "." + fName;
            // In order to make sure the names are unique, the printable name will be the same as the field name.
            string varName = GetPrintableName(fieldName);

            // Check to make sure the value is available in RON.
            builder.AppendLine(
                $"if ({elementName}.TryGet(\"{fName}\", out {_iElementFullName} {varName})) {{");
            
            if (typeof(IEnumerable).IsAssignableFrom(fType) && fType != typeof(string))
            {
                string fTypeName = fType.GetElementType().FullName;
                string fVarName = GetPrintableName(fTypeName);

                string indexerName = fVarName + "_Indexer";
                builder.AppendLine($"    {fVarName}_")
                builder.AppendLine($"    for (int {indexerName} = 0; )")
                builder.AppendLine($"    {fTypeName} {fVarName} = new {fTypeName}();");
                builder.AppendLine("    " + Indent(GenerateDeserializerForType(fType.GetElementType(), fieldName, fVarName)));

                builder.AppendLine("\n}");
                
                continue;
            }

            builder.Append($"    {fieldName} = ");

            // Handle various types of primitives.
            if (fType.IsPrimitive && fType != typeof(char) && fType != typeof(bool))
            {
                builder.Append(
                    $"({fType.FullName}) (({_valueElementFullName}<double>) {varName}).Value;");
            }
            else if (fType == typeof(bool))
                builder.Append($"(({_valueElementFullName}<bool>) {varName}).Value;");
            else if (fType == typeof(string))
                builder.Append($"(({_valueElementFullName}<string>) {varName}).Value;");
            else // Handle complex types, such as other structs & classes.
            {
                if (fType == type)
                {
                    throw new InvalidOperationException(
                        "Field type cannot be the same as the current type, as this causes issues during generation.");
                }

                // Assign the new type to the current field.
                builder.AppendLine($"new {fType.FullName}();");

                // Then, append a deserializer for said type.
                // We pass the printable variable name, as well as the field name. This makes everything else automatic
                // beyond belief!
                builder.Append("    " + Indent(GenerateDeserializerForType(fType, varName, fieldName)));
            }

            builder.AppendLine("\n}");
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