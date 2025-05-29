using Unity.Burst;
using Unity.Entities;
using UnityEngine;

[BurstCompile]
public partial struct HealthRegenLevelUpSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        if (!GameManager.Instance.IsPlaying()) return;

        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        if (SystemAPI.TryGetSingletonEntity<HealthRegenComponent>(out Entity entity))
        {
            HealthRegenComponent component
                = SystemAPI.GetComponent<HealthRegenComponent>(entity);

            if (state.EntityManager.HasComponent<UpgradeEvent>(entity))
            {
                PassiveComponent passiveComponent = SystemAPI.GetComponent<PassiveComponent>(entity);
                component.healthRegenValue += component.increment;
                passiveComponent.Level += 1;

                ecb.SetComponent(entity, component);
                ecb.SetComponent(entity, passiveComponent);
                ecb.RemoveComponent<UpgradeEvent>(entity);
            }
        }
    }
}
