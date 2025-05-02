//using Unity.Collections;
//using Unity.Entities;
//using Unity.Mathematics;
//using Unity.Transforms;
//using UnityEngine;

//[UpdateInGroup(typeof(SimulationSystemGroup))]
//public partial struct SlimeBulletSpawnerSystem : ISystem
//{
//    private EntityManager entityManager;
//    private Entity player;

//    public void OnUpdate(ref SystemState state)
//    {
//        float deltaTime = SystemAPI.Time.DeltaTime;
//        var ecb = new EntityCommandBuffer(Allocator.Temp);
//        entityManager = state.EntityManager;

//        if (!SystemAPI.TryGetSingletonEntity<PlayerTagComponent>(out player))
//        {
//            Debug.Log($"Cant Found Player Entity in ShootSlimeBulletSystem!");
//            return;
//        }

//        foreach (var (shooter, entity) in SystemAPI.Query<RefRW<SlimeBulletShooterComponent>>().WithEntityAccess())
//        {
//            shooter.ValueRW.timer -= deltaTime;

//            if (shooter.ValueRW.timer <= 0 && shooter.ValueRW.bulletsRemaining > 0)
//            {
//                // Spawn the bullet
//                Entity bullet = BulletManager.Instance.Take(ecb);

//                Unity.Mathematics.Random random = new Unity.Mathematics.Random(12345);
//                float bonusDistance = random.NextFloat(shooter.ValueRO.minimumDistanceBetweenBullets, shooter.ValueRO.maximumDistanceBetweenBullets);

//                float distance = shooter.ValueRO.previousDistance + bonusDistance;

//                SetBulletStats(ecb, bullet, shooter.ValueRO.damage, shooter.ValueRO.passthroughDamageModifier, shooter.ValueRO.cooldown, 
//                    distance, shooter.ValueRO.moveSpeed, shooter.ValueRO.existDuration, shooter.ValueRO.slowModifier, shooter.ValueRO.slowRadius,
//                    shooter.ValueRO.isSlimeFrenzyActive, shooter.ValueRO.bonusDamagePercent);

//                ////Damages player
//                //if (entityManager.HasComponent<PlayerHealthComponent>(player))
//                //{
//                //    if (isSlimeFrenzyActive)
//                //    {
//                //        ecb.AddComponent(player, new DamageEventComponent
//                //        {
//                //            damageAmount = (int)(slimeBulletComponent.damagePlayerAmount * hpCostPerShotPercent),
//                //        });
//                //    }
//                //    else
//                //    {
//                //        ecb.AddComponent(player, new DamageEventComponent
//                //        {
//                //            damageAmount = slimeBulletComponent.damagePlayerAmount,
//                //        });
//                //    }
//                //}

//                shooter.ValueRW.bulletsRemaining--;
//                shooter.ValueRW.timer = shooter.ValueRW.delay;

//                shooter.ValueRW.previousDistance = distance;
                    
//                if (shooter.ValueRW.bulletsRemaining == 0)
//                {
//                    ecb.RemoveComponent<SlimeBulletShooterComponent>(entity);
//                }
//            }
//        }

//        ecb.Playback(state.EntityManager);
//        ecb.Dispose();
//    }

//    private void SetBulletStats(EntityCommandBuffer ecb, Entity bullet, int damage, float passthroughDamageModifier, float cooldown, float maxDistance, float moveSpeed,
//    float existDuration, float slowModifier, float slowRadius, bool isSlimeFrenzyActive, float bonusDamagePercent)
//    {
//        float3 playerPosition = entityManager.GetComponentData<LocalTransform>(player).Position;

//        ecb.SetComponent(bullet, new LocalTransform
//        {
//            Position = playerPosition,
//            Rotation = Quaternion.identity,
//            Scale = 1f
//        });

//        float3 mouseWorldPosition = MapManager.GetMouseWorldPosition();

//        float3 moveDirection = math.normalize(mouseWorldPosition - playerPosition);

//        if (!entityManager.HasComponent<SlimeBulletComponent>(bullet))
//        {
//            ecb.AddComponent(bullet, new SlimeBulletComponent
//            {
//                isAbleToMove = true,
//                isBeingSummoned = false,
//                moveDirection = moveDirection,
//                moveSpeed = moveSpeed,
//                distanceTraveled = 0,
//                maxDistance = maxDistance,
//                remainingDamage = damage,
//                passthroughDamageModifier = passthroughDamageModifier,
//                lastHitEnemy = Entity.Null,
//                healPlayerAmount = 0,
//                existDuration = existDuration,
//                hasHealPlayer = false,
//                slowModifier = slowModifier,
//                slowRadius = slowRadius,
//            });
//        }
//        else
//        {
//            ecb.SetComponent(bullet, new SlimeBulletComponent
//            {
//                isAbleToMove = true,
//                isBeingSummoned = false,
//                moveDirection = moveDirection,
//                moveSpeed = moveSpeed,
//                distanceTraveled = 0,
//                maxDistance = maxDistance,
//                remainingDamage = damage,
//                passthroughDamageModifier = passthroughDamageModifier,
//                lastHitEnemy = Entity.Null,
//                healPlayerAmount = 0,
//                existDuration = existDuration,
//                hasHealPlayer = false,
//                slowModifier = slowModifier,
//                slowRadius = slowRadius,
//            });
//        }
//    }
//}
