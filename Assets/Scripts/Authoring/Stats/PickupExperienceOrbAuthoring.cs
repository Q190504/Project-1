using Unity.Entities;
using UnityEngine;

public class PickupExperienceOrbAuthoring : MonoBehaviour
{
    public float basePickupRadius;
    public float pullForce;

    public int currentLevel;
    public int maxLevel;
    public float increment;

    public class Baker : Baker<PickupExperienceOrbAuthoring>
    {
        public override void Bake(PickupExperienceOrbAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new PickupExperienceOrbComponent
            {
                basePickupRadius = authoring.basePickupRadius,
                pickupRadius = authoring.basePickupRadius,
                pullForce = authoring.pullForce, 

                currentLevel = authoring.currentLevel,
                maxLevel = authoring.maxLevel,
                increment = authoring.increment,
            });
        }
    }
}

public struct PickupExperienceOrbComponent : IComponentData
{
    public float basePickupRadius;
    public float pickupRadius;
    public float pullForce; 

    public int currentLevel;
    public int maxLevel;
    public float increment;
}
