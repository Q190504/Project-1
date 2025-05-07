using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using UnityEngine;

[UpdateAfter(typeof(PawPrintPoisonCloudExistingSystem))]
[UpdateAfter(typeof(PawPrintPoisonerResetDamageTagSystem))]
public partial struct PawPrintPoisonCloudDamageSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PawPrintPoisonCloudComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        double currentTime = SystemAPI.Time.ElapsedTime;

        var job = new PawPrintPoisonCloudDamageEnemyJob
        {
            pawPrintPoisonCloudLookup = SystemAPI.GetComponentLookup<PawPrintPoisonCloudComponent>(true),
            enemyLookup = SystemAPI.GetComponentLookup<EnemyTagComponent>(true),
            damagedByPoisonCloudThisTickTagLookup = SystemAPI.GetComponentLookup<DamagedByPoisonCloudThisTickTag>(true),
            ecb = ecb,
            currentTime = currentTime,
        };

        state.Dependency = job.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
    }
}

[BurstCompile]
struct PawPrintPoisonCloudDamageEnemyJob : ITriggerEventsJob
{
    [ReadOnly] public ComponentLookup<PawPrintPoisonCloudComponent> pawPrintPoisonCloudLookup;
    [ReadOnly] public ComponentLookup<EnemyTagComponent> enemyLookup;
    [ReadOnly] public ComponentLookup<DamagedByPoisonCloudThisTickTag> damagedByPoisonCloudThisTickTagLookup;
    public EntityCommandBuffer ecb;
    [ReadOnly] public double currentTime;
    
    public void Execute(TriggerEvent triggerEvent)
    {
        Entity entityA = triggerEvent.EntityA;
        Entity entityB = triggerEvent.EntityB;

        bool entityAIsEnemy = enemyLookup.HasComponent(entityA);
        bool entityBIsEnemy = enemyLookup.HasComponent(entityB);

        if ((!entityAIsEnemy && entityBIsEnemy) || (entityAIsEnemy && !entityBIsEnemy))
        {
            Entity enemyEntity = entityAIsEnemy ? entityA : entityB;
            Entity cloudEntity = entityAIsEnemy ? entityB : entityA;

            // Check if the cloud has the PawPrintPoisonCloud component
            if (!pawPrintPoisonCloudLookup.HasComponent(cloudEntity) || !pawPrintPoisonCloudLookup.HasComponent(cloudEntity))
                return;

            var pawPrintPoisonCloudComponent = pawPrintPoisonCloudLookup[cloudEntity];

            // Return if hasn't pass a tick
            if (currentTime - pawPrintPoisonCloudComponent.lastTick < pawPrintPoisonCloudComponent.tick)
                return;

            // Pass if the enemy is already damaged by a cloud
            if (damagedByPoisonCloudThisTickTagLookup.HasComponent(enemyEntity))
                return;

            // Deal damage
            int damage = pawPrintPoisonCloudComponent.damagePerTick;

            if (damage <= 0)
                return;

            ecb.AddComponent(enemyEntity, new DamageEventComponent { damageAmount = damage });
            ecb.AddComponent(enemyEntity, new DamagedByPoisonCloudThisTickTag());
        }
    }
}
