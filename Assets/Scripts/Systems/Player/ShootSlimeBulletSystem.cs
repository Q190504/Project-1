using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial struct ShootSlimeBulletSystem : ISystem
{
    private EntityManager entityManager;
    private Entity player;
    private float shootTimer;

    public void OnUpdate(ref SystemState state)
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        if (!SystemAPI.TryGetSingletonEntity<PlayerTagComponent>(out player))
        {
            Debug.Log($"Cant Found Player Entity in ShootSlimeBulletSystem!");
            return;
        }

        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        if (SystemAPI.TryGetSingleton<PlayerInputComponent>(out var playerInput)
            && SystemAPI.TryGetSingleton<ShootSlimeBulletComponent>(out var shootSlimeBulletComponent)
            && SystemAPI.TryGetSingleton<PlayerTagComponent>(out var playerTagComponent)
            && SystemAPI.TryGetSingleton<SlimeFrenzyComponent>(out var slimeFrenzyComponent))
        {
            if (playerInput.isShootingPressed && !playerTagComponent.isStunned && shootTimer <= 0)
            {
                Shoot(ecb, playerTagComponent.isFrenzing, slimeFrenzyComponent.bonusDamagePercent, slimeFrenzyComponent.hpCostPerShotPercent);

                if (playerTagComponent.isFrenzing)
                    shootTimer = shootSlimeBulletComponent.delayTime * (1 - slimeFrenzyComponent.fireRateReductionPercent);
                else
                    shootTimer = shootSlimeBulletComponent.delayTime;
            }
            else
            {
                shootTimer -= SystemAPI.Time.DeltaTime;
            }
        }

        ecb.Playback(entityManager);
        ecb.Dispose();
    }

    private void Shoot(EntityCommandBuffer ecb, bool isSlimeFrenzyActive, float bonusDamagePercent, float hpCostPerShotPercent)
    {
        Entity bullet = BulletManager.Instance.Take(ecb);
        SetBulletPositionAndDirection(ecb, bullet, isSlimeFrenzyActive, bonusDamagePercent, hpCostPerShotPercent);

        SlimeBulletComponent slimeBulletComponent = entityManager.GetComponentData<SlimeBulletComponent>(bullet);

        if (entityManager.HasComponent<PlayerHealthComponent>(player))
        {
            if (isSlimeFrenzyActive)
            {
                ecb.AddComponent(player, new DamageEventComponent
                {
                    damageAmount = (int)(slimeBulletComponent.damagePlayerAmount * hpCostPerShotPercent),
                });
            }
            else
            {
                ecb.AddComponent(player, new DamageEventComponent
                {
                    damageAmount = slimeBulletComponent.damagePlayerAmount,
                });
            }
        }
    }

    private void SetBulletPositionAndDirection(EntityCommandBuffer ecb, Entity bullet, bool isSlimeFrenzyActive, float bonusDamagePercent, float hpCostPerShotPercent)
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

        SlimeBulletComponent slimeBulletComponent = entityManager.GetComponentData<SlimeBulletComponent>(bullet);

        ecb.SetComponent(bullet, new SlimeBulletComponent
        {
            isAbleToMove = true,
            isBeingSummoned = false,
            moveDirection = moveDirection,
            moveSpeed = slimeBulletComponent.moveSpeed,
            distanceTraveled = 0,
            maxDistance = slimeBulletComponent.maxDistance,
            damageEnemyAmount = slimeBulletComponent.damageEnemyAmount,
            damagePlayerAmount = slimeBulletComponent.damagePlayerAmount,
            healPlayerAmount = slimeBulletComponent.healPlayerAmount,
            existDuration = slimeBulletComponent.existDuration,
            hasDamagedEnemy = false,
            hasHealPlayer = false,
        });
    }
}
