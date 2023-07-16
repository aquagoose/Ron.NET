using System;
using Ron.NET;
using Ron.NET.Elements;
using Ron.NET.Generator;
using Ron.Tests;

Console.WriteLine(RonGenerator.GenerateDeserializerForType(typeof(TestStruct), "element", "obj"));

const string ron = @"
test: 4,
Hello: 5,
Test: ""This is a test."",
Thing: true,

MyVector: (X: 5.1, Y: 67, Z: -3.4766)
";

IElement element = RON.Parse(ron);
TestStruct obj = Deserialize(element);

Console.WriteLine(obj);

TestStruct Deserialize(IElement element)
{
    TestStruct obj = new TestStruct();
    
    return obj;
}