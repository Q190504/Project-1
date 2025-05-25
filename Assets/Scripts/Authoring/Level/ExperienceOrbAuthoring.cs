using Unity.Entities;
using UnityEngine;

public class ExperienceOrbAuthoring : MonoBehaviour
{
    public class Baker : Baker<ExperienceOrbAuthoring>
    {
        public override void Bake(ExperienceOrbAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new ExperienceOrbComponent
            {
                hasBeenCollected = false,
                experience = 10,
            });
        }
    }
}

public struct ExperienceOrbComponent : IComponentData
{
    public bool hasBeenCollected;
    public int experience;
}
