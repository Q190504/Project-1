using Unity.Burst;
using Unity.Entities;
using UnityEngine;

[BurstCompile]
[UpdateAfter(typeof(PawPrintPoisonerSystem))]
public partial struct PawPrintPoisonCloudExistingSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PawPrintPoisonCloudComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (pawPrintPoisonCloudComponent, cloudEntity) in SystemAPI.Query<RefRW<PawPrintPoisonCloudComponent>>().WithEntityAccess())
        {
            double currentTime = SystemAPI.Time.ElapsedTime;
            // Return if hasn't pass a tick
            if (currentTime - pawPrintPoisonCloudComponent.ValueRO.lastTick < pawPrintPoisonCloudComponent.ValueRO.tick)
                return;

            pawPrintPoisonCloudComponent.ValueRW.lastTick = currentTime;
            pawPrintPoisonCloudComponent.ValueRW.existDurationTimer -= SystemAPI.Time.DeltaTime;

            // Return this cloud if it's duration is over
            if(pawPrintPoisonCloudComponent.ValueRO.existDurationTimer <= 0)
            {
                ProjectilesManager.Instance.ReturnPoisonCloud(cloudEntity, ecb);
            }
        }
    }
}
