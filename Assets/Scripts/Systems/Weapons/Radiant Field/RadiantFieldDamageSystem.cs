using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Collections;
using UnityEngine;
using Unity.Transforms;
using Unity.Mathematics;

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

        if (!SystemAPI.TryGetSingletonEntity<RadiantFieldComponent>(out var radiantFieldComponent))
        {
            Debug.Log($"Cant Found Radiant Field Component Entity in RadiantFieldDamageSystem!");
            return;
        }

        WeaponComponent weaponComponent = SystemAPI.GetComponent<WeaponComponent>(radiantFieldComponent);
        int currentLevel = weaponComponent.Level;
        if (currentLevel == 0)
            return;

        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

        foreach (var (radiantField, transform, entity) in SystemAPI.Query<RefRW<RadiantFieldComponent>, RefRW<LocalTransform>>().WithEntityAccess())
        {
            if (radiantField.ValueRO.timer <= 0)
            {
                NativeList<DistanceHit> hits = new NativeList<DistanceHit>(Allocator.Temp);

                CollisionFilter filter = new CollisionFilter
                {
                    BelongsTo = 1 << 6,
                    CollidesWith = 1 << 3,
                    GroupIndex = 0
                };

                var blobData = radiantField.ValueRO.Data;
                if (!blobData.IsCreated || blobData.Value.Levels.Length == 0) continue;
                ref var levelData = ref blobData.Value.Levels[currentLevel];
                float radius = levelData.radius;

                physicsWorldSingleton.OverlapSphere(transform.ValueRO.Position, radius / 2,
                ref hits, filter);

                //DebugDrawSphere(transform.ValueRO.Position, radius / 2, Color.yellow);

                int damage = levelData.damagePerTick;

                if (damage > 0)
                {
                    foreach (var enemy in hits)
                    {
                        // Check if the hit entity is an enemy
                        if (!SystemAPI.HasComponent<EnemyTagComponent>(enemy.Entity))
                            continue;

                        ecb.AddComponent(enemy.Entity, new DamageEventComponent { damageAmount = damage });
                    }
                }

                hits.Dispose();

                radiantField.ValueRW.timer = radiantField.ValueRO.timeBetween;
            }
            else
            {
                radiantField.ValueRW.timer -= Time.deltaTime;
            }
        }


        //var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        //var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        //double currentTime = (float)SystemAPI.Time.ElapsedTime;

        //var job = new RadiantFieldDamageEnemyJob
        //{
        //    weaponComponentLookup = SystemAPI.GetComponentLookup<WeaponComponent>(true),
        //    radiantFieldLookup = SystemAPI.GetComponentLookup<RadiantFieldComponent>(true),
        //    enemyLookup = SystemAPI.GetComponentLookup<EnemyTagComponent>(true),
        //    ecb = ecb,
        //    currentTime = currentTime,
        //    genericDamageModifier = genericDamageModifier,
        //    bonusDamagePercent = bonusDamagePercent
        //};

        //state.Dependency = job.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
    }

    void DebugDrawSphere(float3 center, float radius, Color color)
    {
        int segments = 16;
        for (int i = 0; i < segments; i++)
        {
            float angle1 = (i / (float)segments) * math.PI * 2;
            float angle2 = ((i + 1) / (float)segments) * math.PI * 2;

            float3 p1 = center + new float3(math.cos(angle1), math.sin(angle1), 0) * radius;
            float3 p2 = center + new float3(math.cos(angle2), math.sin(angle2), 0) * radius;

            Debug.DrawLine(p1, p2, color, 0.1f);
        }
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
