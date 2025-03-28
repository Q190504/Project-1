using Unity.Entities;
using Unity.Transforms;
using Unity.Physics;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

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
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        float currentTime = (float)SystemAPI.Time.ElapsedTime;

        var job = new CreepAttackJob
        {
            playerLookup = SystemAPI.GetComponentLookup<PlayerHealthComponent>(true),
            enemyLookup = SystemAPI.GetComponentLookup<EnemyTagComponent>(true),
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
    [ReadOnly] public ComponentLookup<EnemyTagComponent> enemyLookup;
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

            if (enemyLookup.HasComponent(enemyEntity))
            {
                var enemyComponent = enemyLookup[enemyEntity];
                int damage = enemyComponent.damage;

                if (cooldownLookup.HasComponent(enemyEntity))
                {
                    var cooldown = cooldownLookup[enemyEntity];

                    if (currentTime - cooldown.lastAttackTime >= cooldown.cooldownTime)
                    {
                        if (entityAIsPlayer)
                        {
                            ecb.AddComponent(entityA, new DamageEventComponent { damageAmount = damage });
                        }
                        else if (entityBIsPlayer)
                        {
                            ecb.AddComponent(entityB, new DamageEventComponent { damageAmount = damage });
                        }

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