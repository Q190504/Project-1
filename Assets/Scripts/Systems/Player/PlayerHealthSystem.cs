using UnityEngine;
using Unity.Entities;
using Unity.Burst;

[BurstCompile]
public partial struct PlayerHealthSystem : ISystem
{
    private EntityManager entityManager;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerHealthComponent>();
        entityManager = state.EntityManager;

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
                state.EntityManager.SetComponentData(player, playerHealth);

                UpdateHPBar(playerHealth.currentHealth, playerHealth.maxHealth);

                // Update tracker
                tracker.playerHealthSystemInitialized = true;

                state.EntityManager.SetComponentData(SystemAPI.GetSingletonEntity<InitializationTrackerComponent>(), tracker);
            }
        }

        // Game has ended
        if(!GameManager.Instance.IsPlaying())
            return;

        if (entityManager.HasComponent<PlayerHealthComponent>(player))
        {
            PlayerHealthComponent playerHealth = entityManager.GetComponentData<PlayerHealthComponent>(player);

            ArmorComponent armorComponent;
            int armor = 0;
            if (SystemAPI.HasComponent<ArmorComponent>(player))
            {
                armorComponent = entityManager.GetComponentData<ArmorComponent>(player);
                armor = armorComponent.armorValue;
            }
            else
            {
                Debug.Log($"Cant Found Ability Haste Component in PawPrintPoisonerSystem!");
            }

            //Take Damage
            if (state.EntityManager.HasComponent<DamageEventComponent>(player))
            {
                // Calculate damage
                DamageEventComponent damageEventComponent = state.EntityManager.GetComponentData<DamageEventComponent>(player);
                int baseDamage = damageEventComponent.damageAmount;
                int finalDamage = baseDamage - armor;
                if(baseDamage - armor <= 0)
                    finalDamage = 0;

                // Apply damage
                playerHealth.currentHealth -= finalDamage;

                if (playerHealth.currentHealth < 0)
                    playerHealth.currentHealth = 0;

                //Update HP Bar
                UpdateHPBar(playerHealth.currentHealth, playerHealth.maxHealth);

                if (playerHealth.currentHealth <= 0)
                {
                    //onDie action
                    Debug.Log("Player Died!");
                    GameManager.Instance.EndGame(false);
                }

                ecb.RemoveComponent<DamageEventComponent>(player);
            }

            //Heal
            if (state.EntityManager.HasComponent<HealEventComponent>(player))
            {
                var healEventComponent = state.EntityManager.GetComponentData<HealEventComponent>(player);
                playerHealth.currentHealth += healEventComponent.healAmount;

                if (playerHealth.currentHealth >= playerHealth.maxHealth)
                {
                    playerHealth.currentHealth = playerHealth.maxHealth;
                }

                //Update HP Bar
                UpdateHPBar(playerHealth.currentHealth, playerHealth.maxHealth);

                ecb.RemoveComponent<HealEventComponent>(player);
            }

            ecb.SetComponent(player, playerHealth);
        }
    }

    public void UpdateHPBar(int currentHP, int maxHP)
    {
        GamePlayUIManager.Instance.UpdateHPBar(currentHP, maxHP);
    }
}
