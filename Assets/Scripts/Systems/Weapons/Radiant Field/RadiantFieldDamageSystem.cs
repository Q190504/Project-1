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

        // Get Generic Damage Modifier
        float genericDamageModifier = 0;
        if (SystemAPI.TryGetSingleton<GenericDamageModifierComponent>(out GenericDamageModifierComponent genericDamageModifierComponent))
        {
            genericDamageModifier = genericDamageModifierComponent.genericDamageModifierValue;
        }
        else
        {
            Debug.Log($"Cant find Generic Damage Modifier Component in RadiantFieldDamageSystem!");
        }

        // Get Frenzy data
        SlimeFrenzyComponent slimeFrenzyComponent;
        float bonusDamagePercent = 0;
        if (SystemAPI.HasComponent<SlimeFrenzyComponent>(player))
        {
            slimeFrenzyComponent = entityManager.GetComponentData<SlimeFrenzyComponent>(player);
            if (slimeFrenzyComponent.isActive)
                bonusDamagePercent = slimeFrenzyComponent.bonusDamagePercent;
        }
        else
        {
            Debug.Log($"Cant find Slime Frenzy Component in RadiantFieldDamageSystem!");
        }

        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        double currentTime = (float)SystemAPI.Time.ElapsedTime;

        var job = new RadiantFieldDamageEnemyJob
        {
            weaponComponentLookup = SystemAPI.GetComponentLookup<WeaponComponent>(true),
            radiantFieldLookup = SystemAPI.GetComponentLookup<RadiantFieldComponent>(true),
            enemyLookup = SystemAPI.GetComponentLookup<EnemyTagComponent>(true),
            ecb = ecb,
            currentTime = currentTime,
            genericDamageModifier = genericDamageModifier,
            bonusDamagePercent = bonusDamagePercent
        };

        state.Dependency = job.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
    }
}

[BurstCompile]
struct RadiantFieldDamageEnemyJob : ITriggerEventsJob
{
    [ReadOnly] public ComponentLookup<WeaponComponent> weaponComponentLookup;
    [ReadOnly] public ComponentLookup<RadiantFieldComponent> radiantFieldLookup;
    [ReadOnly] public ComponentLookup<EnemyTagComponent> enemyLookup;
    public EntityCommandBuffer ecb;
    [ReadOnly] public double currentTime;
    [ReadOnly] public float genericDamageModifier;
    [ReadOnly] public float bonusDamagePercent;

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

            if (!radiantFieldLookup.HasComponent(radiantFieldEntity) || !weaponComponentLookup.HasComponent(radiantFieldEntity))
            {
                return;
            }

            var radiantFieldComponent = radiantFieldLookup[radiantFieldEntity];
            var weaponComponent = weaponComponentLookup[radiantFieldEntity];

            if (weaponComponent.Level <= 0) // is inactive
            {
                return;
            }

            // Skip if has not pass a tick
            if (currentTime - radiantFieldComponent.lastTickTime < radiantFieldComponent.timeBetween)
                return;

            // Calculate damage
            RadiantFieldLevelData currerntLevelData = radiantFieldComponent.Data.Value.Levels[weaponComponent.Level];
            int baseDamage = currerntLevelData.damagePerTick;
            int finalDamage = (int)(baseDamage * (1 + genericDamageModifier + bonusDamagePercent));

            // Deal damage
            if (finalDamage > 0)
                ecb.AddComponent(enemyEntity, new DamageEventComponent { damageAmount = finalDamage });
        }
    }
}
