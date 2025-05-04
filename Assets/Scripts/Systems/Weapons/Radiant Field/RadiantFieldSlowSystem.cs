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
    private EntityManager entityManager;

    public void OnCreate(ref SystemState state)
    {
        entityManager = state.EntityManager;

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

        // Clean-up tags
        foreach (var (slowedByRadiantFieldTag, entity) in SystemAPI.Query<RefRO<SlowedByRadiantFieldTag>>().WithEntityAccess())
        {
            if (!entityManager.HasComponent<StillSlowedByRadiantFieldThisFrameTag>(entity))
                ecb.RemoveComponent<SlowedByRadiantFieldTag>(entity);
        }

        foreach (var (_, entity) in SystemAPI.Query<RefRW<StillSlowedByRadiantFieldThisFrameTag>>().WithEntityAccess())
        {
            ecb.RemoveComponent<StillSlowedByRadiantFieldThisFrameTag>(entity);
        }
    }
}

//[BurstCompile]
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

            // Skip if has not pass a tick
            if (currentTime - radiantFieldComponent.lastTickTime < radiantFieldComponent.timeBetween)
                return;

            radiantFieldComponent.lastTickTime = currentTime;
            ecb.SetComponent(radiantFieldEntity, radiantFieldComponent);

            RadiantFieldLevelData currerntLevelData = radiantFieldComponent.Data.Value.Levels[radiantFieldComponent.currentLevel];

            float slowModifier = currerntLevelData.slowModifier;


            if (slowModifier <= 0 || slowModifier >= 1)
                return;

            // Slow enemy
            if (velocityLookup.HasComponent(enemyEntity))
            {
                PhysicsVelocity enemyVelocity = velocityLookup[enemyEntity];
                EnemyTagComponent enemyTagComponent = enemyLookup[enemyEntity];

                if (math.lengthsq(enemyVelocity.Linear) > 0)
                {
                    enemyVelocity.Linear = math.normalize(enemyVelocity.Linear) * (enemyTagComponent.speed * slowModifier);
                    ecb.SetComponent(enemyEntity, enemyVelocity);

                    Debug.Log(enemyEntity.Index);
                }
            }

            // Add Tags
            if (!slowedByRadiantFieldTagLookup.HasComponent(enemyEntity))
                ecb.AddComponent(enemyEntity, new SlowedByRadiantFieldTag());

            ecb.AddComponent(enemyEntity, new StillSlowedByRadiantFieldThisFrameTag());
        }
    }
}