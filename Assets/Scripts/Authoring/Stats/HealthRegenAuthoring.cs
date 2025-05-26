using Unity.Entities;
using UnityEngine;

public class HealthRegenAuthoring : MonoBehaviour
{
    public int healthRegenValue;

    public int currentLevel;
    public int maxLevel;
    public int increment;

    public class Baker : Baker<HealthRegenAuthoring>
    {
        public override void Bake(HealthRegenAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new HealthRegenComponent
            {
                healthRegenValue = authoring.healthRegenValue,

                currentLevel = authoring.currentLevel,
                maxLevel = authoring.maxLevel,
                increment = authoring.increment,
            });
        }
    }
}

public struct HealthRegenComponent : IComponentData
{
    public int healthRegenValue;

    public int currentLevel;
    public int maxLevel;
    public int increment;
}
