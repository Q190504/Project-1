using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class SlimeBulletAuthoring : MonoBehaviour
{
    public bool isAbleToMove;
    public float3 moveDirection;
    public float moveSpeed;
    public float distanceTraveled;
    public float minimumDistance;
    public int damageEnemyAmount;
    public int damagePlayerAmount;
    public int healPlayerAmount;
    public float existDuration;

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
                damageEnemyAmount = 0,
                damagePlayerAmount = 0,
                healPlayerAmount = 0,
                existDuration = 0,
                hasHealPlayer = false,
                isBeingSummoned = false,
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
    public int damageEnemyAmount;
    public int damagePlayerAmount;
    public int healPlayerAmount;
    public float existDuration;
    public bool hasHealPlayer;
}
