using Unity.Burst;
using Unity.Entities;
using UnityEngine;

[UpdateAfter(typeof(PawPrintPoisonerSystem))]
public partial struct PawPrintPoisonCloudExistingSystem : ISystem
{
    EntityManager entityManager;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PawPrintPoisonCloudComponent>();
        entityManager = state.EntityManager;
    }

    public void OnUpdate(ref SystemState state)
    {
        if (!GameManager.Instance.IsPlaying()) return;

        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (pawPrintPoisonCloudComponent, cloudEntity) in 
            SystemAPI.Query<RefRW<PawPrintPoisonCloudComponent>>().WithEntityAccess())
        {
            pawPrintPoisonCloudComponent.ValueRW.existDurationTimer -= SystemAPI.Time.DeltaTime;

            // Return this cloud if it's duration is over
            if (pawPrintPoisonCloudComponent.ValueRO.existDurationTimer <= 0)
            {
                // Return the visual game object
                if (entityManager.HasComponent<VisualReferenceComponent>(cloudEntity))
                {
                    VisualReferenceComponent visualReferenceComponent =
                        entityManager.GetComponentData<VisualReferenceComponent>(cloudEntity);
                    AnimationManager.Instance.ReturnPoisonCloud(visualReferenceComponent.gameObject);
                }

                ProjectilesManager.Instance.ReturnPoisonCloud(cloudEntity, ecb);
                continue;
            }
        }
    }
}
