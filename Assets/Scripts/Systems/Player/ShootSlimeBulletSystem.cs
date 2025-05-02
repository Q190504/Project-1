using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
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
        float deltaTime = SystemAPI.Time.DeltaTime;
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        if(!SystemAPI.TryGetSingleton<PlayerTagComponent>(out var playerTagComponent))
        {
            Debug.LogError("Cant find PlayerTagComponent in ShootSlimeBulletSystem");
            return;
        }

        if (!SystemAPI.TryGetSingletonEntity<PlayerTagComponent>(out player))
        {
            Debug.Log($"Cant Found Player Entity in ShootSlimeBulletSystem!");
            return;
        }

        if (!SystemAPI.TryGetSingleton<SlimeFrenzyComponent>(out var slimeFrenzyComponent))
        {
            Debug.LogError("Cant find SlimeFrenzyComponent in ShootSlimeBulletSystem");
            return;
        }
        
        foreach (var (weapon, shooterEntity) in SystemAPI.Query<RefRW<SlimeBulletShooterComponent>>().WithEntityAccess())
        {
            ref var shooter = ref weapon.ValueRW;

            shooter.timer -= deltaTime;
            if (shooter.timer > 0) continue;

            var blobData = shooter.Data;
            if (!blobData.IsCreated || blobData.Value.Levels.Length == 0) continue;

            // Determine weapon level index (can be from another component if you support dynamic leveling)
            int levelIndex = 1;
            ref var levelData = ref blobData.Value.Levels[levelIndex];

            int damage = levelData.damage;
            float cooldown = levelData.cooldown;
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

            Shoot(
                ecb, shooterEntity, damage, cooldown, bulletCount, bulletRemaining,
                minimumDistance, minDistBetweenBullets, maxDistBetweenBullets,
                passthroughDamageModifier, moveSpeed, existDuration,
                slowModifier, slowRadius, playerTagComponent.isFrenzing,
                slimeFrenzyComponent.bonusDamagePercent
            );

            shooter.timer = cooldown; // Reset timer
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
        float slowRadius,
        bool isSlimeFrenzyActive,
        float bonusDamagePercent)
    {

        for (int i = 0; i < bulletRemaining; i++ )
        {
            // Spawn the bullet
            Entity bullet = BulletManager.Instance.Take(ecb);

            float bonusDistance = (maxDistBetweenBullets - minDistBetweenBullets) / bulletRemaining;

            float distance = minimumDistance + i * bonusDistance;

            SetBulletStats(ecb, bullet, damage, passthroughDamageModifier, cooldown,
                distance, moveSpeed, existDuration, slowModifier, slowRadius,
                isSlimeFrenzyActive, bonusDamagePercent);


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

    private void SetBulletStats(EntityCommandBuffer ecb, Entity bullet, int damage, float passthroughDamageModifier, float cooldown, float maxDistance, float moveSpeed,
float existDuration, float slowModifier, float slowRadius, bool isSlimeFrenzyActive, float bonusDamagePercent)
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
