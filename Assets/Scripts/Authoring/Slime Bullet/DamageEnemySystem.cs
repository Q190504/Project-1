using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Physics;
using Unity.Burst;
using Unity.Collections;
using static UnityEngine.UI.Image;
using Unity.Mathematics;

[BurstCompile]
public partial struct SlimeBulletDamageEnemySystem : ISystem
{
    [ReadOnly]
    public ComponentLookup<EnemyTagComponent> EnemyLookup;
    public void OnUpdate(ref SystemState state)
    {
        PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;

        // Define the sphere collider shape for OverlapSphere
        var enemyFilter = new CollisionFilter
        {
            BelongsTo = 1 << 7,  
            CollidesWith = 1 << 8, 
            GroupIndex = 0
        };

        foreach (var (localTransform, slimeBulletComponent) in SystemAPI.Query<RefRO<LocalTransform>, RefRW<SlimeBulletComponent>>().WithAbsent<Disabled>())
        {
            if (slimeBulletComponent.ValueRO.isAbleToMove)
            {
                float3 circleCenter = localTransform.ValueRO.Position;
                float circleRadius = slimeBulletComponent.ValueRO.colliderSize;

                Aabb boundingBox = new Aabb
                {
                    Min = new float3(circleCenter.x - circleRadius, circleCenter.y - circleRadius, 0f),
                    Max = new float3(circleCenter.x + circleRadius, circleCenter.y + circleRadius, 0f)
                };

                OverlapAabbInput overlapInput = new OverlapAabbInput
                {
                    Aabb = boundingBox,
                    Filter = enemyFilter
                };

                NativeList<int> overlappingBodies = new NativeList<int>(Allocator.Temp);
                physicsWorldSingleton.OverlapAabb(overlapInput, ref overlappingBodies);

                foreach (var bodyIndex in overlappingBodies)
                {
                    Entity hitEntity = physicsWorldSingleton.Bodies[bodyIndex].Entity;
                    float3 entityPosition = physicsWorldSingleton.Bodies[bodyIndex].WorldFromBody.pos;

                    // Ensure it's within the actual circle (since AABB is a square approximation)
                    if (math.distance(entityPosition.x, circleCenter) <= circleRadius || math.distance(entityPosition.y, circleCenter) <= circleRadius)
                    {
                        UnityEngine.Debug.Log($"Entity {hitEntity} is truly inside the circle.");
                    }
                }
            }
        }
    }
}
