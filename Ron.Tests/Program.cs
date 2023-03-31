using System;
using Ron.NET;
using Ron.Tests;
using RonGen;

/*string ron = @"
Stuff(
    Number: 3,
    String: ""This is a test!"",
    Boolean: true,

    Struct: (X: 0, Y: 1.23452, Z: ""hello"", W: false)
)
";

Element element = Ron.NET.Ron.Parse(ron);

Console.WriteLine(element["Stuff"]["String"]);
Console.WriteLine(element["Stuff"]["Struct"]["W"]);
Console.WriteLine();
Console.WriteLine(element);*/

Console.WriteLine(Generator.GenerateDeserializerMethodForType(typeof(TestClass)));

TestClass obj = Deserialize_Ron_Tests_TestClass(@"
Hello: 22,
Test: ""This is some text."",
Thing: true");

Console.WriteLine(obj);

static Ron.Tests.TestClass Deserialize_Ron_Tests_TestClass(string ron)
{
    Element element = Ron.NET.Ron.Parse(ron);
    var obj = new Ron.Tests.TestClass();
    obj.Hello = element["Hello"].GetValue<System.Int32>();
    obj.Test = element["Test"].GetValue<System.String>();
    obj.Thing = element["Thing"].GetValue<System.Boolean>();

    return obj;
}
