using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class PathFindingAuthoring : MonoBehaviour
{
    public int2 startPosition;
    public int2 endPosition;

    public class PathFindingBaker : Baker<PathFindingAuthoring>
    {
        public override void Bake(PathFindingAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new PathFindingComponent
            {
                startPosition = authoring.startPosition,
                endPosition = authoring.endPosition,
            });
        }
    }
}

public struct PathFindingComponent : IComponentData
{
    public int2 startPosition;
    public int2 endPosition;
}
