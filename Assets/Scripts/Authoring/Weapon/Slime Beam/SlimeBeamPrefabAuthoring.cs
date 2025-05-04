using Unity.Entities;
using UnityEngine;

public class SlimeBeamPrefabAuthoring : MonoBehaviour
{
    public GameObject slimeBeamPrefab;

    public class SlimeBeamPrefabBaker : Baker<SlimeBeamPrefabAuthoring>
    {
        public override void Bake(SlimeBeamPrefabAuthoring authoring)
        {
            Entity slimeBeamEntity = GetEntity(authoring.slimeBeamPrefab, TransformUsageFlags.Dynamic);
            Entity mainEntity = GetEntity(authoring, TransformUsageFlags.Dynamic);

            AddComponent(mainEntity, new SlimeBeamPrefabComponent
            {
                slimeBeamPrefab = slimeBeamEntity,
            });
        }
    }
}

public struct SlimeBeamPrefabComponent : IComponentData
{
    public Entity slimeBeamPrefab;
}