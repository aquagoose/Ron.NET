using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Text;
using Ron.NET;

namespace RonGen;

public static class Generator
{
    public const string Namespace = "Ron.NET";
    
    public static string GenerateDeserializerMethodForType(Type type, string methodName = null)
    {
        Console.WriteLine("Generating deserializer method...");
        methodName ??= $"Deserialize_{type.FullName.Replace('.', '_')}";

        StringBuilder builder = new StringBuilder();
        builder.AppendLine($"public static object {methodName}(string ron)" + "\n{");
        builder.AppendLine("    " + 
            GenerateDeserializerForType(type)
                .Replace("\n", "\n    ")
                .Replace("<<RON_SERIALIZER_TEXT>>", "ron")
            );
        builder.AppendLine("    return (object) obj;\n}");
        return builder.ToString();
    }
    
    public static string GenerateDeserializerForType(Type type)
    {
        Console.Write("    Searching for a parameterless constructor... ");
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
        Console.WriteLine("Found!");
        StringBuilder code = new StringBuilder($"{Namespace}.Element element = {Namespace}.RON.Parse(<<RON_SERIALIZER_TEXT>>);\n");
        
        code.AppendLine($"var obj = new {type.FullName}();");

        Console.WriteLine("    Generating deserializer code...");
        code.AppendLine(AppendDeserializerCode(type, ""));
        Console.WriteLine("    Done!");

        return code.ToString();
    }

    private static string AppendDeserializerCode(Type type, string types)
    {
        StringBuilder code = new StringBuilder();
        
        foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            string[] splitTypes = types.Split('.', StringSplitOptions.RemoveEmptyEntries);

            Console.WriteLine(Pad($"        Generating deserializer for field {field.Name}...", splitTypes.Length * 2));
            
            if (field.GetCustomAttribute(typeof(RonIgnoreAttribute)) != null)
            {
                Console.WriteLine(Pad($"        Ignored.", splitTypes.Length * 2));
                continue;
            }
            
            StringBuilder element = new StringBuilder("element");

            code.Append("obj.");
            foreach (string t in splitTypes)
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
            
            Console.WriteLine(Pad("        Done!", splitTypes.Length * 2));
        }

        return code.ToString();
    }

    public static string GenerateSerializerMethodForType(Type type, string methodName = null)
    {
        Console.WriteLine("Generating serializer method...");
        methodName ??= "Serialize_" + type.GetTypeNameWithoutGeneric(true);

        StringBuilder code = new StringBuilder($"public static {Namespace}.Element {methodName}(object value)" + "\n{\n");

        code.AppendLine($"    {type.FullName} sValue = ({type.FullName}) value;");
        
        code.AppendLine("    " + 
            GenerateSerializerForType(type)
                .Replace("\n", "\n    ")
                .Replace("<<RON_SERIALIZER_TYPE_NAME>>", "sValue")
            );

        code.AppendLine("    return element;");
        code.Append('}');

        return code.ToString();
    }

    public static string GenerateSerializerForType(Type type)
    {
        const string defaultElement = "element";
        
        StringBuilder code = new StringBuilder($"{Namespace}.Element {defaultElement} = new {Namespace}.Element();\n");

        Console.WriteLine("    Generating serializer code...");
        code.Append(AppendSerializerCode(type, defaultElement, "<<RON_SERIALIZER_TYPE_NAME>>"));
        Console.WriteLine("    Done!");
        
        return code.ToString();
    }

    private static string AppendSerializerCode(Type type, string elementName, string varName)
    {
        StringBuilder code = new StringBuilder();

        foreach (FieldInfo info in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            string[] splitVar = varName.Split('.', StringSplitOptions.RemoveEmptyEntries);
            Console.WriteLine(Pad($"        Generating serializer for field \"{info.Name}\"", splitVar.Length * 2));

            if (info.GetCustomAttribute(typeof(RonIgnoreAttribute)) != null)
            {
                Console.WriteLine(Pad($"        Ignored.", splitVar.Length * 2));
                continue;
            }
            
            if (!info.FieldType.IsPrimitive && info.FieldType.BaseType != typeof(Enum) && info.FieldType != typeof(string))
            {
                string name = info.FieldType.GetTypeNameWithoutGeneric(true) + "_Element" + ((ulong) Random.Shared.NextInt64()).ToString("X");
                code.AppendLine($"{Namespace}.Element {name} = new {Namespace}.Element();");
                code.AppendLine(AppendSerializerCode(info.FieldType, name, varName + $".{info.Name}"));
                code.AppendLine($"{elementName}.Elements.Add(\"{info.Name}\", {name});");
            }
            else
                code.AppendLine($"{elementName}.Elements.Add(\"{info.Name}\", new {Namespace}.Element({varName}.{info.Name}));");
            
            Console.WriteLine(Pad("        Done!", splitVar.Length * 2));
        }

        return code.ToString();
    }

    public static string GenerateRonGenMethodsClass(Type[] types, string className, string @namespace)
    {
        Console.WriteLine($"Generating RonGenMethods class for {types.Length} types.");
        StringBuilder code = new StringBuilder($"namespace {@namespace};\n\n");
        code.AppendLine($"public class {className} : {Namespace}.RonGenMethods" + "\n{");

        code.AppendLine($"    public {className}() : base()" + "\n    {\n        Methods = new()\n        {");
        int i = 0;
        foreach (Type type in types)
        {
            code.Append($"            [typeof({type.GetTypeNameWithoutGeneric()})] = (Serialize_{type.GetTypeNameWithoutGeneric(true)}, Deserialize_{type.GetTypeNameWithoutGeneric(true)})");
            if (i < types.Length - 1)
                code.AppendLine(",");
            else
                code.AppendLine();
        }

        code.AppendLine("        };");
        
        code.AppendLine("    }");

        code.AppendLine();

        foreach (Type type in types)
        {
            Console.WriteLine($"Generating serializer for {type.GetTypeNameWithoutGeneric()}... ");
            code.AppendLine("    " + 
                GenerateSerializerMethodForType(type)
                    .Replace("\n", "\n    ")
            );
            Console.WriteLine("Done!");
            code.AppendLine();
            Console.WriteLine($"Generating deserializer for {type.GetTypeNameWithoutGeneric()}...");
            code.AppendLine("    " + 
                GenerateDeserializerMethodForType(type)
                    .Replace("\n", "\n    ")
                );
            Console.WriteLine("Done!");
            code.AppendLine();
        }

        code.Append("}");

        return code.ToString();
    }

    private static string GetTypeNameWithoutGeneric(this Type type, bool replace = false)
    {
        string name = type.FullName;
        if (replace)
            name = name.Replace('.', '_');
        int index = name.IndexOf('`');
        if (index != -1)
            return name[..index];

        return name;
    }

    private static string Pad(string text, int padAmount)
    {
        return text.PadLeft(text.Length + padAmount);
    }
}