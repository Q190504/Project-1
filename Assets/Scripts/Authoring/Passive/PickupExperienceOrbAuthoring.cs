using Unity.Entities;
using UnityEngine;

public class PickupExperienceOrbAuthoring : MonoBehaviour
{
    public int ID;
    public float basePickupRadius;
    public float pullForce;

    public int currentLevel;
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

                increment = authoring.increment,
            });

            AddComponent(entity, new PassiveComponent
            {
                ID = authoring.ID,
                Level = authoring.currentLevel,
                MaxLevel = 5,
                DisplayName = "Pickup Radius",
                Description = "Increases the radius that experience orbs can be drawn into.",
            });
        }
    }
}

public struct PickupExperienceOrbComponent : IComponentData
{
    public float basePickupRadius;
    public float pickupRadius;
    public float pullForce; 

    public float increment;
}
