using Unity.Entities;
using UnityEngine;

public class PlayerHealthAuthoring : MonoBehaviour
{
    public float currentHealth;
    public float maxHealth;

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
            });
        }
    }
}

public struct PlayerHealthComponent : IComponentData
{
    public float currentHealth;
    public float maxHealth;
}
