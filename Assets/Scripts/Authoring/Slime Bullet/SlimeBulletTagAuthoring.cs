using Unity.Entities;
using UnityEngine;

public class SlimeBulletTagAuthoring : MonoBehaviour
{
    public class SlimeBulletBaker : Baker<SlimeBulletTagAuthoring>
    {
        public override void Bake(SlimeBulletTagAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new SlimeTagComponent());
        }
    }
}

public struct SlimeTagComponent : IComponentData
{

}
