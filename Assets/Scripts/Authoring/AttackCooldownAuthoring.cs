using Unity.Entities;
using UnityEngine;

public class AttackCooldownAuthoring : MonoBehaviour
{
    public float cooldownTime;

    public class AttackCooldownBaker : Baker<AttackCooldownAuthoring>
    {
        public override void Bake(AttackCooldownAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new AttackCooldownComponent
            {
                lastAttackTime = 0,
                cooldownTime = authoring.cooldownTime,
            });
        }
    }
}

public struct AttackCooldownComponent : IComponentData
{
    public float lastAttackTime;
    public float cooldownTime;
}
