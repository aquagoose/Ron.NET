using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.Loader;
using Ron.NET;
using RonGen;

/*Console.WriteLine(Generator.GenerateSerializerMethodForType(typeof(CustomType)));

CustomType custom = new CustomType()
{
    Value1 = 3,
    Test = "Bonjour",
    AnotherValue = true,
    Vector = new Vector3(2.3f, 5.6f, 100f)
};

Console.WriteLine(Serialize_CustomType(custom));

static Element Serialize_CustomType(CustomType value)
{
    Ron.NET.Element element = new Ron.NET.Element();
    element.Elements.Add("Value1", new Element(value.Value1));
    element.Elements.Add("Test", new Element(value.Test));
    element.Elements.Add("AnotherValue", new Element(value.AnotherValue));
    Ron.NET.Element System_Numerics_Vector3_ElementF4C655E56001192 = new Ron.NET.Element();
    System_Numerics_Vector3_ElementF4C655E56001192.Elements.Add("X", new Element(value.Vector.X));
    System_Numerics_Vector3_ElementF4C655E56001192.Elements.Add("Y", new Element(value.Vector.Y));
    System_Numerics_Vector3_ElementF4C655E56001192.Elements.Add("Z", new Element(value.Vector.Z));
    
    element.Elements.Add("Vector", System_Numerics_Vector3_ElementF4C655E56001192);
    
    return element;
}



struct CustomType
{
    public int Value1;
    public string Test;
    public bool AnotherValue;
    public Vector3 Vector;
}*/

string path = args[0];

AssemblyDependencyResolver resolver = new AssemblyDependencyResolver(path);

AssemblyLoadContext.Default.Resolving += (context, name) =>
{
    string pth = resolver.ResolveAssemblyToPath(name);
    if (pth != null)
        return context.LoadFromAssemblyPath(pth);
    return null;
};

Assembly assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
assembly.GetReferencedAssemblies();

foreach (Type type in assembly.GetTypes().Where(type => type.GetCustomAttribute(typeof(RonGenAttribute)) != null))
{
    Console.WriteLine(Generator.GenerateDeserializerMethodForType(type));
    Console.WriteLine(Generator.GenerateSerializerMethodForType(type));
}