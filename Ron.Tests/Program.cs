using System;
using Ron.NET;
using Ron.Tests;
using RonGen;

const string ron = @"
Hello: 5,
Test: ""stuff"",
Thing: false,

Vectors: [
    (X: 3.5, Y: 2.7, Z: 1.7),
    (X: 3.6, Y: 2.6, Z: 3.6),
    (X: 3.7, Y: 2.5, Z: 5.1),
    (X: 3.8, Y: 2.4, Z: 6.8),
    (X: 3.9, Y: 2.3, Z: 8.5),
    (X: 4.0, Y: 2.2, Z: 10.2),
    (X: 4.1, Y: 2.1, Z: 11.9),
    (X: 4.2, Y: 2.0, Z: 13.6),
]";

Console.WriteLine(Generator.GenerateDeserializerMethodForType(typeof(TestClass)));

TestClass test = (TestClass) Deserialize_Ron_Tests_TestClass(ron);
Console.WriteLine(test);

static object Deserialize_Ron_Tests_TestClass(string ron)
{
    Ron.NET.IElement element = Ron.NET.RON.Parse(ron);
    var obj = new Ron.Tests.TestClass();
    obj.Hello = (System.Int32) ((Ron.NET.ValueElement<double>) element["Hello"]).Value;
    obj.Test = ((Ron.NET.ValueElement<string>) element["Test"]).Value;
    obj.Thing = ((Ron.NET.ValueElement<bool>) element["Thing"]).Value;
    var System_Numerics_Vector3_List68D39ECDDBF33BC2 = new System.Collections.Generic.List<System.Numerics.Vector3>();
    Ron.NET.ElementArray System_Numerics_Vector3_Element68D39ECDDBF33BC2 = (Ron.NET.ElementArray) element["Vectors"];
    for (int System_Numerics_Vector3_Element68D39ECDDBF33BC2_Indexer = 0; System_Numerics_Vector3_Element68D39ECDDBF33BC2_Indexer < System_Numerics_Vector3_Element68D39ECDDBF33BC2.Elements.Count; System_Numerics_Vector3_Element68D39ECDDBF33BC2_Indexer++)
    {
        System.Numerics.Vector3 item = new System.Numerics.Vector3();
        item.X = (System.Single) ((Ron.NET.ValueElement<double>) System_Numerics_Vector3_Element68D39ECDDBF33BC2[System_Numerics_Vector3_Element68D39ECDDBF33BC2_Indexer]["X"]).Value;
        item.Y = (System.Single) ((Ron.NET.ValueElement<double>) System_Numerics_Vector3_Element68D39ECDDBF33BC2[System_Numerics_Vector3_Element68D39ECDDBF33BC2_Indexer]["Y"]).Value;
        item.Z = (System.Single) ((Ron.NET.ValueElement<double>) System_Numerics_Vector3_Element68D39ECDDBF33BC2[System_Numerics_Vector3_Element68D39ECDDBF33BC2_Indexer]["Z"]).Value;
        System_Numerics_Vector3_List68D39ECDDBF33BC2.Add(item);
    }
    obj.Vectors = new System.Collections.Generic.HashSet<System.Numerics.Vector3>(System_Numerics_Vector3_List68D39ECDDBF33BC2);
    
    return (object) obj;
}