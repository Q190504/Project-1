using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public partial struct SlimeBulletDamageEnemySystem : ISystem
{
    private EntityQuery _collisionGroup;

    public void OnCreate(ref SystemState state)
    {
        _collisionGroup = SystemAPI.QueryBuilder()
            .WithAll<PhysicsCollider, PhysicsVelocity>()
            .Build();
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        float currentTime = (float)SystemAPI.Time.ElapsedTime;

        var job = new SlimeBulletDamageEnemyJob
        {
            slimeBulletLookup = SystemAPI.GetComponentLookup<SlimeBulletComponent>(true),
            enemyLookup = SystemAPI.GetComponentLookup<EnemyTagComponent>(true),
            ecb = ecb,
        };

        state.Dependency = job.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
    }
}


[BurstCompile]
struct SlimeBulletDamageEnemyJob : ITriggerEventsJob
{
    [ReadOnly] public ComponentLookup<SlimeBulletComponent> slimeBulletLookup;
    [ReadOnly] public ComponentLookup<EnemyTagComponent> enemyLookup;
    public EntityCommandBuffer ecb;

    public void Execute(TriggerEvent triggerEvent)
    {
        Entity entityA = triggerEvent.EntityA;
        Entity entityB = triggerEvent.EntityB;

        bool entityAIsEnemy = enemyLookup.HasComponent(entityA);
        bool entityBIsEnemy = enemyLookup.HasComponent(entityB);


        if (entityAIsEnemy || entityBIsEnemy)
        {
            Entity slimeBulletEntity = entityAIsEnemy ? entityB : entityA;

            if (slimeBulletLookup.HasComponent(slimeBulletEntity))
            {
                var slimeBulletComponent = slimeBulletLookup[slimeBulletEntity];

                if (!slimeBulletComponent.hasDamagedEnemy && (slimeBulletComponent.isAbleToMove || slimeBulletComponent.isBeingSummoned))
                {
                    int damage = slimeBulletComponent.damageEnemyAmount;

                    if (entityAIsEnemy)
                    {
                        ecb.AddComponent(entityA, new DamageEventComponent { damageAmount = damage });
                    }
                    else if (entityBIsEnemy)
                    {
                        ecb.AddComponent(entityB, new DamageEventComponent { damageAmount = damage });
                    }

                    // Set slimeBulletComponent.isAbleToMove to false & hasDamagedEnemy = true
                    ecb.SetComponent(slimeBulletEntity, new SlimeBulletComponent
                    {
                        isAbleToMove = false,
                        hasDamagedEnemy = true,
                        isBeingSummoned = slimeBulletComponent.isBeingSummoned,
                        damageEnemyAmount = slimeBulletComponent.damageEnemyAmount,
                        damagePlayerAmount = slimeBulletComponent.damagePlayerAmount,
                        distanceTraveled = slimeBulletComponent.distanceTraveled,
                        existDuration = slimeBulletComponent.existDuration,
                        maxDistance = slimeBulletComponent.maxDistance,
                        moveDirection = slimeBulletComponent.moveDirection,
                        healPlayerAmount = slimeBulletComponent.healPlayerAmount,
                        moveSpeed = slimeBulletComponent.moveSpeed,
                        hasHealPlayer = slimeBulletComponent.hasHealPlayer,
                    });
                }
            }
        }
    }
}
