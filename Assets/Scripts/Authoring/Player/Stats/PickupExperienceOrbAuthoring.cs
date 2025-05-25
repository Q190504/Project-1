using Unity.Entities;
using UnityEngine;

public class PickupExperienceOrbAuthoring : MonoBehaviour
{
    public int pickupRadius;
    public float pullForce;

    public class Baker : Baker<PickupExperienceOrbAuthoring>
    {
        public override void Bake(PickupExperienceOrbAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new PickupExperienceOrbComponent
            {
                pickupRadius = authoring.pickupRadius,
                pullForce = authoring.pullForce,
            });
        }
    }
}

public struct PickupExperienceOrbComponent : IComponentData
{
    public int pickupRadius;
    public float pullForce;
}
