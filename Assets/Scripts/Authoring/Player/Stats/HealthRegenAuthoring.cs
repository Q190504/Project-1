using Unity.Entities;
using UnityEngine;

public class HealthRegenAuthoring : MonoBehaviour
{
    public float healthRegenValue;

    public int currentLevel;
    public int maxLevel;
    public float increment;

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
    public float healthRegenValue;

    public int currentLevel;
    public int maxLevel;
    public float increment;
}
