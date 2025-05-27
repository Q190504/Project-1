using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[UpdateAfter(typeof(GameInitializationSystem))]
public partial struct ShootSlimeBulletSystem : ISystem
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

        float deltaTime = SystemAPI.Time.DeltaTime;
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        if (!SystemAPI.TryGetSingleton<PlayerTagComponent>(out var playerTagComponent))
        {
            Debug.LogError("Cant find PlayerTagComponent in ShootSlimeBulletSystem");
            return;
        }

        if (!SystemAPI.TryGetSingletonEntity<PlayerTagComponent>(out player))
        {
            Debug.Log($"Cant Found Player Entity in ShootSlimeBulletSystem!");
            return;
        }

        // Get Frenzy data
        float bonusDamagePercent = 0;
        if (!SystemAPI.TryGetSingleton<SlimeFrenzyComponent>(out var slimeFrenzyComponent))
        {
            Debug.LogError("Cant find SlimeFrenzyComponent in ShootSlimeBulletSystem");
            return;
        }
        else
        {
            slimeFrenzyComponent = entityManager.GetComponentData<SlimeFrenzyComponent>(player);
            if (slimeFrenzyComponent.isActive)
                bonusDamagePercent = slimeFrenzyComponent.bonusDamagePercent;
        }

        // Get Ability Haste
        AbilityHasteComponent abilityHasteComponent;
        float abilityHaste = 0;
        if (SystemAPI.HasComponent<AbilityHasteComponent>(player))
        {
            abilityHasteComponent = entityManager.GetComponentData<AbilityHasteComponent>(player);
            abilityHaste = abilityHasteComponent.abilityHasteValue;
        }
        else
        {
            Debug.Log($"Cant Found Ability Haste Component in ShootSlimeBulletSystem!");
        }

        // Get Generic Damage Modifier
        GenericDamageModifierComponent genericDamageModifierComponent;
        float genericDamageModifier = 0;
        if (SystemAPI.HasComponent<GenericDamageModifierComponent>(player))
        {
            genericDamageModifierComponent = entityManager.GetComponentData<GenericDamageModifierComponent>(player);
            genericDamageModifier = genericDamageModifierComponent.genericDamageModifierValue;
        }
        else
        {
            Debug.Log($"Cant find Generic Damage Modifier Component in ShootSlimeBulletSystem!");
        }

        foreach (var (weaponComponent, weapon, shooterEntity) in SystemAPI.Query<RefRO<WeaponComponent>, RefRW<SlimeBulletShooterComponent>>().WithEntityAccess())
        {
            ref var shooter = ref weapon.ValueRW;

            shooter.timer -= deltaTime;
            if (shooter.timer > 0) continue;

            var blobData = shooter.Data;
            if (!blobData.IsCreated || blobData.Value.Levels.Length == 0) continue;

            // Determine weapon level index 
            int levelIndex = weaponComponent.ValueRO.Level;

            ref var levelData = ref blobData.Value.Levels[levelIndex];

            int baseDamage = levelData.damage;
            int finalDamage = (int)(baseDamage * (1 + genericDamageModifier + bonusDamagePercent));

            float baseCooldownTime = levelData.cooldown;
            float finalCooldownTime = baseCooldownTime * (100 / (100 + abilityHaste));

            int bulletCount = levelData.bulletCount;
            int bulletRemaining = bulletCount;
            float minimumDistance = levelData.minimumDistance;
            float minDistBetweenBullets = levelData.minimumDistanceBetweenBullets;
            float maxDistBetweenBullets = levelData.maximumDistanceBetweenBullets;
            float passthroughDamageModifier = levelData.passthroughDamageModifier;
            float moveSpeed = levelData.moveSpeed;
            float existDuration = levelData.existDuration;
            float slowModifier = levelData.slowModifier;
            float slowRadius = levelData.slowRadius;

            Shoot(ecb, shooterEntity, finalDamage, finalCooldownTime, bulletCount, bulletRemaining,
                minimumDistance, minDistBetweenBullets, maxDistBetweenBullets,
                passthroughDamageModifier, moveSpeed, existDuration,
                slowModifier, slowRadius);

            shooter.timer = finalCooldownTime; // Reset timer
        }
    }

    private void Shoot(
        EntityCommandBuffer ecb,
        Entity shooterEntity,
        int damage,
        float cooldown,
        int bulletCount,
        int bulletRemaining,
        float minimumDistance,
        float minDistBetweenBullets,
        float maxDistBetweenBullets,
        float passthroughDamageModifier,
        float moveSpeed,
        float existDuration,
        float slowModifier,
        float slowRadius)
    {

        for (int i = 0; i < bulletRemaining; i++)
        {
            // Spawn the bullet
            Entity bullet = ProjectilesManager.Instance.TakeSlimeBullet(ecb);

            float bonusDistance = (maxDistBetweenBullets - minDistBetweenBullets) / bulletRemaining;

            float distance = minimumDistance + i * bonusDistance;

            SetBulletStats(ecb, bullet, damage, passthroughDamageModifier, cooldown,
                distance, moveSpeed, existDuration, slowModifier, slowRadius);


            ////Damages player
            //if (entityManager.HasComponent<PlayerHealthComponent>(player))
            //{
            //    if (isSlimeFrenzyActive)
            //    {
            //        ecb.AddComponent(player, new DamageEventComponent
            //        {
            //            damageAmount = (int)(slimeBulletComponent.damagePlayerAmount * hpCostPerShotPercent),
            //        });
            //    }
            //    else
            //    {
            //        ecb.AddComponent(player, new DamageEventComponent
            //        {
            //            damageAmount = slimeBulletComponent.damagePlayerAmount,
            //        });
            //    }
            //}

            //shooter.ValueRW.bulletsRemaining--;
            //shooter.ValueRW.timer = shooter.ValueRW.delay;

            //shooter.ValueRW.previousDistance = distance;

            //if (shooter.ValueRW.bulletsRemaining == 0)
            //{
            //    ecb.RemoveComponent<SlimeBulletShooterComponent>(entity);
            //}
        }
    }

    private void SetBulletStats(EntityCommandBuffer ecb, Entity bullet, int damage, float passthroughDamageModifier,
        float cooldown, float maxDistance, float moveSpeed, float existDuration, float slowModifier,
        float slowRadius)
    {
        float3 playerPosition = entityManager.GetComponentData<LocalTransform>(player).Position;

        ecb.SetComponent(bullet, new LocalTransform
        {
            Position = playerPosition,
            Rotation = Quaternion.identity,
            Scale = 1f
        });

        float3 mouseWorldPosition = MapManager.GetMouseWorldPosition();

        float3 moveDirection = math.normalize(mouseWorldPosition - playerPosition);

        if (!entityManager.HasComponent<SlimeBulletComponent>(bullet))
        {
            ecb.AddComponent(bullet, new SlimeBulletComponent
            {
                isAbleToMove = true,
                isBeingSummoned = false,
                moveDirection = moveDirection,
                moveSpeed = moveSpeed,
                distanceTraveled = 0,
                maxDistance = maxDistance,
                remainingDamage = damage,
                passthroughDamageModifier = passthroughDamageModifier,
                lastHitEnemy = Entity.Null,
                healPlayerAmount = 0,
                existDuration = existDuration,
                hasHealPlayer = false,
                slowModifier = slowModifier,
                slowRadius = slowRadius,
            });
        }
        else
        {
            ecb.SetComponent(bullet, new SlimeBulletComponent
            {
                isAbleToMove = true,
                isBeingSummoned = false,
                moveDirection = moveDirection,
                moveSpeed = moveSpeed,
                distanceTraveled = 0,
                maxDistance = maxDistance,
                remainingDamage = damage,
                passthroughDamageModifier = passthroughDamageModifier,
                lastHitEnemy = Entity.Null,
                healPlayerAmount = 0,
                existDuration = existDuration,
                hasHealPlayer = false,
                slowModifier = slowModifier,
                slowRadius = slowRadius,
            });
        }
    }
}
