using Unity.Entities;
using UnityEngine;

public class PlayerHealthAuthoring : MonoBehaviour
{
    public int currentHealth;
    public int maxHealth;

    public int currentLevel;
    public int maxLevel;
    public float increment;

    public class PlayerHealthBaker : Baker<PlayerHealthAuthoring>
    {
        public override void Bake(PlayerHealthAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            authoring.currentHealth = authoring.maxHealth;

            AddComponent(entity, new PlayerHealthComponent
            {
                currentHealth = authoring.currentHealth,
                maxHealth = authoring.maxHealth,

                currentLevel = authoring.currentLevel,
                maxLevel = authoring.maxLevel,
                increment = authoring.increment,
            });
        }
    }
}

public struct PlayerHealthComponent : IComponentData
{
    public int currentHealth;
    public int maxHealth;

    public int currentLevel;
    public int maxLevel;
    public float increment;
}
