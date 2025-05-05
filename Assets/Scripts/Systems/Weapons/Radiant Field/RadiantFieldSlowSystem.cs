using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
[UpdateAfter(typeof(EnemyMoveSystem))]
public partial struct RadiantFieldSlowSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<RadiantFieldComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        double currentTime = (float)SystemAPI.Time.ElapsedTime;

        var job = new RadiantFieldSlowEnemyJob
        {
            radiantFieldLookup = SystemAPI.GetComponentLookup<RadiantFieldComponent>(true),
            enemyLookup = SystemAPI.GetComponentLookup<EnemyTagComponent>(true),
            velocityLookup = SystemAPI.GetComponentLookup<PhysicsVelocity>(false),
            slowedByRadiantFieldTagLookup = SystemAPI.GetComponentLookup<SlowedByRadiantFieldTag>(true),
            ecb = ecb,
            currentTime = currentTime,
        };

        var jobHandle = job.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);

        jobHandle.Complete();
    }
}

[BurstCompile]
struct RadiantFieldSlowEnemyJob : ITriggerEventsJob
{
    [ReadOnly] public ComponentLookup<RadiantFieldComponent> radiantFieldLookup;
    [ReadOnly] public ComponentLookup<EnemyTagComponent> enemyLookup;
    public ComponentLookup<PhysicsVelocity> velocityLookup;
    [ReadOnly] public ComponentLookup<SlowedByRadiantFieldTag> slowedByRadiantFieldTagLookup;
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

            if (!radiantFieldLookup.HasComponent(radiantFieldEntity) || !radiantFieldLookup.HasComponent(radiantFieldEntity))
                return;

            RadiantFieldComponent radiantFieldComponent = radiantFieldLookup[radiantFieldEntity];

            if (radiantFieldComponent.currentLevel <= 0) // is inactive
            {
                return;
            }

            RadiantFieldLevelData currerntLevelData = radiantFieldComponent.Data.Value.Levels[radiantFieldComponent.currentLevel];

            float slowModifier = currerntLevelData.slowModifier;

            if (slowModifier <= 0 || slowModifier >= 1)
                return;

            if (!slowedByRadiantFieldTagLookup.HasComponent(enemyEntity))
            {
                // Slow enemy
                if (velocityLookup.HasComponent(enemyEntity))
                {
                    PhysicsVelocity enemyVelocity = velocityLookup[enemyEntity];

                    if (math.lengthsq(enemyVelocity.Linear) > 0)
                    {
                        enemyVelocity.Linear = math.normalize(enemyVelocity.Linear) * slowModifier;
                        ecb.SetComponent(enemyEntity, enemyVelocity);
                    }
                }
            }
        }
    }
}