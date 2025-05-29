using Unity.Entities;
using UnityEngine;

public class PlayerHealthAuthoring : MonoBehaviour
{
    public int baseMaxHealth;

    public class PlayerHealthBaker : Baker<PlayerHealthAuthoring>
    {
        public override void Bake(PlayerHealthAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new PlayerHealthComponent
            {
                baseMaxHealth = authoring.baseMaxHealth,
                currentHealth = authoring.baseMaxHealth,
                maxHealth = authoring.baseMaxHealth,
            });
        }
    }
}

public struct PlayerHealthComponent : IComponentData
{
    public int baseMaxHealth;
    public int currentHealth;
    public int maxHealth;
}
