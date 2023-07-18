using System;
using System.Collections.Generic;
using System.Numerics;

namespace Ron.Tests;

public struct TestStruct
{
    public int Hello;
    public string Test;
    public bool Thing;

    public MyEnum MyEnum;

    public Vector3 MyVector;

    public string HelloAgain;

    //public Matrix4x4 PleaseHelpIHaveRunOutOfVariableNames;

    public Vector3[] MyArr;
}

public enum MyEnum
{
    Stuff,
    Things,
    Blah
}