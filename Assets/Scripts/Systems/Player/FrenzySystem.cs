using Unity.Entities;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;

[BurstCompile]
public partial struct FrenzySystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (frenzyTimer, playerTag, entity) in
                    SystemAPI.Query<RefRW<SlimeFrenzyTimerComponent>, RefRW<PlayerTagComponent>>().WithEntityAccess())
        {
            frenzyTimer.ValueRW.timeRemaining -= SystemAPI.Time.DeltaTime;

            if (frenzyTimer.ValueRO.timeRemaining <= 0)
            {
                ecb.RemoveComponent<SlimeFrenzyTimerComponent>(entity);

                if (SystemAPI.HasComponent<PlayerTagComponent>(entity))
                    playerTag.ValueRW.isFrenzing = false;

                // Remove the frenzy effect UI when frenzy expires
                GamePlayUIManager.Instance.RemoveEffectImage(ref GamePlayUIManager.Instance.frenzyEffectIndex);
            }
            else
            {
                if (SystemAPI.HasComponent<PlayerTagComponent>(entity))
                    playerTag.ValueRW.isFrenzing = true;

                //if hasn't Frenzy Effect Image yet
                if (GamePlayUIManager.Instance.frenzyEffectIndex == -1)
                    GamePlayUIManager.Instance.AddFrenzyEffectImage();

                // Update frenzy duration UI
                GamePlayUIManager.Instance.UpdateEffectDurationUI(GamePlayUIManager.Instance.frenzyEffectIndex, frenzyTimer.ValueRO.timeRemaining, frenzyTimer.ValueRO.initialDuration);
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}
