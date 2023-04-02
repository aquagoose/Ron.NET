using System;
using System.Collections;
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
        StringBuilder code = new StringBuilder($"{Namespace}.IElement element = {Namespace}.RON.Parse(<<RON_SERIALIZER_TEXT>>);\n");
        
        code.AppendLine($"var obj = new {type.FullName}();");

        Console.WriteLine("    Generating deserializer code...");
        code.AppendLine(AppendDeserializerCode(type, ""));
        Console.WriteLine("    Done!");

        return code.ToString();
    }

    private static string AppendDeserializerCode(Type type, string types, string objectName = "obj", string elementName = "element")
    {
        StringBuilder code = new StringBuilder();
        
        string[] splitTypes = types.Split('.', StringSplitOptions.RemoveEmptyEntries);
        
        foreach (FieldInfo info in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            Console.WriteLine(Pad($"        Generating deserializer for field {info.Name}...", splitTypes.Length * 2));
            
            if (info.GetCustomAttribute(typeof(RonIgnoreAttribute)) != null)
            {
                Console.WriteLine(Pad($"        Ignored.", splitTypes.Length * 2));
                continue;
            }
            
            StringBuilder element = new StringBuilder(elementName);
            
            foreach (string t in splitTypes)
                element.Append($"[\"{t}\"]");

            element.Append($"[\"{info.Name}\"]");

            if (typeof(IEnumerable).IsAssignableFrom(info.FieldType) && info.FieldType != typeof(string))
            {
                string randomId = Random.Shared.NextInt64().ToString("X");
                
                Type fType;

                if (info.FieldType.IsArray)
                    fType = info.FieldType.GetElementType();
                else
                    fType = info.FieldType.GetGenericArguments()[0];
                
                string listName = $"{fType.GetTypeNameWithoutGeneric(true)}_List" + randomId;
                string tempElementName = $"{fType.GetTypeNameWithoutGeneric(true)}_Element" + randomId;

                code.AppendLine($"var {listName} = new System.Collections.Generic.List<{fType.FullName}>();");
                code.AppendLine($"{Namespace}.ElementArray {tempElementName} = ({Namespace}.ElementArray) {element};");

                string indexerName = tempElementName + "_Indexer";
                
                code.AppendLine($"for (int {indexerName} = 0; {indexerName} < {tempElementName}.Elements.Count; {indexerName}++)" + "\n{");
                code.AppendLine($"    {fType.FullName} item = new {fType.FullName}();");

                code.Append("    " + AppendDeserializerCode(fType, types, "item", tempElementName + $"[{indexerName}]").Replace("\n", "\n    "));

                code.AppendLine($"{listName}.Add(item);");
                
                code.AppendLine("}");
                
                code.Append($"{objectName}.");
            
                foreach (string t in splitTypes)
                    code.Append(t + ".");

                code.Append(info.Name + " = ");

                if (info.FieldType.IsArray)
                    code.Append($"{listName}.ToArray();");
                else
                    code.Append($"new {info.FieldType.GetTypeNameWithoutGeneric()}<{fType}>({listName});");
      
                continue;
            }

            code.Append($"{objectName}.");
            
            foreach (string t in splitTypes)
                code.Append(t + ".");

            code.Append(info.Name + " = ");

            if (info.FieldType == typeof(sbyte) || info.FieldType == typeof(byte) ||
                info.FieldType == typeof(short) || info.FieldType == typeof(ushort) ||
                info.FieldType == typeof(int) || info.FieldType == typeof(uint) ||
                info.FieldType == typeof(long) || info.FieldType == typeof(ulong) ||
                info.FieldType == typeof(float) || info.FieldType == typeof(double))
            {
                code.AppendLine($"({info.FieldType.FullName}) (({Namespace}.ValueElement<double>) {element}).Value;");
            }
            else if (info.FieldType == typeof(string))
                code.AppendLine($"(({Namespace}.ValueElement<string>) {element}).Value;");
            else if (info.FieldType == typeof(char))
                code.AppendLine($"(({Namespace}.ValueElement<char>) {element}).Value;");
            else if (info.FieldType == typeof(bool))
                code.AppendLine($"(({Namespace}.ValueElement<bool>) {element}).Value;");
            else if (info.FieldType.BaseType == typeof(Enum))
                code.AppendLine($"System.Enum.Parse<{info.FieldType.FullName}>((({Namespace}.ValueElement<string>) {element}).Value);");
            else
            {
                code.AppendLine($"new {info.FieldType.GetTypeNameWithoutGeneric() + (info.FieldType.IsGenericType ? $"<{info.FieldType.GetGenericArguments()[0].FullName}>" : "" )}();");
                code.Append(AppendDeserializerCode(info.FieldType, types + $".{info.Name}"));
            }
            
            Console.WriteLine(Pad("        Done!", splitTypes.Length * 2));
        }

        return code.ToString();
    }

    public static string GenerateSerializerMethodForType(Type type, string methodName = null)
    {
        Console.WriteLine("Generating serializer method...");
        methodName ??= "Serialize_" + type.GetTypeNameWithoutGeneric(true);

        StringBuilder code = new StringBuilder($"public static {Namespace}.IElement {methodName}(object value)" + "\n{\n");

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
        
        StringBuilder code = new StringBuilder($"{Namespace}.ElementSet {defaultElement} = new {Namespace}.ElementSet();\n");

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
                code.AppendLine($"{Namespace}.ElementSet {name} = new {Namespace}.ElementSet();");
                code.AppendLine(AppendSerializerCode(info.FieldType, name, varName + $".{info.Name}"));
                code.AppendLine($"{elementName}.Elements.Add(\"{info.Name}\", {name});");
            }
            else if (info.FieldType.BaseType == typeof(Enum))
                code.AppendLine($"{elementName}.Elements.Add(\"{info.Name}\", new {Namespace}.ValueElement<string>({varName}.{info.Name}.ToString(), {Namespace}.ElementType.Enum));");
            else
            {
                string elementType = "?????????";

                if (info.FieldType == typeof(sbyte) || info.FieldType == typeof(byte) ||
                    info.FieldType == typeof(short) || info.FieldType == typeof(ushort) ||
                    info.FieldType == typeof(int) || info.FieldType == typeof(uint) ||
                    info.FieldType == typeof(long) || info.FieldType == typeof(ulong) ||
                    info.FieldType == typeof(float) || info.FieldType == typeof(double))
                {
                    elementType = $"<double>({varName}.{info.Name}, {Namespace}.ElementType.Number)";
                }
                else if (info.FieldType == typeof(string))
                    elementType = $"<string>({varName}.{info.Name}, {Namespace}.ElementType.String)";
                else if (info.FieldType == typeof(char))
                    elementType = $"<char>({varName}.{info.Name}, {Namespace}.ElementType.Char)";
                else if (info.FieldType == typeof(bool))
                    elementType = $"<bool>({varName}.{info.Name}, {Namespace}.ElementType.Bool)";
                
                code.AppendLine($"{elementName}.Elements.Add(\"{info.Name}\", new {Namespace}.ValueElement{elementType});");
            }

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
        string name = type.IsArray ? type.GetElementType().FullName : type.FullName;
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