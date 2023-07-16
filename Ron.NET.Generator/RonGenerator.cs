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
    
    public static string GenerateDeserializerForType(Type type, string elementName, string objName)
    {
        StringBuilder builder = new StringBuilder();

        foreach (FieldInfo info in type.GetFields())
        {
            Console.WriteLine(info.Name);
            string fName = info.Name;

            builder.AppendLine(
                $"if ({elementName}.TryGet(\"{fName}\", out {_iElementFullName} {objName}_{fName})) {{");

            builder.Append("    ");

            if (info.FieldType.IsPrimitive && info.FieldType != typeof(char) && info.FieldType != typeof(bool))
            {
                builder.AppendLine(
                    $"{objName}.{fName} = ({info.FieldType.FullName}) (({_valueElementFullName}<double>) {objName}_{fName}).Value;");
            }

            builder.AppendLine("}");
        }

        return builder.ToString();
    }
}