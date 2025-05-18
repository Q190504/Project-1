using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using Unity.Mathematics;

[BurstCompile]
[UpdateAfter(typeof(PawPrintPoisonCloudExistingSystem))]
public partial struct PawPrintPoisonCloudDamageSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PawPrintPoisonCloudComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        if (!GameManager.Instance.GetGameState()) return;

        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

        foreach (var (cloud, transform, cloudEntity) in SystemAPI.Query<RefRW<PawPrintPoisonCloudComponent>, RefRW<LocalTransform>>().WithEntityAccess())
        {
            cloud.ValueRW.tickTimer -= SystemAPI.Time.DeltaTime;
            if (cloud.ValueRW.tickTimer <= 0)
            {
                NativeList<ColliderCastHit> hits = new NativeList<ColliderCastHit>(Allocator.Temp);

                physicsWorldSingleton.SphereCastAll(transform.ValueRO.Position, cloud.ValueRO.cloudRadius / 2, float3.zero, 1,
                    ref hits, CollisionFilter.Default);

                //DebugDrawSphere(transform.ValueRO.Position, cloud.ValueRO.cloudRadius / 2, Color.magenta);

                int damage = cloud.ValueRO.damagePerTick;
                foreach (var enemy in hits)
                {
                    // Check if the hit entity is an enemy
                    if (!SystemAPI.HasComponent<EnemyTagComponent>(enemy.Entity))
                        continue;

                    ecb.AddComponent(enemy.Entity, new DamageEventComponent { damageAmount = damage });

                    double elapsedTime = SystemAPI.Time.ElapsedTime;
                }

                cloud.ValueRW.tickTimer = cloud.ValueRO.tick;
                hits.Dispose();
            }
        }
    }

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