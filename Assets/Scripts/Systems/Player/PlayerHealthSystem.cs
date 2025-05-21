using UnityEngine;
using Unity.Entities;
using Unity.Burst;

[BurstCompile]
public partial struct PlayerHealthSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerHealthComponent>();

        // Ensure InitializationTracker exists
        if (!SystemAPI.HasSingleton<InitializationTrackerComponent>())
        {
            var trackerEntity = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponentData(trackerEntity, new InitializationTrackerComponent());
        }
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        if (SystemAPI.TryGetSingletonEntity<PlayerHealthComponent>(out Entity player) && GameManager.Instance.IsInitializing())
        {
            // Track Initialization Progress
            if (SystemAPI.TryGetSingleton<InitializationTrackerComponent>(out var tracker) && !tracker.playerHealthSystemInitialized)
            {
                var playerHealth = SystemAPI.GetComponent<PlayerHealthComponent>(player);

                playerHealth.currentHealth = playerHealth.maxHealth;
                UpdateHPBar(playerHealth.currentHealth, playerHealth.maxHealth);

                // Update tracker
                tracker.playerHealthSystemInitialized = true;

                ecb.SetComponent(SystemAPI.GetSingletonEntity<InitializationTrackerComponent>(), tracker);
            }
        }

        // Game has ended
        if(!GameManager.Instance.IsPlaying())
            return;

        foreach (var (playerHealth, playerEntity) in SystemAPI.Query<RefRW<PlayerHealthComponent>>().WithEntityAccess())
        {
            //Take Damage
            if (state.EntityManager.HasComponent<DamageEventComponent>(playerEntity))
            {
                var damage = state.EntityManager.GetComponentData<DamageEventComponent>(playerEntity);
                playerHealth.ValueRW.currentHealth -= damage.damageAmount;

                if (playerHealth.ValueRO.currentHealth < 0)
                    playerHealth.ValueRW.currentHealth = 0;

                //Update HP Bar
                UpdateHPBar(playerHealth.ValueRO.currentHealth, playerHealth.ValueRO.maxHealth);

                if (playerHealth.ValueRO.currentHealth <= 0)
                {
                    //onDie action
                    Debug.Log("Player Died!");
                    GameManager.Instance.EndGame(false);
                }

                ecb.RemoveComponent<DamageEventComponent>(playerEntity);
            }

            //Heal
            if (state.EntityManager.HasComponent<HealEventComponent>(playerEntity))
            {
                var healEventComponent = state.EntityManager.GetComponentData<HealEventComponent>(playerEntity);
                playerHealth.ValueRW.currentHealth += healEventComponent.healAmount;

                if (playerHealth.ValueRO.currentHealth >= playerHealth.ValueRO.maxHealth)
                {
                    playerHealth.ValueRW.currentHealth = playerHealth.ValueRO.maxHealth;
                }

                //Update HP Bar
                UpdateHPBar(playerHealth.ValueRO.currentHealth, playerHealth.ValueRO.maxHealth);

                ecb.RemoveComponent<HealEventComponent>(playerEntity);
            }
        }
    }

    public void UpdateHPBar(int currentHP, int maxHP)
    {
        GamePlayUIManager.Instance.UpdateHPBar(currentHP, maxHP);
    }
}
