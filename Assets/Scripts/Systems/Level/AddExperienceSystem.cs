using UnityEngine;
using Unity.Entities;
using Unity.Burst;

//[BurstCompile]
public partial struct PlayerLevelSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerLevelComponent>();

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

        if (SystemAPI.TryGetSingletonEntity<PlayerLevelComponent>(out Entity player) && GameManager.Instance.IsInitializing())
        {
            // Track Initialization Progress
            if (SystemAPI.TryGetSingleton<InitializationTrackerComponent>(out var tracker) && !tracker.playerLevelInitialized)
            {
                var playerLevel = SystemAPI.GetComponent<PlayerLevelComponent>(player);

                playerLevel.currentLevel = 1;
                playerLevel.maxLevel = 99;
                playerLevel.experience = 0;
                playerLevel.experienceToNextLevel = 20;
                state.EntityManager.SetComponentData(player, playerLevel);

                UpdateXPBar(playerLevel.currentLevel, playerLevel.experience, playerLevel.experienceToNextLevel);

                // Update tracker
                tracker.playerLevelInitialized = true;

                state.EntityManager.SetComponentData(SystemAPI.GetSingletonEntity<InitializationTrackerComponent>(), tracker);
            }
        }

        // Game has ended
        if (!GameManager.Instance.IsPlaying())
            return;

        foreach (var (playerLevel, playerEntity) in SystemAPI.Query<RefRW<PlayerLevelComponent>>().WithEntityAccess())
        {
            // Add Experience
            if (state.EntityManager.HasComponent<AddExperienceComponent>(playerEntity))
            {
                var experienceOrb = state.EntityManager.GetComponentData<AddExperienceComponent>(playerEntity);
                playerLevel.ValueRW.experience += experienceOrb.experienceAmount;

                // Level up
                if (playerLevel.ValueRO.experience >= playerLevel.ValueRO.experienceToNextLevel)
                {
                    playerLevel.ValueRW.currentLevel++;

                    // Reach the max level
                    if (playerLevel.ValueRO.currentLevel > playerLevel.ValueRO.maxLevel)
                    {
                        playerLevel.ValueRW.currentLevel = playerLevel.ValueRO.maxLevel;
                        playerLevel.ValueRW.experience = playerLevel.ValueRO.experienceToNextLevel;
                    }
                    else
                    {
                        playerLevel.ValueRW.experience -= playerLevel.ValueRO.experienceToNextLevel;

                        // Inscrease the experience needed for the next level
                        playerLevel.ValueRW.experienceToNextLevel = (int)(playerLevel.ValueRO.experienceToNextLevel * 1.2f);

                        // Open Upgrade Panel
                        ecb.AddComponent<PlayerLevelUpEvent>(playerEntity);
                    }
                }

                // Update Level Bar
                UpdateXPBar(playerLevel.ValueRO.currentLevel, playerLevel.ValueRO.experience, playerLevel.ValueRO.experienceToNextLevel);

                ecb.RemoveComponent<AddExperienceComponent>(playerEntity);
            }
        }
    }

    public void UpdateXPBar(int currentLevel, int experience, int experienceToNextLevel)
    {
        GamePlayUIManager.Instance.UpdateXPBar(currentLevel, experience, experienceToNextLevel);
    }
}
