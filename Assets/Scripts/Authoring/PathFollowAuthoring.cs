using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class PathFollowAuthoring : MonoBehaviour
{
    public int pathIndex;
}

public class PathFollowBaker : Baker<PathFollowAuthoring>
{
    public override void Bake(PathFollowAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);

        AddComponent(entity, new PathFollowComponent
        {
            pathIndex = authoring.pathIndex,
        });
    }
}

public struct PathFollowComponent : IComponentData
{
    public int pathIndex;
}
