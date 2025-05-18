using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Mathematics;

public partial struct SlimeBulletStopMovingSystem : ISystem

{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SimulationSingleton>();
    }

    public void OnUpdate(ref SystemState state)
    {
        if (!GameManager.Instance.GetGameState()) return;

        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        var job = new SlimeBulletStopJob
        {
            slimeBulletLookup = SystemAPI.GetComponentLookup<SlimeBulletComponent>(false),
            impassibleTagLookup = SystemAPI.GetComponentLookup<ImpassibleTagComponent>(true),
            physicsVelocityLookup = SystemAPI.GetComponentLookup<PhysicsVelocity>(false),
            ecb = ecb,
        };

        state.Dependency = job.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
    }
}


[BurstCompile]
struct SlimeBulletStopJob : ICollisionEventsJob
{
    public ComponentLookup<SlimeBulletComponent> slimeBulletLookup;
    public ComponentLookup<PhysicsVelocity> physicsVelocityLookup;
    [ReadOnly] public ComponentLookup<ImpassibleTagComponent> impassibleTagLookup;
    public EntityCommandBuffer ecb;

    public void Execute(CollisionEvent collisionEvent)
    {
        Entity entityA = collisionEvent.EntityA;
        Entity entityB = collisionEvent.EntityB;

        bool entityAIsWall = impassibleTagLookup.HasComponent(entityA);
        bool entityBIsWall = impassibleTagLookup.HasComponent(entityB);

        if ((!entityAIsWall && entityBIsWall) || (entityAIsWall && !entityBIsWall))
        {
            Entity bullet = entityAIsWall ? entityB : entityA;

            if (!slimeBulletLookup.HasComponent(bullet))
                return;

            var bulletComponent = slimeBulletLookup[bullet];

            // Skip if already stopped moving
            if (!bulletComponent.isAbleToMove)
                return;

            bulletComponent.isAbleToMove = false;

            var physicsVelocity = physicsVelocityLookup[bullet];
            physicsVelocity.Linear.xy = float2.zero;

            ecb.SetComponent(bullet, bulletComponent);
            ecb.SetComponent(bullet, physicsVelocity);
        }
    }
}
