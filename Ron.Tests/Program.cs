using System;
using Ron.NET;
using Ron.Tests;
using RonGen;

const string ron = @"
ID: ""Test"",
Name: ""Stuff"",
Type: Cube,
Index: 1,
Rotation: (X: 0, Y: 0, Z: 0, W: 1.0, B: (Bing: ""Chilling"", Stuff: Things, Nada: (Haha: ""What""))),
Cubes: [
    (
        Position: (X: 0, Y: 0, Z: 0),
        Bits: [
            (X: 1, Y: 0, Z: 0),
            (X: 0, Y: 1, Z: 0),
            (X: 1, Y: 0, Z: 1),
            (X: 1, Y: 2, Z: 0)
        ]
    )
]
";

IElement parsed = RON.Parse(ron);
Console.WriteLine(IElement.Serialize(parsed, SerializeOptions.PrettyPrint));