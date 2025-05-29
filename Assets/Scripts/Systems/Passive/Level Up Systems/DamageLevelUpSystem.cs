using Unity.Burst;
using Unity.Entities;
using UnityEngine;

[BurstCompile]
public partial struct DamageLevelUpSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        if (!GameManager.Instance.IsPlaying()) return;

        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        if (SystemAPI.TryGetSingletonEntity<GenericDamageModifierComponent>(out Entity entity))
        {
            GenericDamageModifierComponent component
                = SystemAPI.GetComponent<GenericDamageModifierComponent>(entity);

            if (state.EntityManager.HasComponent<UpgradeEvent>(entity))
            {
                PassiveComponent passiveComponent = SystemAPI.GetComponent<PassiveComponent>(entity);
                component.genericDamageModifierValue += component.increment;
                passiveComponent.Level += 1;

                ecb.SetComponent(entity, component);
                ecb.SetComponent(entity, passiveComponent);
                ecb.RemoveComponent<UpgradeEvent>(entity);
            }
        }
    }
}