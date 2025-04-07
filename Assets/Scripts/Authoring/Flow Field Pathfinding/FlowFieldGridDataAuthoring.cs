using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class FlowFieldGridDataAuthoring : MonoBehaviour
{
    public int width;
    public int height;
    public float nodeSize;
    public float3 originPosition;
    public bool showDebug;

    class Baker : Baker<FlowFieldGridDataAuthoring>
    {
        public override void Bake(FlowFieldGridDataAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new FlowFieldGridDataComponent
            {
                width = authoring.width,
                height = authoring.height,
                nodeSize = authoring.nodeSize,
                originPosition = authoring.originPosition,
                showDebug = authoring.showDebug,
            });
        }
    }
}

public struct FlowFieldGridDataComponent : IComponentData
{
    public int width;
    public int height;
    public float nodeSize;
    public float3 originPosition;
    public bool showDebug;
}
