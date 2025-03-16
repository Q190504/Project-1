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

    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        if (!SystemAPI.TryGetSingletonEntity<PlayerTagComponent>(out player))
        {
            Debug.Log($"Cant Found Player Entity in SlimeBulletMoverSystem!");
            return;
        }

        if (SystemAPI.TryGetSingleton<PlayerInputComponent>(out var playerInput) && SystemAPI.TryGetSingleton<SlimeReclaimComponent>(out var slimeReclaimComponent))
        {
            PlayerHealthComponent playerHealthComponent = entityManager.GetComponentData<PlayerHealthComponent>(player);

            if (playerInput.isSkillPressed
                && CheckPlayerHealth(playerHealthComponent.currentHealth, playerHealthComponent.maxHealth)
                && cooldownTimer <= 0
                && SystemAPI.Query<RefRW<SlimeBulletComponent>>().Any())
            {
                // Apply stun effect
                if (!entityManager.HasComponent<StunTimerComponent>(player))
                    ecb.AddComponent(player, new StunTimerComponent { timeRemaining = slimeReclaimComponent.stunPlayerTime });

                // Activate skill effects
                foreach (var slimeBulletComponent in SystemAPI.Query<RefRW<SlimeBulletComponent>>())
                    slimeBulletComponent.ValueRW.isBeingSummoned = true;

                cooldownTimer = slimeReclaimComponent.cooldownTime;
            }
            else
            {
                cooldownTimer -= SystemAPI.Time.DeltaTime;
            }
        }

        ecb.Playback(entityManager);
        ecb.Dispose();
    }

    private bool CheckPlayerHealth(int currentHealth, int maxHealth)
    {
        return (float)currentHealth / maxHealth <= GameManager.SKILL_2_THRESHOLD;
    }
}
