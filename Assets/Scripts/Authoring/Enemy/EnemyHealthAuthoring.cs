using Unity.Entities;
using UnityEngine;

public class EnemyHealthAuthoring : MonoBehaviour
{
    public float currentHealth;
    public float maxHealth;


    public class EnemyHealthBaker : Baker<EnemyHealthAuthoring>
    {
        public override void Bake(EnemyHealthAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            authoring.currentHealth = authoring.maxHealth;

            AddComponent(entity, new EnemyHealthComponent
            {
                currentHealth = authoring.currentHealth,
                maxHealth = authoring.maxHealth,
            });
        }
    }
}

public struct EnemyHealthComponent : IComponentData
{
    public float currentHealth;
    public float maxHealth;
}
