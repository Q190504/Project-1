using Unity.Entities;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;

[BurstCompile]
public partial struct FrenzySystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SlimeFrenzyComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        if (!GameManager.Instance.IsPlaying()) return;

        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (frenzyTimer, SlimeFrenzyComponent, entity) in
                    SystemAPI.Query<RefRW<SlimeFrenzyTimerComponent>, RefRW<SlimeFrenzyComponent>>().WithEntityAccess())
        {
            frenzyTimer.ValueRW.timeRemaining -= SystemAPI.Time.DeltaTime;

            if (frenzyTimer.ValueRO.timeRemaining <= 0)
            {
                ecb.RemoveComponent<SlimeFrenzyTimerComponent>(entity);

                SlimeFrenzyComponent.ValueRW.isActive = false;

                // Remove the frenzy effect UI when frenzy expires
                GamePlayUIManager.Instance.RemoveEffectImage(ref GamePlayUIManager.Instance.frenzyEffectIndex);
            }
            else
            {
                SlimeFrenzyComponent.ValueRW.isActive = true;

                //if hasn't Frenzy Effect Image yet
                if (GamePlayUIManager.Instance.frenzyEffectIndex == -1)
                    GamePlayUIManager.Instance.AddFrenzyEffectImage();

                // Update frenzy duration UI
                GamePlayUIManager.Instance.UpdateEffectDurationUI(GamePlayUIManager.Instance.frenzyEffectIndex, frenzyTimer.ValueRO.timeRemaining, frenzyTimer.ValueRO.initialDuration);
            }
        }
    }
}
