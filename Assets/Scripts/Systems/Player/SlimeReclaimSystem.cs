using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial struct SlimeReclaimSystem : ISystem
{
    private EntityManager entityManager;
    private Entity player;
    private float cooldownTimer;

    private bool hasWaitingSlimeBullet;

    public void OnUpdate(ref SystemState state)
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        if (!SystemAPI.TryGetSingletonEntity<PlayerTagComponent>(out player))
        {
            Debug.Log($"Cant Found Player Entity in SlimeReclaimSystem!");
            return;
        }

        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        if (SystemAPI.TryGetSingleton<PlayerInputComponent>(out var playerInput) && SystemAPI.TryGetSingleton<SlimeReclaimComponent>(out var slimeReclaimComponent))
        {
            PlayerHealthComponent playerHealthComponent = entityManager.GetComponentData<PlayerHealthComponent>(player);

            if (cooldownTimer <= 0)
            {
                GamePlayUIManager.Instance.SetSkill2CooldownUI(false);

                //Check any waiting slime bullet
                hasWaitingSlimeBullet = false;
                foreach (var (slimeBulletComponent, entity) in SystemAPI.Query<RefRO<SlimeBulletComponent>>().WithEntityAccess())
                {
                    if (!slimeBulletComponent.ValueRO.isAbleToMove)
                    {
                        hasWaitingSlimeBullet = true;
                        break;
                    }
                }

                if (CheckPlayerHealth(playerHealthComponent.currentHealth, playerHealthComponent.maxHealth)
                && hasWaitingSlimeBullet)
                {
                    //Update UI
                    GamePlayUIManager.Instance.SetSkill2ImageOpacity(true);

                    if (playerInput.isSpacePressed)
                    {
                        //// Apply stun effect
                        //if (!entityManager.HasComponent<StunTimerComponent>(player))
                        //    ecb.AddComponent(player, new StunTimerComponent
                        //    {
                        //        timeRemaining = slimeReclaimComponent.stunPlayerTime,
                        //        initialDuration = slimeReclaimComponent.stunPlayerTime,
                        //    });

                        // Activate skill effects
                        foreach (var slimeBulletComponent in SystemAPI.Query<RefRW<SlimeBulletComponent>>())
                        {
                            if (!slimeBulletComponent.ValueRO.isAbleToMove)
                                slimeBulletComponent.ValueRW.isBeingSummoned = true;
                        }

                        cooldownTimer = slimeReclaimComponent.cooldownTime;
                    }
                }
                else
                    GamePlayUIManager.Instance.SetSkill1ImageOpacity(false);
            }
            else
            {
                cooldownTimer -= SystemAPI.Time.DeltaTime;
                //update UI cooldown
                GamePlayUIManager.Instance.SetSkill2CooldownUI(true);
                GamePlayUIManager.Instance.SetSkill2ImageOpacity(false);
                GamePlayUIManager.Instance.UpdateSkill2CooldownUI(cooldownTimer, slimeReclaimComponent.cooldownTime);
            }
        }

        ecb.Playback(entityManager);
        ecb.Dispose();
    }

    private bool CheckPlayerHealth(int currentHealth, int maxHealth)
    {
        //if (maxHealth <= 0) return false;
        //if (currentHealth <= 0) return false;

        //return (float)currentHealth / maxHealth <= GameManager.Instance.SKILL_2_THRESHOLD;

        if (currentHealth <= 0) return false;
        else return true;
    }
}
