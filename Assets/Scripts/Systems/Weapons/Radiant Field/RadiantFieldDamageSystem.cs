using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Collections;
using UnityEngine;

[BurstCompile]
public partial struct RadiantFieldDamageSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        double currentTime = (float)SystemAPI.Time.ElapsedTime;

        var job = new RadiantFieldDamageEnemyJob
        {
            radiantFieldLookup = SystemAPI.GetComponentLookup<RadiantFieldComponent>(true),
            enemyLookup = SystemAPI.GetComponentLookup<EnemyTagComponent>(true),
            ecb = ecb,
            currentTime = currentTime,
        };

        state.Dependency = job.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
    }
}

[BurstCompile]
struct RadiantFieldDamageEnemyJob : ITriggerEventsJob
{
    [ReadOnly] public ComponentLookup<RadiantFieldComponent> radiantFieldLookup;
    [ReadOnly] public ComponentLookup<EnemyTagComponent> enemyLookup;
    public EntityCommandBuffer ecb;
    public double currentTime;

    public void Execute(TriggerEvent triggerEvent)
    {
        Entity entityA = triggerEvent.EntityA;
        Entity entityB = triggerEvent.EntityB;

        bool entityAIsEnemy = enemyLookup.HasComponent(entityA);
        bool entityBIsEnemy = enemyLookup.HasComponent(entityB);

        if ((!entityAIsEnemy && entityBIsEnemy) || (entityAIsEnemy && !entityBIsEnemy))
        {
            Entity enemyEntity = entityAIsEnemy ? entityA : entityB;
            Entity radiantFieldEntity = entityAIsEnemy ? entityB : entityA;

            if (!radiantFieldLookup.HasComponent(radiantFieldEntity))
            {
                return;
            }

            var radiantFieldComponent = radiantFieldLookup[radiantFieldEntity];

            if (radiantFieldComponent.currentLevel <= 0) // is inactive
            {
                Debug.Log($"radiant Field is inactive");
                return;
            }

            // Skip if has not pass a tick
            if (currentTime - radiantFieldComponent.lastTickTime < radiantFieldComponent.timeBetween)
                return;

            // Deal damage
            RadiantFieldLevelData currerntLevelData = radiantFieldComponent.Data.Value.Levels[radiantFieldComponent.currentLevel];
            int damage = currerntLevelData.damagePerTick;

            if (damage <= 0)
                return;

            ecb.AddComponent(enemyEntity, new DamageEventComponent { damageAmount = damage });
        }
    }
}
