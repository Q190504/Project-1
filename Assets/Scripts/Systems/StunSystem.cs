using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[BurstCompile]
public partial struct StunSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (stunTimer, entity) in SystemAPI.Query<RefRW<StunTimerComponent>>().WithEntityAccess())
        {
            stunTimer.ValueRW.timeRemaining -= SystemAPI.Time.DeltaTime;

            if (stunTimer.ValueRO.timeRemaining <= 0)
            {
                ecb.RemoveComponent<StunTimerComponent>(entity);

                if (SystemAPI.HasComponent<PlayerTagComponent>(entity))
                {
                    var playerTag = SystemAPI.GetComponent<PlayerTagComponent>(entity);
                    playerTag.isStunned = false;
                    ecb.SetComponent(entity, playerTag);
                }
            }
            else
            {
                if (SystemAPI.HasComponent<PlayerTagComponent>(entity))
                {
                    var playerTag = SystemAPI.GetComponent<PlayerTagComponent>(entity);
                    playerTag.isStunned = true;
                    ecb.SetComponent(entity, playerTag);
                }
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}
