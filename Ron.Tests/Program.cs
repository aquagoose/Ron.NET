using System;
using Ron.NET;
using Ron.NET.Elements;
using Ron.NET.Generator;
using Ron.Tests;

Console.WriteLine(RonGenerator.GenerateDeserializerForType(typeof(TestStruct), "element", "obj"));

const string ron = @"
test: 4,
//Hello: 5
";

IElement element = RON.Parse(ron);
TestStruct obj = Deserialize(element);

Console.WriteLine(obj);

TestStruct Deserialize(IElement element)
{
    TestStruct obj = new TestStruct();

    if (element.TryGet("Hello", out Ron.NET.Elements.IElement obj_Hello)) {
        obj.Hello = (System.Int32) ((Ron.NET.Elements.ValueElement<double>) obj_Hello).Value;
    }

    return obj;
}