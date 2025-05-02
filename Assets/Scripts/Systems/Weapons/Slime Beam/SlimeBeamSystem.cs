using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Collections;

public partial struct SlimeBeamSystem : ISystem
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
        float deltaTime = SystemAPI.Time.DeltaTime;
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        var job = new SlimeBeamDamageEnemyJob
        {
            slimeBeamLookup = SystemAPI.GetComponentLookup<SlimeBeamComponent>(true),
            enemyLookup = SystemAPI.GetComponentLookup<EnemyTagComponent>(true),
            ecb = ecb,
            deltaTime = deltaTime,
        };

        state.Dependency = job.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
    }
}

//[BurstCompile]
struct SlimeBeamDamageEnemyJob : ITriggerEventsJob
{
    [ReadOnly] public ComponentLookup<SlimeBeamComponent> slimeBeamLookup;
    [ReadOnly] public ComponentLookup<EnemyTagComponent> enemyLookup;
    [ReadOnly] public float deltaTime;
    public EntityCommandBuffer ecb;

    public void Execute(TriggerEvent triggerEvent)
    {
        Entity entityA = triggerEvent.EntityA;
        Entity entityB = triggerEvent.EntityB;

        bool entityAIsEnemy = enemyLookup.HasComponent(entityA);
        bool entityBIsEnemy = enemyLookup.HasComponent(entityB);

        if (entityAIsEnemy || entityBIsEnemy)
        {
            Entity enemyEntity = entityAIsEnemy ? entityA : entityB;
            Entity beamEntity = entityAIsEnemy ? entityB : entityA;

            if (!slimeBeamLookup.HasComponent(beamEntity) || !slimeBeamLookup.HasComponent(beamEntity))
                return;

            var beamComponent = slimeBeamLookup[beamEntity];

            beamComponent.timer -= deltaTime;
            // Destroy if out of lifetime
            if (beamComponent.timer <= 0)
            {
                BulletManager.Instance.ReturnSlimeBeam(beamEntity, ecb);
                return;
            }

            // Skip if has deal damage to enemies in frame(s) before
            if (beamComponent.hasDealDamageToEnemies)
                return;

            // Deal damage
            int damage = beamComponent.damage;

            if (damage <= 0)
                return;

            ecb.AddComponent(enemyEntity, new DamageEventComponent { damageAmount = damage });

            beamComponent.hasDealDamageToEnemies = true;
            ecb.SetComponent(beamEntity, beamComponent);
        }
    }
}