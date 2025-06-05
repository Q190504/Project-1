using Unity.Entities;
using UnityEngine;

public class CreepDamageAuthoring : MonoBehaviour
{
    public int baseDamage;

    public class Baker : Baker<CreepDamageAuthoring>
    {
        public override void Bake(CreepDamageAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new CreepDamageComponent
            {
                baseDamage = authoring.baseDamage,
                damage = authoring.baseDamage,
            });
        }
    }
}

public struct CreepDamageComponent : IComponentData
{
    public int damage;
    public int baseDamage;
}
