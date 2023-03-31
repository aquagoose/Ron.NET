using System;
using System.Collections.Generic;
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
        if (type.IsValueType)
            goto SKIP_ERROR;
        
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

        code.AppendLine(AppendDeserializerCode(type, ""));

        return code.ToString();
    }

    private static string AppendDeserializerCode(Type type, string types)
    {
        StringBuilder code = new StringBuilder();
        
        foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            StringBuilder element = new StringBuilder("element");

            code.Append("obj.");
            foreach (string t in types.Split('.', StringSplitOptions.RemoveEmptyEntries))
            {
                code.Append(t + ".");
                element.Append($"[\"{t}\"]");
            }

            element.Append($"[\"{field.Name}\"]");

            code.Append(field.Name + " = ");


            if (field.FieldType == typeof(sbyte) || field.FieldType == typeof(byte) ||
                field.FieldType == typeof(short) || field.FieldType == typeof(ushort) ||
                field.FieldType == typeof(int) || field.FieldType == typeof(uint) ||
                field.FieldType == typeof(long) || field.FieldType == typeof(ulong) ||
                field.FieldType == typeof(float) || field.FieldType == typeof(double))
            {
                code.AppendLine($"({field.FieldType.FullName}) {element}.AsDouble;");
            }
            else if (field.FieldType == typeof(string))
                code.AppendLine($"{element}.AsString;");
            else if (field.FieldType == typeof(char))
                code.AppendLine($"{element}.AsChar;");
            else if (field.FieldType == typeof(bool))
                code.AppendLine($"{element}.AsBool;");
            else if (field.FieldType.BaseType == typeof(Enum))
                code.AppendLine($"System.Enum.Parse<{field.FieldType.FullName}>({element}.AsString);");
            else
            {
                code.AppendLine($"new {field.FieldType.GetTypeNameWithoutGeneric() + (field.FieldType.IsGenericType ? $"<{field.FieldType.GetGenericArguments()[0].FullName}>" : "" )}();");
                code.Append(AppendDeserializerCode(field.FieldType, types + $".{field.Name}"));
            }
        }

        return code.ToString();
    }

    //public static string GenerateSerializerForType(Type type)
    //{
        
    //}

    private static string GetTypeNameWithoutGeneric(this Type type)
    {
        string name = type.FullName;
        int index = name.IndexOf('`');
        if (index != -1)
            return name[..index];

        return name;
    }
}