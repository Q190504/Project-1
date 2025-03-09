using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class PathPositionAuthoring : MonoBehaviour
{
    public class PathPositionBaker : Baker<PathPositionAuthoring>
    {
        public override void Bake(PathPositionAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddBuffer<PathPositionComponent>(entity);
        }
    }
}

[InternalBufferCapacity(64)]
public struct PathPositionComponent : IBufferElementData
{
    public int2 position;
}