using Unity.Entities;
using Unity.Transforms;
using Unity.Physics;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using UnityEngine;

public partial struct CreepAttackSystem : ISystem
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
        if (!GameManager.Instance.IsPlaying()) return;

        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        float currentTime = (float)SystemAPI.Time.ElapsedTime;

        var job = new CreepAttackJob
        {
            playerLookup = SystemAPI.GetComponentLookup<PlayerHealthComponent>(true),
            creepLookup = SystemAPI.GetComponentLookup<CreepTagComponent>(true),
            creepDamageLookup = SystemAPI.GetComponentLookup<CreepDamageComponent>(true),
            cooldownLookup = SystemAPI.GetComponentLookup<AttackCooldownComponent>(false),
            ecb = ecb,
            currentTime = currentTime,
        };

        state.Dependency = job.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
    }
}

[BurstCompile]
struct CreepAttackJob : ITriggerEventsJob
{
    [ReadOnly] public ComponentLookup<PlayerHealthComponent> playerLookup;
    [ReadOnly] public ComponentLookup<CreepTagComponent> creepLookup;
    [ReadOnly] public ComponentLookup<CreepDamageComponent> creepDamageLookup;
    public ComponentLookup<AttackCooldownComponent> cooldownLookup; 
    public EntityCommandBuffer ecb;

    public float currentTime;

    public void Execute(TriggerEvent triggerEvent)
    {
        Entity entityA = triggerEvent.EntityA;
        Entity entityB = triggerEvent.EntityB;

        bool entityAIsPlayer = playerLookup.HasComponent(entityA);
        bool entityBIsPlayer = playerLookup.HasComponent(entityB);

        if (entityAIsPlayer ||  entityBIsPlayer)
        {
            Entity enemyEntity = entityAIsPlayer ? entityB : entityA;
            Entity playerEntity = entityAIsPlayer ? entityA : entityB;

            if (creepLookup.HasComponent(enemyEntity))
            {
                var damageComponent = creepDamageLookup[enemyEntity];
                int damage = damageComponent.damage;

                if (cooldownLookup.HasComponent(enemyEntity))
                {
                    var cooldown = cooldownLookup[enemyEntity];

                    if (currentTime - cooldown.lastAttackTime >= cooldown.cooldownTime)
                    {
                        ecb.AddComponent(playerEntity, new DamageEventComponent { damageAmount = damage });

                        ecb.SetComponent(enemyEntity, new AttackCooldownComponent
                        {
                            lastAttackTime = currentTime,
                            cooldownTime = cooldown.cooldownTime
                        });
                    }
                }   
            }
        }
    }
}