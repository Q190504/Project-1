using Unity.Entities;
using UnityEngine;

public class StunAuthoring : MonoBehaviour
{
    public class Baker : Baker<StunAuthoring>
    {
        public override void Bake(StunAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new StunComponent
            {
                isStunned = false,
            });
        }
    }
}

public struct StunComponent : IComponentData
{
    public bool isStunned;
}
