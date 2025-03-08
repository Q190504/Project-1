using Unity.Entities;
using UnityEngine;

public class SlimeBulletPrefabAuthoring : MonoBehaviour
{
    public GameObject slimeBulletPrefab;

    public class SlimeBulletPrefabBaker : Baker<SlimeBulletPrefabAuthoring>
    {
        public override void Bake(SlimeBulletPrefabAuthoring authoring)
        {
            Entity slimeBulletEntity = GetEntity(authoring.slimeBulletPrefab, TransformUsageFlags.Dynamic);
            Entity mainEntity = GetEntity(authoring, TransformUsageFlags.Dynamic);

            AddComponent(mainEntity, new SlimeBulletPrefabComponent
            {
                slimeBulletPrefab = slimeBulletEntity,
            });
        }
    }
}

public struct SlimeBulletPrefabComponent : IComponentData
{
    public Entity slimeBulletPrefab;
}
