using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Physics;
using Unity.Burst;
using Unity.Transforms;

[BurstCompile]
[UpdateAfter(typeof(GameInitializationSystem))]
public partial struct SlimeBeamShooterSystem : ISystem
{
    private EntityManager entityManager;
    private Entity player;

    public void OnCreate(ref SystemState state)
    {
        entityManager = state.EntityManager;
    }

    public void OnUpdate(ref SystemState state)
    {
        if (!GameManager.Instance.IsPlaying()) return;

        if (!SystemAPI.TryGetSingletonEntity<PlayerTagComponent>(out player))
        {
            Debug.Log($"Cant Found Player Entity in SlimeBeamShooterSystem!");
            return;
        }

        // Get Ability Haste
        float abilityHaste = 0;
        if (SystemAPI.TryGetSingleton<AbilityHasteComponent>(out AbilityHasteComponent abilityHasteComponent))
        {
            abilityHaste = abilityHasteComponent.abilityHasteValue;
        }
        else
        {
            Debug.Log($"Cant Found Ability Haste Component in SlimeBeamShooterSystem!");
        }

        // Get Generic Damage Modifier
        float genericDamageModifier = 0;
        if (SystemAPI.TryGetSingleton<GenericDamageModifierComponent>(out GenericDamageModifierComponent genericDamageModifierComponent))
        {
            genericDamageModifier = genericDamageModifierComponent.genericDamageModifierValue;
        }
        else
        {
            Debug.Log($"Cant find Generic Damage Modifier Component in SlimeBeamShooterSystem!");
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
            Debug.Log($"Cant find Slime Frenzy Component in SlimeBeamShooterSystem!");
        }

        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        float deltaTime = SystemAPI.Time.DeltaTime;

        foreach (var (weaponComponent, SlimeBeamShooter, entity) 
            in SystemAPI.Query<RefRO<WeaponComponent>, RefRW<SlimeBeamShooterComponent>>().WithEntityAccess())
        {
            ref var beamShooter = ref SlimeBeamShooter.ValueRW;
            beamShooter.timer -= deltaTime;
            if (beamShooter.timer > 0) continue;

            var blobData = beamShooter.Data;
            if (!blobData.IsCreated || blobData.Value.Levels.Length == 0) continue;
            
            // Determine weapon level
            int level = weaponComponent.ValueRO.Level;

            if (level <= 0) // is active
            {
                return;
            }

            ref var levelData = ref blobData.Value.Levels[level];

            int baseDamage = levelData.damage;
            int finalDamage = (int)(baseDamage * (1 + genericDamageModifier + bonusDamagePercent));

            float baseCooldownTime = levelData.cooldown;
            float finalCooldownTime = baseCooldownTime * (100 / (100 + abilityHaste));

            float timeBetween = levelData.timeBetween;
            float spawnOffsetPositon = beamShooter.spawnOffsetPositon;

            if(level == 5) //max level
            {
                for (int beamCount = 0; beamCount < 4; beamCount++)
                    PerformSingleBeam(entity, spawnOffsetPositon, finalDamage, beamCount, ecb);

                beamShooter.timer = finalCooldownTime; // Reset timer
            }
            else
            {
                beamShooter.timeBetween += deltaTime;

                if (beamShooter.timeBetween >= timeBetween && beamShooter.beamCount < 4)
                {
                    PerformSingleBeam(entity, spawnOffsetPositon, finalDamage, beamShooter.beamCount, ecb);

                    beamShooter.beamCount++;
                    beamShooter.timeBetween = 0f;
                }
                else if (beamShooter.beamCount >= 4)
                {
                    beamShooter.beamCount = 0;
                    beamShooter.timer = finalCooldownTime; // Reset timer
                }
            }
        }
    }

    private void PerformSingleBeam(Entity entity, float spawnOffsetPositon, int damage, int beamCount, EntityCommandBuffer ecb)
    {
        PhysicsWorldSingleton physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        NativeList<Unity.Physics.RaycastHit> hits = new NativeList<Unity.Physics.RaycastHit>(Allocator.Temp);

        float3 playerPosition = entityManager.GetComponentData<LocalTransform>(player).Position;
        float3 position = playerPosition + GetAttackDirection(spawnOffsetPositon, beamCount);
        Quaternion rotation = GetRotation(beamCount);

        //spawn beam
        Entity slimeBeamInstance = ProjectilesManager.Instance.TakeSlimeBeam(ecb);
        SetStats(ecb, slimeBeamInstance, damage, position, rotation);

        hits.Dispose();
    }

    private void SetStats(EntityCommandBuffer ecb, Entity beam, int damage, float3 originPosition, quaternion rotation)
    {
        ecb.SetComponent(beam, new LocalTransform
        {
            Position = originPosition,
            Rotation = rotation,
            Scale = 1f
        });

        ecb.AddComponent(beam, new SlimeBeamComponent
        {
            damage = damage,
            hasDealDamageToEnemies = false,
            timer = 0.1f,
        });
    }

    private float3 GetAttackDirection(float spawnOffsetPositon, int count)
    {
        switch (count % 4)
        {
            case 0: return new float3(0, spawnOffsetPositon, 0);  // Top 
            case 1: return new float3(spawnOffsetPositon, 0, 0);  // Right 
            case 2: return new float3(0, -spawnOffsetPositon, 0); // Bottom
            case 3: return new float3(-spawnOffsetPositon, 0, 0); // Left 
            default: return float3.zero;
        }
    }

    private Quaternion GetRotation(int count)
    {
        switch (count % 4)
        {
            case 0: return Quaternion.identity;         // Top 0 degrees
            case 1: return Quaternion.Euler(0, 0, 270); // Right 270 degrees
            case 2: return Quaternion.Euler(0, 0, 180); // Bottom 180 degrees
            case 3: return Quaternion.Euler(0, 0, 90);  // Left 90 degrees
            default: return Quaternion.identity;
        }
    }
}
