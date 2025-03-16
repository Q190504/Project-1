using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

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
            Debug.Log($"Cant Found Player Entity!");
            return;
        }

        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        if (SystemAPI.TryGetSingleton<PlayerInputComponent>(out var playerInput) && SystemAPI.TryGetSingleton<ShootSlimeBulletComponent>(out var shootSlimeBulletComponent))
        {
            if (playerInput.isShootingPressed && shootTimer <= 0)
            {
                Shoot(ecb);
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

    private void Shoot(EntityCommandBuffer ecb)
    {
        Entity bullet = BulletManager.Instance.Take(ecb);
        SetBulletPositionAndDirection(ecb, bullet);

        SlimeBulletComponent slimeBulletComponent = entityManager.GetComponentData<SlimeBulletComponent>(bullet);

        if (entityManager.HasComponent<PlayerHealthComponent>(player))
        {
            ecb.AddComponent(player, new DamageEventComponent
            {
                damageAmount = slimeBulletComponent.damagePlayerAmount,
            });
        }
    }

    private void SetBulletPositionAndDirection(EntityCommandBuffer ecb, Entity bullet)
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
            colliderSize = slimeBulletComponent.colliderSize,
            existDuration = slimeBulletComponent.existDuration,
        });
    }
}
