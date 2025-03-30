using Unity.Entities;
using UnityEngine;

public class SlimeSlashAuthoring : MonoBehaviour
{
    public class Baker : Baker<SlimeSlashAuthoring>
    {
        public override void Bake(SlimeSlashAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new SlimeSlashComponent
            {
                executionCount = 0,
            });
        }
    }
}

public struct SlimeSlashComponent : IComponentData
{
    public int executionCount;
}
