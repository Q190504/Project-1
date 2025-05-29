using Unity.Burst;
using Unity.Entities;
using UnityEngine;

[BurstCompile]
public partial struct MaxHealthLevelUpSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        if (!GameManager.Instance.IsPlaying()) return;

        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        if (SystemAPI.TryGetSingletonEntity<MaxHealthComponent>(out Entity entity))
        {
            MaxHealthComponent component
                = SystemAPI.GetComponent<MaxHealthComponent>(entity);

            if (state.EntityManager.HasComponent<UpgradeEvent>(entity))
            {
                PassiveComponent passiveComponent = SystemAPI.GetComponent<PassiveComponent>(entity);
                passiveComponent.Level += 1;

                if (SystemAPI.TryGetSingletonEntity<PlayerHealthComponent>(out Entity player))
                {
                    PlayerHealthComponent playerHealthComponent = SystemAPI.GetComponent<PlayerHealthComponent>(player);
                    playerHealthComponent.maxHealth += component.increment;
                    
                    ecb.AddComponent(player, new HealEventComponent
                    {
                        healAmount = component.increment,
                    });

                    ecb.SetComponent(player, playerHealthComponent);
                }
                else
                {
                    Debug.LogWarning("PlayerHealthComponent not found in MaxHealthLevelUpSystem. Cannot update player's health.");
                }

                ecb.SetComponent(entity, passiveComponent);
                ecb.RemoveComponent<UpgradeEvent>(entity);
            }
        }
    }
}