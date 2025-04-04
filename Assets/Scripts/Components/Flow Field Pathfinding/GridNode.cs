using Unity.Entities;
using Unity.Mathematics;

public struct GridNode : IBufferElementData
{
    public int index;
    public int x;
    public int y;
    public byte cost;
    public byte bestCost;
    public float2 vector;
}