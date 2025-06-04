using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine.UIElements;

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
            enemyLocalTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true),
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
    [ReadOnly] public ComponentLookup<LocalTransform> enemyLocalTransformLookup;
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

            // Skip if (stopped moving and not being summoned) or already hit this enemy 
            if ((!bulletComponent.isAbleToMove && !bulletComponent.isBeingSummoned) || bulletComponent.lastHitEnemy == enemyEntity)
                return;

            // Deal damage
            int damage = bulletComponent.remainingDamage;

            if (damage <= 0)
                return;

            ecb.AddComponent(enemyEntity, new DamageEventComponent { damageAmount = damage });

            // Reduce damage for future hits if the bullet is not being summoned
            if (!bulletComponent.isBeingSummoned)
            {
                bulletComponent.remainingDamage = (int)(damage * bulletComponent.passthroughDamageModifier);
                bulletComponent.lastHitEnemy = enemyEntity;
            }

            LocalTransform enemyTransform;
            if (enemyLocalTransformLookup.HasComponent(enemyEntity))
            {
                enemyTransform = enemyLocalTransformLookup[enemyEntity];
                var eventEntity = ecb.CreateEntity();
                ecb.AddComponent(eventEntity, new PlaySFXEvent
                {
                    sfxId = SFXID.SlimeBulletHit,
                });
            }

            ecb.SetComponent(bulletEntity, bulletComponent);
        }
    }
}