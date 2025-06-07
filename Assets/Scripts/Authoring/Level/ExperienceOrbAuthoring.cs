using Unity.Entities;
using UnityEngine;

public class ExperienceOrbAuthoring : MonoBehaviour
{
    public int experience;

    public class Baker : Baker<ExperienceOrbAuthoring>
    {
        public override void Bake(ExperienceOrbAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new ExperienceOrbComponent
            {
                hasBeenCollected = false,
                isBeingPulled = false,
                experience = authoring.experience,
            });
        }
    }
}

public struct ExperienceOrbComponent : IComponentData
{
    public bool hasBeenCollected;
    public bool isBeingPulled;
    public int experience;
}
