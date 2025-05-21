using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;

public partial struct SlimeBulletDamageEnemySystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SlimeBulletComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        if (!GameManager.Instance.IsPlaying()) return;

        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

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

        if ((!entityAIsEnemy && entityBIsEnemy) || (entityAIsEnemy && !entityBIsEnemy))
        {
            Entity enemyEntity = entityAIsEnemy ? entityA : entityB;
            Entity bulletEntity = entityAIsEnemy ? entityB : entityA;

            if (!slimeBulletLookup.HasComponent(bulletEntity) || !slimeBulletLookup.HasComponent(bulletEntity))
                return;

            var bulletComponent = slimeBulletLookup[bulletEntity];

            // Skip if already hit this enemy or stopped moving
            if (!bulletComponent.isAbleToMove || bulletComponent.lastHitEnemy == enemyEntity)
                return;

            // Deal damage
            int damage = bulletComponent.remainingDamage;

            if (damage <= 0)
                return;

            ecb.AddComponent(enemyEntity, new DamageEventComponent { damageAmount = damage });

            // Reduce damage for future hits
            bulletComponent.remainingDamage = (int)(damage * bulletComponent.passthroughDamageModifier);
            bulletComponent.lastHitEnemy = enemyEntity;

            ecb.SetComponent(bulletEntity, bulletComponent);
        }
    }
}