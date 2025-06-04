using Unity.Entities;
using UnityEngine;

public class CreepDamageAuthoring : MonoBehaviour
{
    public int damage;

    public class Baker : Baker<CreepDamageAuthoring>
    {
        public override void Bake(CreepDamageAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new CreepDamageComponent
            {
                damage = authoring.damage,
            });
        }
    }
}

public struct CreepDamageComponent : IComponentData
{
    public int damage;
}
