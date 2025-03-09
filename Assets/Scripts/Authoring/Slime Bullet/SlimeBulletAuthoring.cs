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
    public float damageAmount;

    public class SlimeBulletBaker : Baker<SlimeBulletAuthoring>
    {
        public override void Bake(SlimeBulletAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new SlimeBulletComponent
            {
                isAbleToMove = authoring.isAbleToMove,
                moveDirection = authoring.moveDirection,
                moveSpeed = authoring.moveSpeed,
                distanceTraveled = authoring.distanceTraveled,
                maxDistance = authoring.maxDistance,
                colliderSize = authoring.colliderSize,
                damageAmount = authoring.damageAmount,
            });
        }
    }
}

public struct SlimeBulletComponent : IComponentData
{
    public bool isAbleToMove;
    public float3 moveDirection;
    public float moveSpeed;
    public float distanceTraveled;
    public float maxDistance;
    public float colliderSize;
    public float damageAmount;
}
