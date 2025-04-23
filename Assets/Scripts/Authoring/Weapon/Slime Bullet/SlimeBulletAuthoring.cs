using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class SlimeBulletAuthoring : MonoBehaviour
{
    public class SlimeBulletBaker : Baker<SlimeBulletAuthoring>
    {
        public override void Bake(SlimeBulletAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new SlimeBulletComponent
            {
                isAbleToMove = true,
                moveDirection = 0,
                moveSpeed = 0,
                distanceTraveled = 0,
                maxDistance = 0,
                remainingDamage = 0,
                passthroughDamageModifier = 0,
                lastHitEnemy = Entity.Null,
                healPlayerAmount = 0,
                existDuration = 0,
                hasHealPlayer = false,
                isBeingSummoned = false,
                slowModifier = 0,
                slowRadius = 0,
            });
        }
    }
}

public struct SlimeBulletComponent : IComponentData
{
    public bool isAbleToMove;
    public bool isBeingSummoned;
    public float3 moveDirection;
    public float moveSpeed;
    public float distanceTraveled;
    public float maxDistance;
    public int remainingDamage;
    public float passthroughDamageModifier;
    public Entity lastHitEnemy;
    public int healPlayerAmount;
    public float existDuration;
    public bool hasHealPlayer;
    public float slowModifier;
    public float slowRadius;
}
