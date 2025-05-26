using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Collections;
using UnityEngine;

[BurstCompile]
public partial struct RadiantFieldDamageSystem : ISystem
{
    private EntityManager entityManager;

    public void OnCreate(ref SystemState state)
    {
        entityManager = state.EntityManager;
        state.RequireForUpdate<RadiantFieldComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        if (!GameManager.Instance.IsPlaying()) return;

        if (!SystemAPI.TryGetSingletonEntity<PlayerTagComponent>(out var player))
        {
            Debug.LogError("Cant find Player Entity in RadiantFieldDamageSystem");
        }

        GenericDamageModifierComponent genericDamageModifierComponent;
        float genericDamageModifier = 0;
        if (SystemAPI.HasComponent<GenericDamageModifierComponent>(player))
        {
            genericDamageModifierComponent = entityManager.GetComponentData<GenericDamageModifierComponent>(player);
            genericDamageModifier = genericDamageModifierComponent.genericDamageModifierValue;
        }
        else
        {
            Debug.Log($"Cant find Generic Damage Modifier Component in RadiantFieldDamageSystem!");
        }

        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        double currentTime = (float)SystemAPI.Time.ElapsedTime;

        var job = new RadiantFieldDamageEnemyJob
        {
            radiantFieldLookup = SystemAPI.GetComponentLookup<RadiantFieldComponent>(true),
            enemyLookup = SystemAPI.GetComponentLookup<EnemyTagComponent>(true),
            ecb = ecb,
            currentTime = currentTime,
            genericDamageModifier = genericDamageModifier
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
    [ReadOnly] public double currentTime;
    [ReadOnly] public float genericDamageModifier;

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
                return;
            }

            // Skip if has not pass a tick
            if (currentTime - radiantFieldComponent.lastTickTime < radiantFieldComponent.timeBetween)
                return;

            // Deal damage
            RadiantFieldLevelData currerntLevelData = radiantFieldComponent.Data.Value.Levels[radiantFieldComponent.currentLevel];
            int baseDamage = currerntLevelData.damagePerTick;
            int finalDamage = (int)(baseDamage * (1 + genericDamageModifier));

            if (finalDamage > 0)
                ecb.AddComponent(enemyEntity, new DamageEventComponent { damageAmount = finalDamage });
        }
    }
}
