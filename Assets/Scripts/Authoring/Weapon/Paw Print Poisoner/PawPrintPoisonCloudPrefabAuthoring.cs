using Unity.Entities;
using UnityEngine;

public class PawPrintPoisonCloudPrefabAuthoring : MonoBehaviour
{
    public GameObject pawPrintPoisonCloudPrefab;

    public class SlimeBulletPrefabBaker : Baker<PawPrintPoisonCloudPrefabAuthoring>
    {
        public override void Bake(PawPrintPoisonCloudPrefabAuthoring authoring)
        {
            Entity slimeBulletEntity = GetEntity(authoring.pawPrintPoisonCloudPrefab, TransformUsageFlags.Dynamic);
            Entity mainEntity = GetEntity(authoring, TransformUsageFlags.Dynamic);

            AddComponent(mainEntity, new PawPrintPoisonCloudPrefabComponent
            {
                pawPrintPoisonCloudPrefab = slimeBulletEntity,
            });
        }
    }
}

public struct PawPrintPoisonCloudPrefabComponent : IComponentData
{
    public Entity pawPrintPoisonCloudPrefab;
}
