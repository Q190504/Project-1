using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using UnityEngine;

[BurstCompile]
[UpdateAfter(typeof(EnemyMoveSystem))]
public partial struct SlimeBulletSlowEnemySystem : ISystem
{
    private EntityManager entityManager;

    public void OnCreate(ref SystemState state)
    {
        entityManager = state.EntityManager;
        state.RequireForUpdate<SlimeBulletComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        // Temporarily track enemies that should remain slowed
        int estimatedEnemyCount = SystemAPI.QueryBuilder().WithAll<EnemyTagComponent>().Build().CalculateEntityCount();
        var stillSlowedEnemies = new NativeHashSet<Entity>(estimatedEnemyCount, Allocator.Temp);

        // First, check all bullets
        foreach (var (bulletComponent, bulletTransform, bulletEntity) in
            SystemAPI.Query<RefRO<SlimeBulletComponent>, RefRO<LocalTransform>>().WithEntityAccess())
        {
            if (bulletComponent.ValueRO.isAbleToMove || bulletComponent.ValueRO.isBeingSummoned)
                continue;

            float3 bulletPos = bulletTransform.ValueRO.Position;
            float radius = bulletComponent.ValueRO.slowRadius;
            float slowModifier = bulletComponent.ValueRO.slowModifier;

            if (slowModifier > 0)
            {
                DebugDrawSphere(bulletPos, radius, Color.cyan);

                foreach (var (transform, enemyComponent, velocity, entity) in
                    SystemAPI.Query<RefRO<LocalTransform>, RefRO<EnemyTagComponent>, RefRW<PhysicsVelocity>>().WithEntityAccess())
                {
                    if (!entityManager.HasComponent<EnemyTagComponent>(entity))
                        continue;

                    float3 toCenter = bulletPos - transform.ValueRO.Position;
                    float dist = math.length(toCenter);

                    if (dist <= radius && dist > 0.01f)
                    {
                        if (!state.EntityManager.HasComponent<SlowedBySlimeBulletTag>(entity))
                            ecb.AddComponent(entity, new SlowedBySlimeBulletTag());

                        if (math.lengthsq(velocity.ValueRO.Linear) > 0)
                            velocity.ValueRW.Linear = math.normalize(velocity.ValueRO.Linear) * (slowModifier);

                        // Mark this enemy as still being affected
                        if(!stillSlowedEnemies.Contains(entity))
                            stillSlowedEnemies.Add(entity);
                    }
                }
            }
        }

        // Remove the tag from enemies who are no longer in any radius
        foreach (var (slowedBySlimeBulletTag, entity) in SystemAPI.Query<RefRO<SlowedBySlimeBulletTag>>().WithEntityAccess())
        {
            if (!stillSlowedEnemies.Contains(entity))
            {
                ecb.RemoveComponent<SlowedBySlimeBulletTag>(entity);
            }
        }

        ecb.Playback(state.EntityManager);
        stillSlowedEnemies.Dispose();
    }


    void DebugCapsuleCast(float3 position1, float3 position2, float radius)
    {
        // Draw the capsule with lines
        Debug.DrawLine(position1, position2, Color.red, 0.1f);

        // Draw spheres at capsule ends to visualize the capsule shape
        DebugDrawSphere(position1, radius, Color.cyan);
        DebugDrawSphere(position2, radius, Color.cyan);
    }

    // Helper method to draw a sphere using Debug.DrawLine
    void DebugDrawSphere(float3 center, float radius, Color color)
    {
        int segments = 16;
        for (int i = 0; i < segments; i++)
        {
            float angle1 = (i / (float)segments) * math.PI * 2;
            float angle2 = ((i + 1) / (float)segments) * math.PI * 2;

            float3 p1 = center + new float3(math.cos(angle1), math.sin(angle1), 0) * radius;
            float3 p2 = center + new float3(math.cos(angle2), math.sin(angle2), 0) * radius;

            Debug.DrawLine(p1, p2, color, 0.1f);
        }
    }
}
