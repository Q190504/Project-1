using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public partial struct SlimeBulletHealPlayerSystem : ISystem
{
    private EntityManager entityManager;
    private Entity player;

    public void OnUpdate(ref SystemState state)
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        if (!SystemAPI.TryGetSingletonEntity<PlayerTagComponent>(out player))
        {
            Debug.Log($"Cant Found Player Entity in SlimeBulletHealPlayerSystem!");
            return;
        }

        PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        NativeList<ColliderCastHit> hits = new NativeList<ColliderCastHit>(Allocator.Temp);

        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (localTransform, slimeBulletComponent, entity) in SystemAPI.Query<RefRO<LocalTransform>, RefRW<SlimeBulletComponent>>().WithEntityAccess())
        {
            if (!slimeBulletComponent.ValueRO.isAbleToMove || slimeBulletComponent.ValueRO.isBeingSummoned)
            {
                hits.Clear();

                float3 point1 = new float3(localTransform.ValueRO.Position - slimeBulletComponent.ValueRO.colliderSize);
                float3 point2 = new float3(localTransform.ValueRO.Position + slimeBulletComponent.ValueRO.colliderSize);

                physicsWorldSingleton.CapsuleCastAll(point1, point2, slimeBulletComponent.ValueRO.colliderSize / 2, float3.zero, 1, ref hits, CollisionFilter.Default);

                foreach (ColliderCastHit hit in hits)
                {
                    if (entityManager.HasComponent<PlayerHealthComponent>(hit.Entity))
                    {
                        //collecting
                        if (!slimeBulletComponent.ValueRO.isBeingSummoned)
                        {
                            ecb.AddComponent(hit.Entity, new HealEventComponent
                            {
                                healAmount = slimeBulletComponent.ValueRO.healPlayerAmount,
                            });
                        }
                        else //being summoned => bonus HP
                        {
                            SlimeReclaimComponent slimeReclaimComponent = entityManager.GetComponentData<SlimeReclaimComponent>(player);

                            ecb.AddComponent(hit.Entity, new HealEventComponent
                            {
                                healAmount = (int)(slimeBulletComponent.ValueRO.healPlayerAmount * slimeReclaimComponent.hpHealPrecentPerBullet),
                            });
                        }

                        BulletManager.Instance.Return(entity, ecb);
                    }
                }
            }
        }

        hits.Dispose();
    }
}

