using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[BurstCompile]
public partial struct SlimeBeamSystem : ISystem
{
    private EntityManager entityManager;

    public void OnCreate(ref SystemState state)
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    public void OnUpdate(ref SystemState state)
    {
        if (!GameManager.Instance.IsPlaying()) return;

        float deltaTime = SystemAPI.Time.DeltaTime;
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (slimeBeamComponent, beamEntity) in SystemAPI.Query<RefRW<SlimeBeamComponent>>().WithEntityAccess())
        {
            slimeBeamComponent.ValueRW.timer -= deltaTime;
            // Destroy if out of lifetime
            if (slimeBeamComponent.ValueRO.timer <= 0)
                ProjectilesManager.Instance.ReturnSlimeBeam(beamEntity, ecb);
        }

        ecb.Playback(entityManager);
        ecb.Dispose();
    }
}
