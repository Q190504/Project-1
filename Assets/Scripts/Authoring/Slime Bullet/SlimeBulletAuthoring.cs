using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class SlimeBulletAuthoring : MonoBehaviour
{
    public bool isAbleToMove;
    public float3 moveDirection;
    public float moveSpeed;
    public float distanceTraveled;
    public float maxDistance;
    public float colliderSize;
    public int damageEnemyAmount;
    public int damagePlayerAmount;
    public int healPlayerAmount;
    public float existDuration;

    public class SlimeBulletBaker : Baker<SlimeBulletAuthoring>
    {
        public override void Bake(SlimeBulletAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            authoring.colliderSize = authoring.GetComponent<CapsuleCollider>().radius;

            AddComponent(entity, new SlimeBulletComponent
            {
                isAbleToMove = authoring.isAbleToMove,
                moveDirection = authoring.moveDirection,
                moveSpeed = authoring.moveSpeed,
                distanceTraveled = authoring.distanceTraveled,
                maxDistance = authoring.maxDistance,
                colliderSize = authoring.colliderSize,
                damageEnemyAmount = authoring.damageEnemyAmount,
                damagePlayerAmount = authoring.damagePlayerAmount,
                healPlayerAmount = authoring.healPlayerAmount,
                existDuration = authoring.existDuration,
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
    public float colliderSize;
    public int damageEnemyAmount;
    public int damagePlayerAmount;
    public int healPlayerAmount;
    public float existDuration;
}
