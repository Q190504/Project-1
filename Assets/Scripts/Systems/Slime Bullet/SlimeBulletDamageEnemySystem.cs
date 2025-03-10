using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

public partial struct SlimeBulletDamageEnemySystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        NativeList<ColliderCastHit> hits = new NativeList<ColliderCastHit>(Allocator.Temp);

        foreach (var (localTransform, slimeBulletComponent, entity) in SystemAPI.Query<RefRO<LocalTransform>, RefRW<SlimeBulletComponent>>().WithEntityAccess())
        {
            if (slimeBulletComponent.ValueRO.isAbleToMove)
            {
                hits.Clear();

                float3 point1 = new float3(localTransform.ValueRO.Position - slimeBulletComponent.ValueRO.colliderSize);
                float3 point2 = new float3(localTransform.ValueRO.Position + slimeBulletComponent.ValueRO.colliderSize);

                physicsWorldSingleton.CapsuleCastAll(point1, point2, slimeBulletComponent.ValueRO.colliderSize / 2, float3.zero, 1, ref hits, CollisionFilter.Default);

                foreach (ColliderCastHit hit in hits)
                {
                    if (entityManager.HasComponent<EnemyHealthComponent>(hit.Entity))
                    {
                        slimeBulletComponent.ValueRW.isAbleToMove = false;
                        ecb.AddComponent(hit.Entity, new DamageEventComponent
                        {
                            damageAmount = slimeBulletComponent.ValueRO.damageEnemyAmount,
                        });
                    }
                }
            }
        }

        hits.Dispose();
    }
}
