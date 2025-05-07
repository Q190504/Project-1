using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[UpdateBefore(typeof(PawPrintPoisonCloudExistingSystem))]
[UpdateBefore(typeof(PawPrintPoisonCloudDamageSystem))]
public partial struct PawPrintPoisonerResetDamageTagSystem : ISystem
{
    private EntityManager entityManager;
    private Entity pawPrintPoisonerEntity;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PawPrintPoisonerComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {

        if (!SystemAPI.TryGetSingletonEntity<PawPrintPoisonerComponent>(out pawPrintPoisonerEntity))
        {
            Debug.Log($"Cant Found Paw Print Poisoner Entity in PawPrintPoisonerResetDamageTagSystem!");
            return;
        }

        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (_, entity) in SystemAPI.Query<RefRO<DamagedByPoisonCloudThisTickTag>>().WithEntityAccess())
        {
            ecb.RemoveComponent<DamagedByPoisonCloudThisTickTag>(entity);
        }
    }
}