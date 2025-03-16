using System.Linq;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public partial struct SlimeFrenzySystem : ISystem
{
    private EntityManager entityManager;
    private Entity player;
    private float cooldownTimer;
    private bool isCooldownActive;

    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        if (!SystemAPI.TryGetSingletonEntity<PlayerTagComponent>(out player))
        {
            Debug.Log($"Cant Found Player Entity in SlimeFrenzySystem!");
            return;
        }

        if (SystemAPI.TryGetSingleton<PlayerInputComponent>(out var playerInput) && SystemAPI.TryGetSingleton<SlimeFrenzyComponent>(out var slimeFrenzyComponent) && SystemAPI.TryGetSingleton<PlayerTagComponent>(out var playerTagComponent))
        {
            PlayerHealthComponent playerHealthComponent = entityManager.GetComponentData<PlayerHealthComponent>(player);
            var playerTagComponentRef = SystemAPI.GetSingletonRW<PlayerTagComponent>();

            if (playerInput.isSkillPressed
                && CheckPlayerHealth(playerHealthComponent.currentHealth, playerHealthComponent.maxHealth)
                && cooldownTimer <= 0)
            {
                // Apply frenzy effect
                if (!entityManager.HasComponent<SlimeFrenzyTimerComponent>(player))
                    ecb.AddComponent(player, new SlimeFrenzyTimerComponent { timeRemaining = slimeFrenzyComponent.duration });
                isCooldownActive = false;
            }
            else if (!entityManager.HasComponent<SlimeFrenzyTimerComponent>(player) && cooldownTimer < 0 && !isCooldownActive)
            {
                // Frenzy effect ended, start cooldown
                cooldownTimer = slimeFrenzyComponent.cooldownTime;
                isCooldownActive = true;
            }
            else
                cooldownTimer -= SystemAPI.Time.DeltaTime;
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
    private bool CheckPlayerHealth(int currentHealth, int maxHealth)
    {
        if (maxHealth <= 0) return false;
        if (currentHealth <= 0) return false;

        bool isAboveSkill2Threshold = (float)currentHealth / maxHealth > GameManager.Instance.SKILL_2_THRESHOLD;
        bool isSmallerOrEqualSkill1Threshold = (float)currentHealth / maxHealth <= GameManager.Instance.SKILL_1_THRESHOLD;

        return isSmallerOrEqualSkill1Threshold && isAboveSkill2Threshold;
    }
}
