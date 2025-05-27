using Unity.Entities;
using UnityEngine;

public partial struct PickupRadiusLevelUpSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        if (!GameManager.Instance.IsPlaying()) return;

        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        if (SystemAPI.TryGetSingletonEntity<PickupExperienceOrbComponent>(out Entity entity))
        {
            PickupExperienceOrbComponent component
                = SystemAPI.GetComponent<PickupExperienceOrbComponent>(entity);

            if (state.EntityManager.HasComponent<LevelUpComponent>(entity))
            {
                PassiveComponent passiveComponent = SystemAPI.GetComponent<PassiveComponent>(entity);
                component.pickupRadius *= (1 + component.increment);
                passiveComponent.Level += 1;

                ecb.SetComponent(entity, component);
                ecb.SetComponent(entity, passiveComponent);
                ecb.RemoveComponent<LevelUpComponent>(entity);
            }
        }
    }
}

