using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using UnityEngine;

public partial struct SlimeBulletHealPlayerSystem : ISystem
{
    private EntityManager entityManager;
    private Entity player;

    public void OnUpdate(ref SystemState state)
    {
        return;

        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        NativeList<Entity> bulletsToReturn = new NativeList<Entity>(Allocator.TempJob);

        var job = new SlimeBulletHealPlayerJob
        {
            slimeBulletLookup = SystemAPI.GetComponentLookup<SlimeBulletComponent>(true),
            slimeReclaimComponentLookup = SystemAPI.GetComponentLookup<SlimeReclaimComponent>(true),
            playerLookup = SystemAPI.GetComponentLookup<PlayerHealthComponent>(true),
            ecb = ecb,
            bulletsToReturn = bulletsToReturn,
        };

        state.Dependency = job.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
        state.Dependency.Complete();

        foreach (var bullet in bulletsToReturn)
        {
            BulletManager.Instance.Return(bullet, ecb);
        }

        bulletsToReturn.Dispose();
    }
}

[BurstCompile]
struct SlimeBulletHealPlayerJob : ITriggerEventsJob
{
    [ReadOnly] public ComponentLookup<SlimeBulletComponent> slimeBulletLookup;
    [ReadOnly] public ComponentLookup<SlimeReclaimComponent> slimeReclaimComponentLookup;
    [ReadOnly] public ComponentLookup<PlayerHealthComponent> playerLookup;
    public EntityCommandBuffer ecb;
    public NativeList<Entity> bulletsToReturn; 

    public void Execute(TriggerEvent triggerEvent)
    {
        Entity entityA = triggerEvent.EntityA;
        Entity entityB = triggerEvent.EntityB;

        bool entityAIsPlayer = playerLookup.HasComponent(entityA);
        bool entityBIsPlayer = playerLookup.HasComponent(entityB);

        if (entityAIsPlayer || entityBIsPlayer)
        {
            Entity slimeBulletEntity = entityAIsPlayer ? entityB : entityA;

            if (slimeBulletLookup.HasComponent(slimeBulletEntity))
            {
                var slimeBulletComponent = slimeBulletLookup[slimeBulletEntity];
                int healAmount = 0;

                if(!slimeBulletComponent.hasHealPlayer && (!slimeBulletComponent.isAbleToMove || slimeBulletComponent.isBeingSummoned))
                {
                    //collecting
                    if (!slimeBulletComponent.isAbleToMove)
                    {
                        healAmount = slimeBulletComponent.healPlayerAmount;
                    }
                    //being summoned => bonus HP
                    else if (slimeBulletComponent.isBeingSummoned)
                    {
                        var slimeReclaimComponent = slimeReclaimComponentLookup[slimeBulletEntity];
                        healAmount = (int)(slimeBulletComponent.healPlayerAmount * slimeReclaimComponent.hpHealPrecentPerBullet);
                    }

                    //Heal player
                    HealPlayer(entityAIsPlayer, entityA, entityB, healAmount);

                    //Set hasHealPlayer = true
                    ecb.SetComponent(slimeBulletEntity, new SlimeBulletComponent
                    {
                        hasHealPlayer = true,
                        isAbleToMove = slimeBulletComponent.isAbleToMove,
                        isBeingSummoned = slimeBulletComponent.isBeingSummoned,
                        damageEnemyAmount = slimeBulletComponent.damageEnemyAmount,
                        damagePlayerAmount = slimeBulletComponent.damagePlayerAmount,
                        distanceTraveled = slimeBulletComponent.distanceTraveled,
                        existDuration = slimeBulletComponent.existDuration,
                        maxDistance = slimeBulletComponent.maxDistance,
                        moveDirection = slimeBulletComponent.moveDirection,
                        healPlayerAmount = slimeBulletComponent.healPlayerAmount,
                        moveSpeed = slimeBulletComponent.moveSpeed,
                        hasDamagedEnemy = slimeBulletComponent.hasDamagedEnemy,
                    });

                    // Add bullet to return list
                    bulletsToReturn.Add(slimeBulletEntity);
                }
            }
        }
    }

    private void HealPlayer(bool isEntityAPlayer, Entity entityA, Entity entityB, int healAmount)
    {
        if (isEntityAPlayer)
        {
            ecb.AddComponent(entityA, new HealEventComponent { healAmount = healAmount });
        }
        else
        {
            ecb.AddComponent(entityB, new HealEventComponent { healAmount = healAmount });
        }
    }
}


