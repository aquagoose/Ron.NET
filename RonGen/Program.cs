using System;
using System.Numerics;
using RonGen;

TestClass test = Deserialize_TestClass(@"
Hello: 3,
Test: ""this is a test!"",
Thing: true,

Struct: (X: 3.5, Y: 1234, Z: 3.2)
");

Console.WriteLine(test);

//Console.WriteLine(Generator.GenerateDeserializerMethodForType(typeof(TestClass)));

static TestClass Deserialize_TestClass(string ron)
{
    Ron.NET.Element element = Ron.NET.Ron.Parse(ron);
    var obj = new TestClass();
    obj.Hello = (System.Int32) element["Hello"].AsDouble;
    obj.Test = element["Test"].AsString;
    obj.Thing = element["Thing"].AsBool;
    obj.Struct = new System.Numerics.Vector3();
    obj.Struct.X = (System.Single) element["Struct"]["X"].AsDouble;
    obj.Struct.Y = (System.Single) element["Struct"]["Y"].AsDouble;
    obj.Struct.Z = (System.Single) element["Struct"]["Z"].AsDouble;



    return obj;
}


class TestClass
{
    public int Hello;
    public string Test;
    public bool Thing;
    public Vector3 Struct;
}