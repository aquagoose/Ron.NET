using System;
using Ron.NET;
using Ron.Tests;

string ron = @"
Stuff(
    Number: 3,
    String: ""This is a test!"",
    Boolean: true,

    Struct: (X: 0, Y: 1.23452, Z: ""hello"", W: false),
)
";

Element element = Ron.NET.Ron.Parse(ron);

Console.WriteLine(element["Stuff"]["String"]);
Console.WriteLine(element["Stuff"]["Struct"]["W"]);
Console.WriteLine();
Console.WriteLine(element);