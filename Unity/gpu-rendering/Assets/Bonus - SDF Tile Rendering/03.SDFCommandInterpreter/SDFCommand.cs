using UnityEngine;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct SDFCommand
{
    public int shapeType;
    public int operation;

    public Vector4 paramsA;
    public Vector4 paramsB;
}