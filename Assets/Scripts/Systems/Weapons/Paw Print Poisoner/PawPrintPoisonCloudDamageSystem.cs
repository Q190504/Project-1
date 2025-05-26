using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using Unity.Mathematics;

[BurstCompile]
[UpdateAfter(typeof(GameInitializationSystem))]
[UpdateAfter(typeof(PawPrintPoisonCloudExistingSystem))]
[UpdateAfter(typeof(PawPrintPoisonerSystem))]
public partial struct PawPrintPoisonCloudDamageSystem : ISystem
{
    private EntityManager entityManager;

    public void OnCreate(ref SystemState state)
    {
        entityManager = state.EntityManager;
        state.RequireForUpdate<PawPrintPoisonCloudComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        if (!GameManager.Instance.IsPlaying()) return;

        if (!SystemAPI.TryGetSingletonEntity<PawPrintPoisonerComponent>(out var pawPrintPoisoner))
        {
            Debug.Log($"Cant Found Paw Print Poisoner Entity in PawPrintPoisonCloudDamageSystem!");
            return;
        }

        PawPrintPoisonerComponent pawPrintPoisonerComponent = SystemAPI.GetSingleton<PawPrintPoisonerComponent>();
        if (pawPrintPoisonerComponent.level == 0)
            return;

        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

        foreach (var (cloud, transform, cloudEntity) in SystemAPI.Query<RefRW<PawPrintPoisonCloudComponent>, RefRW<LocalTransform>>().WithEntityAccess())
        {
            cloud.ValueRW.tickTimer -= SystemAPI.Time.DeltaTime;
            if (cloud.ValueRW.tickTimer <= 0)
            {
                NativeList<DistanceHit> hits = new NativeList<DistanceHit>(Allocator.Temp);

                CollisionFilter filter = new CollisionFilter
                {
                    BelongsTo = 1 << 4,
                    CollidesWith = 1 << 3,
                    GroupIndex = 0
                };

                physicsWorldSingleton.OverlapSphere(transform.ValueRO.Position, cloud.ValueRO.cloudRadius / 2,
                ref hits, filter);

                //DebugDrawSphere(transform.ValueRO.Position, cloud.ValueRO.cloudRadius / 2, Color.magenta);

                int damage = cloud.ValueRO.damagePerTick;

                if(damage > 0)
                {
                    foreach (var enemy in hits)
                    {
                        // Check if the hit entity is an enemy
                        if (!SystemAPI.HasComponent<EnemyTagComponent>(enemy.Entity))
                            continue;

                        ecb.AddComponent(enemy.Entity, new DamageEventComponent { damageAmount = damage });
                    }
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