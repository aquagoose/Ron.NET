using System;
using System.Numerics;
using System.Reflection;
using System.Text;

namespace RonGen;

public static class Generator
{
    public const string Namespace = "Ron.NET";
    
    public static string GenerateDeserializerMethodForType(Type type, string methodName = null)
    {
        methodName ??= $"Deserialize_{type.FullName.Replace('.', '_')}";

        StringBuilder builder = new StringBuilder();
        builder.AppendLine($"public static {type.FullName} {methodName}(string ron)" + "\n{");
        builder.AppendLine("    " + 
            GenerateDeserializerForType(type)
                .Replace("\n", "\n    ")
                .Replace("<<RON_SERIALIZER_TEXT>>", "ron")
            );
        builder.AppendLine("    return obj;\n}");
        return builder.ToString();
    }
    
    public static string GenerateDeserializerForType(Type type)
    {
        foreach (ConstructorInfo info in type.GetConstructors())
        {
            if (info.GetParameters().Length == 0)
                goto SKIP_ERROR;
        }

        throw new MissingMethodException(
            "Type is missing parameterless constructor which is required for initialization.");
        
        SKIP_ERROR:
        StringBuilder code = new StringBuilder($"{Namespace}.Element element = {Namespace}.Ron.Parse(<<RON_SERIALIZER_TEXT>>);\n");
        
        code.AppendLine($"var obj = new {type.FullName}();");

        foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            code.Append("obj." + field.Name + " = ");

            if (field.FieldType == typeof(sbyte) || field.FieldType == typeof(byte) ||
                field.FieldType == typeof(short) || field.FieldType == typeof(ushort) ||
                field.FieldType == typeof(int) || field.FieldType == typeof(uint) ||
                field.FieldType == typeof(long) || field.FieldType == typeof(ulong) ||
                field.FieldType == typeof(float) || field.FieldType == typeof(double))
            {
                code.AppendLine($"({field.FieldType.FullName}) element[\"{field.Name}\"].AsDouble;");
            }
            else if (field.FieldType == typeof(string))
                code.AppendLine($"element[\"{field.Name}\"].AsString;");
            else if (field.FieldType == typeof(char))
                code.AppendLine($"element[\"{field.Name}\"].AsChar;");
            else if (field.FieldType == typeof(bool))
                code.AppendLine($"element[\"{field.Name}\"].AsBool;");
        }

        return code.ToString();
    }
}