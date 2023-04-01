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
//Array: [
//    1,
//    4.3,
//    ""Haha"",
//    false
//],
";

IElement element = RON.Parse(ron);
Console.WriteLine(element["Struct"]["X"]);
Console.WriteLine(element.Serialize(SerializeOptions.PrettyPrint));