using Unity.Entities;
using UnityEngine;

public class CreepHealthAuthoring : MonoBehaviour
{
    public int baseMaxHealth;

    public class Baker : Baker<CreepHealthAuthoring>
    {
        public override void Bake(CreepHealthAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new CreepHealthComponent
            {
                currentHealth = authoring.baseMaxHealth,
                maxHealth = authoring.baseMaxHealth,
                baseMaxHealth = authoring.baseMaxHealth,
            });
        }
    }
}

public struct CreepHealthComponent : IComponentData
{
    public int currentHealth;
    public int maxHealth;
    public int baseMaxHealth;
}
