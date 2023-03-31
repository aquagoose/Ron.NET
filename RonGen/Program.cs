using System;
using RonGen;

TestClass test = Deserialize_TestClass(@"
Hello: 3,
Test: ""this is a test!"",
Thing: true
");

Console.WriteLine(test);

static TestClass Deserialize_TestClass(string ron)
{
    Ron.NET.Element element = Ron.NET.Ron.Parse(ron);
    var obj = new TestClass();
    obj.Hello = (System.Int32) element["Hello"].AsDouble;
    obj.Test = element["Test"].AsString;
    obj.Thing = element["Thing"].AsBool;

    return obj;
}

class TestClass
{
    public int Hello;
    public string Test;
    public bool Thing;
}