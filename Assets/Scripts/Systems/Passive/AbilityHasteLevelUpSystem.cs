using Unity.Entities;
using UnityEngine;

public partial struct AbilityHasteLevelUpSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        if (!GameManager.Instance.IsPlaying()) return;

        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        if (SystemAPI.TryGetSingletonEntity<AbilityHasteComponent>(out Entity entity))
        {
            AbilityHasteComponent component
                = SystemAPI.GetComponent<AbilityHasteComponent>(entity);

            if (state.EntityManager.HasComponent<AbilityHasteLevelUpEvent>(entity))
            {
                PassiveComponent passiveComponent = SystemAPI.GetComponent<PassiveComponent>(entity);
                component.abilityHasteValue += component.increment;
                passiveComponent.Level += 1;

                ecb.SetComponent(entity, component);
                ecb.SetComponent(entity, passiveComponent);
                ecb.RemoveComponent<AbilityHasteLevelUpEvent>(entity);
            }
        }
    }
}
