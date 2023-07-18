using System;
using Ron.NET;
using Ron.NET.Elements;
using Ron.NET.Generator;
using Ron.Tests;

Console.WriteLine(RonGenerator.GenerateDeserializerForType(typeof(TestStruct), "element", "obj"));

const string ron = @"
Hello: 5,
Test: ""This is a test."",
Thing: true,

MyEnum: Things,

MyVector: (X: 5.1, Y: 67, Z: -3.4766),

HelloAgain: ""Hello!"",

MyArr: [
    (X: 1.0, Y: 2.0, Z: 3.0),
    (Z: 1.2, Y: 34),
    (Y: -24.3324)
]
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
    if (element.TryGet("Test", out Ron.NET.Elements.IElement obj_Test)) {
        obj.Test = ((Ron.NET.Elements.ValueElement<string>) obj_Test).Value;
    }
    if (element.TryGet("Thing", out Ron.NET.Elements.IElement obj_Thing)) {
        obj.Thing = ((Ron.NET.Elements.ValueElement<bool>) obj_Thing).Value;
    }
    if (element.TryGet("MyEnum", out Ron.NET.Elements.IElement obj_MyEnum)) {
        obj.MyEnum = System.Enum.Parse<Ron.Tests.MyEnum>(((Ron.NET.Elements.ValueElement<string>) obj_MyEnum).Value);
    }
    if (element.TryGet("MyVector", out Ron.NET.Elements.IElement obj_MyVector)) {
        obj.MyVector = new System.Numerics.Vector3();
        if (obj_MyVector.TryGet("X", out Ron.NET.Elements.IElement obj_MyVector_X)) {
            obj.MyVector.X = (System.Single) ((Ron.NET.Elements.ValueElement<double>) obj_MyVector_X).Value;
        }
        if (obj_MyVector.TryGet("Y", out Ron.NET.Elements.IElement obj_MyVector_Y)) {
            obj.MyVector.Y = (System.Single) ((Ron.NET.Elements.ValueElement<double>) obj_MyVector_Y).Value;
        }
        if (obj_MyVector.TryGet("Z", out Ron.NET.Elements.IElement obj_MyVector_Z)) {
            obj.MyVector.Z = (System.Single) ((Ron.NET.Elements.ValueElement<double>) obj_MyVector_Z).Value;
        }
    }
    if (element.TryGet("HelloAgain", out Ron.NET.Elements.IElement obj_HelloAgain)) {
        obj.HelloAgain = ((Ron.NET.Elements.ValueElement<string>) obj_HelloAgain).Value;
    }

    return obj;
}