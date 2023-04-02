using System;
using Ron.NET;
using Ron.Tests;

string ron = @"
Number: 3,
String: ""This is a test!"",
Struct: (
    X: 1.5,
    Y: 2.3,
    Z: 7
),
Array: [
    1,
    (X: 3.5, Y: 7.4, Z: 9, W: 1.0),
    ""Haha"",
    false
],
Hello: (
    B: ""A string""
),

Stuff: Up
";

IElement element = RON.Parse(ron);
Console.WriteLine(IElement.Serialize(element, SerializeOptions.None));