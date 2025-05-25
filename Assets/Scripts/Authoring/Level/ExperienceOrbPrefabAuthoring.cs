using Unity.Entities;
using UnityEngine;

public class ExperienceOrbPrefabAuthoring : MonoBehaviour
{
    public GameObject experienceOrbPrefab;

    public class Baker : Baker<ExperienceOrbPrefabAuthoring>
    {
        public override void Bake(ExperienceOrbPrefabAuthoring authoring)
        {
            Entity experienceOrbEntity = GetEntity(authoring.experienceOrbPrefab, TransformUsageFlags.Dynamic);
            Entity mainEntity = GetEntity(authoring, TransformUsageFlags.Dynamic);

            AddComponent(mainEntity, new ExperienceOrbPrefabComponent
            {
                experienceOrbPrefab = experienceOrbEntity,
            });
        }
    }
}

public struct ExperienceOrbPrefabComponent : IComponentData
{
    public Entity experienceOrbPrefab;
}
