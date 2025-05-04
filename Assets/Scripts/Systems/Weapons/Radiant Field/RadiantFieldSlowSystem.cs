using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
[UpdateAfter(typeof(EnemyMoveSystem))]
public partial struct RadiantFieldSlowSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        // Temporarily track enemies that should remain slowed
        int estimatedEnemyCount = SystemAPI.QueryBuilder().WithAll<EnemyTagComponent>().Build().CalculateEntityCount();
        var stillSlowedEnemies = new NativeHashSet<Entity>(estimatedEnemyCount, Allocator.Temp);

        double currentTime = (float)SystemAPI.Time.ElapsedTime;

        var job = new RadiantFieldSlowEnemyJob
        {
            radiantFieldLookup = SystemAPI.GetComponentLookup<RadiantFieldComponent>(true),
            enemyLookup = SystemAPI.GetComponentLookup<EnemyTagComponent>(true),
            velocityLookup = SystemAPI.GetComponentLookup<PhysicsVelocity>(true),
            ecb = ecb,
            currentTime = currentTime,
            stillSlowedEnemies = stillSlowedEnemies
        };

        state.Dependency = job.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);

        // Remove the tag from enemies who are no longer in the radius
        foreach (var (slowedByRadiantFieldTag, entity) in SystemAPI.Query<RefRO<SlowedByRadiantFieldTag>>().WithEntityAccess())
        {
            if (!stillSlowedEnemies.Contains(entity))
            {
                ecb.RemoveComponent<SlowedByRadiantFieldTag>(entity);
            }
        }

        stillSlowedEnemies.Dispose();
    }
}

//[BurstCompile]
struct RadiantFieldSlowEnemyJob : ITriggerEventsJob
{
    [ReadOnly] public ComponentLookup<RadiantFieldComponent> radiantFieldLookup;
    [ReadOnly] public ComponentLookup<EnemyTagComponent> enemyLookup;
    [ReadOnly] public ComponentLookup<PhysicsVelocity> velocityLookup;
    [ReadOnly] public ComponentLookup<SlowedByRadiantFieldTag> slowedByRadiantFieldTagLookup;
    public EntityCommandBuffer ecb;
    public double currentTime;
    public NativeHashSet<Entity> stillSlowedEnemies;

    public void Execute(TriggerEvent triggerEvent)
    {
        Entity entityA = triggerEvent.EntityA;
        Entity entityB = triggerEvent.EntityB;

        bool entityAIsEnemy = enemyLookup.HasComponent(entityA);
        bool entityBIsEnemy = enemyLookup.HasComponent(entityB);

        if (entityAIsEnemy || entityBIsEnemy)
        {
            Entity enemyEntity = entityAIsEnemy ? entityA : entityB;
            Entity radiantFieldEntity = entityAIsEnemy ? entityB : entityA;

            if (!radiantFieldLookup.HasComponent(radiantFieldEntity) || !radiantFieldLookup.HasComponent(radiantFieldEntity))
                return;

            RadiantFieldComponent radiantFieldComponent = radiantFieldLookup[radiantFieldEntity];

            // Skip if has not pass a tick
            if (currentTime - radiantFieldComponent.lastTickTime < radiantFieldComponent.timeBetween)
                return;

            radiantFieldComponent.lastTickTime = currentTime;
            ecb.SetComponent(radiantFieldEntity, radiantFieldComponent);

            RadiantFieldLevelData currerntLevelData = radiantFieldComponent.Data.Value.Levels[radiantFieldComponent.currentLevel];
            // Slow enemy
            float slowModifier = currerntLevelData.slowModifier;
            if (slowModifier <= 0 || slowModifier >= 1)
                return;

            if (!slowedByRadiantFieldTagLookup.HasComponent(enemyEntity))
                ecb.AddComponent(enemyEntity, new SlowedByRadiantFieldTag());

            if (velocityLookup.HasComponent(enemyEntity))
            {
                PhysicsVelocity enemyVelocity = velocityLookup[enemyEntity];
                EnemyTagComponent enemyTagComponent = enemyLookup[enemyEntity];

                if (math.lengthsq(enemyVelocity.Linear) > 0)
                {
                    enemyVelocity.Linear = math.normalize(enemyVelocity.Linear) * (enemyTagComponent.speed * slowModifier);
                    ecb.SetComponent(enemyEntity, enemyVelocity);
                    Debug.Log("slow by rd");
                }

                // Mark this enemy as still being affected
                stillSlowedEnemies.Add(enemyEntity);
            }
        }
    }
}

