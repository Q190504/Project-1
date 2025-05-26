using Unity.Entities;
using UnityEngine;

public class PlayerHealthAuthoring : MonoBehaviour
{
    public int baseMaxHealth;

    public int currentLevel;
    public int maxLevel;
    public int increment;

    public class PlayerHealthBaker : Baker<PlayerHealthAuthoring>
    {
        public override void Bake(PlayerHealthAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new PlayerHealthComponent
            {
                baseMaxHealth = authoring.baseMaxHealth,
                currentHealth = authoring.baseMaxHealth,
                maxHealth = authoring.baseMaxHealth,

                currentLevel = authoring.currentLevel,
                maxLevel = authoring.maxLevel,
                increment = authoring.increment,
            });
        }
    }
}

public struct PlayerHealthComponent : IComponentData
{
    public int baseMaxHealth;
    public int currentHealth;
    public int maxHealth;

    public int currentLevel;
    public int maxLevel;
    public int increment;
}
