using System.Linq;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public partial struct SlimeFrenzySystem : ISystem
{
    private EntityManager entityManager;
    private Entity player;
    private PlayerInputComponent playerInput;
    private SlimeFrenzyComponent slimeFrenzy;
    private float cooldownTimer;
    private bool isFrenzyActive;

    public void OnUpdate(ref SystemState state)
    {
        if (!GameManager.Instance.IsPlaying()) return;

        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        if (!SystemAPI.TryGetSingletonEntity<PlayerTagComponent>(out player))
        {
            Debug.Log($"Cant Found Player Entity in SlimeFrenzySystem!");
            return;
        }

        float abilityHaste = 0;
        if (SystemAPI.TryGetSingleton<AbilityHasteComponent>(out AbilityHasteComponent abilityHasteComponent))
        {
            abilityHaste = abilityHasteComponent.abilityHasteValue;
        }
        else
        {
            Debug.Log($"Cant Found Ability Haste Component in SlimeFrenzySystem!");
        }

        if (!SystemAPI.HasComponent<PlayerInputComponent>(player))
        {
            Debug.Log($"Cant Player Input Component in SlimeFrenzySystem!");
            return;
        }
        else
        {
            playerInput = SystemAPI.GetComponent<PlayerInputComponent>(player);
        }

        if (!SystemAPI.HasComponent<SlimeFrenzyComponent>(player))
        {
            Debug.Log($"Cant Slime Frenzy Component in SlimeFrenzySystem!");
            return;
        }
        else
        {
            slimeFrenzy = SystemAPI.GetComponent<SlimeFrenzyComponent>(player);
        }

        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);


        PlayerHealthComponent playerHealthComponent = entityManager.GetComponentData<PlayerHealthComponent>(player);

        float baseCooldownTime = slimeFrenzy.cooldownTime;
        float finalCooldownTime = baseCooldownTime * (100 / (100 + abilityHaste));

        if (cooldownTimer <= 0)
        {
            GamePlayUIManager.Instance.SetSkill1CooldownUI(false);

            if (!isFrenzyActive)
                GamePlayUIManager.Instance.SetSkill1ImageOpacity(true);
            else
                GamePlayUIManager.Instance.SetSkill1ImageOpacity(false);

            if (playerInput.isEPressed
            && CheckPlayerHealth(playerHealthComponent.currentHealth, playerHealthComponent.maxHealth))
            {
                // Apply frenzy effect
                if (!entityManager.HasComponent<SlimeFrenzyTimerComponent>(player))
                    ecb.AddComponent(player, new SlimeFrenzyTimerComponent
                    {
                        timeRemaining = slimeFrenzy.duration,
                        initialDuration = slimeFrenzy.duration
                    });

                isFrenzyActive = true;
            }
            else if (!entityManager.HasComponent<SlimeFrenzyTimerComponent>(player)
                && isFrenzyActive) //Just ended slime frenzy
            {
                // Frenzy effect ended, start cooldown
                cooldownTimer = finalCooldownTime;
                isFrenzyActive = false;
            }
        }
        else
        {
            cooldownTimer -= SystemAPI.Time.DeltaTime;

            //update UI cooldown
            GamePlayUIManager.Instance.SetSkill1CooldownUI(true);
            GamePlayUIManager.Instance.SetSkill1ImageOpacity(false);
            GamePlayUIManager.Instance.UpdateSkill1CooldownUI(cooldownTimer, finalCooldownTime);
        }
    }

    private bool CheckPlayerHealth(int currentHealth, int maxHealth)
    {
        //if (maxHealth <= 0) return false;
        //if (currentHealth <= 0) return false;

        //bool isAboveSkill2Threshold = (float)currentHealth / maxHealth > GameManager.Instance.SKILL_2_THRESHOLD;
        //bool isSmallerOrEqualSkill1Threshold = (float)currentHealth / maxHealth <= GameManager.Instance.SKILL_1_THRESHOLD;

        //return isSmallerOrEqualSkill1Threshold && isAboveSkill2Threshold;

        if (currentHealth <= 0) return false;
        else return true;
    }
}
