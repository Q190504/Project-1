using Unity.Entities;
using UnityEngine;

public class CreepHealthAuthoring : MonoBehaviour
{
    public float currentHealth;
    public float maxHealth;

    public class Baker : Baker<CreepHealthAuthoring>
    {
        public override void Bake(CreepHealthAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            authoring.currentHealth = authoring.maxHealth;

            AddComponent(entity, new CreepHealthComponent
            {
                currentHealth = authoring.currentHealth,
                maxHealth = authoring.maxHealth,
            });
        }
    }
}

public struct CreepHealthComponent : IComponentData
{
    public float currentHealth;
    public float maxHealth;
}
