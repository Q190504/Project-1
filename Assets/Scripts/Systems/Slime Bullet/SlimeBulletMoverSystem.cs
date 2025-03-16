using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine.InputSystem;

[BurstCompile]
public partial struct SlimeBulletMoverSystem : ISystem
{
    private EntityManager entityManager;
    private Entity player;

    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        if (!SystemAPI.TryGetSingletonEntity<PlayerTagComponent>(out player))
        {
            Debug.Log($"Cant Found Player Entity in SlimeBulletMoverSystem!");
            return;
        }

        foreach (var (localTransform, slimeBulletComponent, physicsVelocity, entity) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<SlimeBulletComponent>, RefRW<PhysicsVelocity>>().WithEntityAccess())
        {
            if (slimeBulletComponent.ValueRO.isAbleToMove)
            {
                float2 targetVelocity = new float2(slimeBulletComponent.ValueRO.moveDirection.x, slimeBulletComponent.ValueRO.moveDirection.y) * slimeBulletComponent.ValueRO.moveSpeed;
                physicsVelocity.ValueRW.Linear.xy = math.lerp(physicsVelocity.ValueRW.Linear.xy, targetVelocity, 0.1f);

                slimeBulletComponent.ValueRW.distanceTraveled += slimeBulletComponent.ValueRO.moveSpeed * SystemAPI.Time.DeltaTime;

                if (slimeBulletComponent.ValueRO.distanceTraveled >= slimeBulletComponent.ValueRO.maxDistance)
                    slimeBulletComponent.ValueRW.isAbleToMove = false;
            }
            else if(!slimeBulletComponent.ValueRO.isAbleToMove)
            {
                physicsVelocity.ValueRW.Linear.xy = 0;
            }
            else if(slimeBulletComponent.ValueRO.isBeingSummoned)
            {
                //return to player

            }
            else
            {
                if (slimeBulletComponent.ValueRO.existDuration <= 0)
                    BulletManager.Instance.Return(entity, ecb);
                else
                    slimeBulletComponent.ValueRW.existDuration -= SystemAPI.Time.DeltaTime;
            }
        }
    }
}
