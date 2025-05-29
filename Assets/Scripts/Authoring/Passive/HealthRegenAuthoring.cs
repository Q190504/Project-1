using Unity.Entities;
using UnityEngine;

public class HealthRegenAuthoring : MonoBehaviour
{
    public int ID;
    public int healthRegenValue;

    public int currentLevel;
    public int increment;

    public class Baker : Baker<HealthRegenAuthoring>
    {
        public override void Bake(HealthRegenAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new HealthRegenComponent
            {
                healthRegenValue = authoring.healthRegenValue,

                increment = authoring.increment,
            });

            AddComponent(entity, new PassiveComponent
            {
                PassiveType = PassiveType.HealthRegen,
                ID = authoring.ID,
                Level = authoring.currentLevel,
                MaxLevel = 5, 
                DisplayName = "Health Regen",
                Description = "Increases health every 0.5 second.",
            });
        }
    }
}

public struct HealthRegenComponent : IComponentData
{
    public int healthRegenValue;

    public int increment;
}
