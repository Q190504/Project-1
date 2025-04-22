using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial struct ShootSlimeBulletSystem : ISystem
{
    private EntityQuery weaponDatabaseQuery;

    public void OnCreate(ref SystemState state)
    {
        weaponDatabaseQuery = state.GetEntityQuery(ComponentType.ReadOnly<WeaponDatabaseComponent>());
    }

    public void OnUpdate(ref SystemState state)
    {
        if (weaponDatabaseQuery.IsEmpty) return;

        var weaponDatabaseComponent = SystemAPI.GetSingleton<WeaponDatabaseComponent>();
        ref var weaponDatabase = ref weaponDatabaseComponent.weaponDatabase.Value;
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        float deltaTime = SystemAPI.Time.DeltaTime;

        Entity activeWeaponEntity = Entity.Null;
        WeaponComponent activeWeapon = default;
        bool foundActiveWeapon = false;


        foreach (var (weapon, entity) in SystemAPI.Query<WeaponComponent>().WithEntityAccess())
        {
            if (weapon.type != WeaponType.SlimeBullet)
                continue;

            if (weapon.currentLevel > 0)
            {
                activeWeaponEntity = entity;
                activeWeapon = weapon;
                foundActiveWeapon = true;
                break; // Stop after finding the first active slime bullet weapon
            }
        }


        if (foundActiveWeapon)
        {
            if (SystemAPI.HasComponent<CooldownComponent>(activeWeaponEntity))
            {
                ref var cooldownComponent = ref SystemAPI.GetComponentRW<CooldownComponent>(activeWeaponEntity).ValueRW;
                cooldownComponent.remainingTime -= deltaTime;

                if (cooldownComponent.remainingTime > 0)
                    return;
            }

            if (SystemAPI.TryGetSingleton<PlayerTagComponent>(out var playerTagComponent)
                && SystemAPI.TryGetSingleton<SlimeFrenzyComponent>(out var slimeFrenzyComponent))
            {
                if (!playerTagComponent.isStunned)
                {
                    GetStats(ref weaponDatabase, WeaponType.SlimeBullet, activeWeapon.currentLevel, out int damage,
                        out float cooldown, out int bulletCount, out float distance, out float minimumDistanceBetweenBullets,
                        out float maximumDistanceBetweenBullets, out float passthroughDamageModifier, out float moveSpeed,
                        out float existDuration);

                    Shoot(ecb, activeWeaponEntity, damage, cooldown, bulletCount, distance, minimumDistanceBetweenBullets,
                        maximumDistanceBetweenBullets, passthroughDamageModifier, moveSpeed, existDuration,
                        playerTagComponent.isFrenzing, slimeFrenzyComponent.bonusDamagePercent);

                    float shootTimer = playerTagComponent.isFrenzing
                        ? cooldown * (1 - slimeFrenzyComponent.fireRateReductionPercent)
                        : cooldown;

                    if (!SystemAPI.HasComponent<CooldownComponent>(activeWeaponEntity))
                    {
                        ecb.AddComponent(activeWeaponEntity, new CooldownComponent { remainingTime = shootTimer });

                    }
                    else
                    {
                        ecb.SetComponent(activeWeaponEntity, new CooldownComponent { remainingTime = shootTimer });
                    }
                }
            }
        }

        //ecb.Playback(entityManager);
        //ecb.Dispose();
    }

    private void Shoot(EntityCommandBuffer ecb, Entity shooterEntity, int damage, float cooldown, int bulletCount, float distance,
        float minimumDistanceBetweenBullets, float maximumDistanceBetweenBullets, float passthroughDamageModifier, float moveSpeed,
        float existDuration, bool isSlimeFrenzyActive, float bonusDamagePercent)
    {
        float delayBetweenShot = 0.1f;

        SlimeBulletShooterComponent shot = new SlimeBulletShooterComponent
        {
            delay = delayBetweenShot,         // delay between bullets
            timer = 0,
            //public EntityCommandBuffer ecb,
            damage = damage,
            previousDamage = damage,
            cooldown = cooldown,
            bulletCount = bulletCount,
            bulletsRemaining = bulletCount,
            minimumDistance = distance,
            minimumDistanceBetweenBullets = minimumDistanceBetweenBullets,
            maximumDistanceBetweenBullets = maximumDistanceBetweenBullets,
            previousDistance = distance,
            passthroughDamageModifier = passthroughDamageModifier,
            moveSpeed = moveSpeed,
            existDuration = existDuration,
            isSlimeFrenzyActive = isSlimeFrenzyActive,
            bonusDamagePercent = bonusDamagePercent,
        };

        ecb.AddComponent(shooterEntity, shot);
    }

    private void GetStats(ref WeaponDatabase weaponDatabase, WeaponType type, int level, out int damage,
        out float cooldown, out int bulletCount, out float distance, out float minimumDistanceBetweenBullets,
        out float maximumDistanceBetweenBullets, out float passthroughDamageModifier, out float moveSpeed, out float existDuration)
    {
        damage = 0;
        cooldown = 0;
        bulletCount = 0;
        minimumDistanceBetweenBullets = 0;
        maximumDistanceBetweenBullets = 0;
        passthroughDamageModifier = 0;
        moveSpeed = 0;
        distance = 0;
        existDuration = 0;

        for (int i = 0; i < weaponDatabase.weapons.Length; i++)
        {
            if (weaponDatabase.weapons[i].type == type)
            {
                int levelIndex = math.clamp(level, 0, weaponDatabase.weapons[i].levels.Length - 1);

                damage = weaponDatabase.weapons[i].levels[levelIndex].damage;
                cooldown = weaponDatabase.weapons[i].levels[levelIndex].cooldownTime;
                bulletCount = weaponDatabase.weapons[i].levels[levelIndex].bulletCount;
                minimumDistanceBetweenBullets = weaponDatabase.weapons[i].levels[levelIndex].minimumDistanceBetweenBullets;
                maximumDistanceBetweenBullets = weaponDatabase.weapons[i].levels[levelIndex].maximumDistanceBetweenBullets;
                passthroughDamageModifier = weaponDatabase.weapons[i].levels[levelIndex].passthroughDamageModifier;
                moveSpeed = weaponDatabase.weapons[i].levels[levelIndex].moveSpeed;
                distance = weaponDatabase.weapons[i].levels[levelIndex].distance;
                existDuration = weaponDatabase.weapons[i].levels[levelIndex].existDuration;

                return;
            }
        }
    }
}
