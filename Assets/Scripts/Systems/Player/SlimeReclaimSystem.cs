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
        if (!GameManager.Instance.IsPlaying()) return;

        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        if (!SystemAPI.TryGetSingletonEntity<PlayerTagComponent>(out player))
        {
            Debug.Log($"Cant Found Player Entity in SlimeReclaimSystem!");
            return;
        }

        float abilityHaste = 0;
        if (SystemAPI.TryGetSingleton<AbilityHasteComponent>(out AbilityHasteComponent abilityHasteComponent))
        {
            abilityHaste = abilityHasteComponent.abilityHasteValue;
        }
        else
        {
            Debug.Log($"Cant Found Ability Haste Component in SlimeReclaimSystem!");
        }

        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        if (SystemAPI.TryGetSingleton<PlayerInputComponent>(out var playerInput)
            && SystemAPI.TryGetSingleton<SlimeReclaimComponent>(out var slimeReclaimComponent))
        {
            PlayerHealthComponent playerHealthComponent = entityManager.GetComponentData<PlayerHealthComponent>(player);

            float baseCooldownTime = slimeReclaimComponent.cooldownTime;
            float finalCooldownTime = baseCooldownTime * (100 / (100 + abilityHaste));

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

                    if (playerInput.isRPressed)
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

                        cooldownTimer = finalCooldownTime;
                    }
                }
                else
                    GamePlayUIManager.Instance.SetSkill2ImageOpacity(false);
            }
            else
            {
                cooldownTimer -= SystemAPI.Time.DeltaTime;
                //update UI cooldown
                GamePlayUIManager.Instance.SetSkill2CooldownUI(true);
                GamePlayUIManager.Instance.SetSkill2ImageOpacity(false);
                GamePlayUIManager.Instance.UpdateSkill2CooldownUI(cooldownTimer, finalCooldownTime);
            }
        }
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
