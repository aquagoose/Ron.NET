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

MyVector: (X: 5.1, Y: 67, Z: -3.4766),

MyArr: [
    (X: 1, Y: 0, Z: 0),
    (X: 0, Y: 1, Z: 0),
    (X: 0, Y: 0, Z: 1),
],

Structs: [
    (
        Blah: 2.5,
        Value: [
            (X: 2.4, Y: 6.87, Z: 3),
            (X: 12, Y: 0, Z: 1.2),
        ]
    ),

    (
        Blah: -4,
        Value: [
            (X: 2)
        ]
    )
]
";

IElement element = RON.Parse(ron);
TestStruct obj = Deserialize(element);

Console.WriteLine(obj);

TestStruct Deserialize(IElement element)
{
    TestStruct obj = new TestStruct();

    return obj;
}