using Unity.Burst;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

[BurstCompile]
[UpdateAfter(typeof(PlayerSuckExperienceOrbSystem))]
public partial struct XPOrbMoveSystem : ISystem
{
    private EntityManager entityManager;
    private Entity player;
    private LocalTransform playerPositionComponent;
    private PickupExperienceOrbComponent playerPickupRadiusComponent;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<ExperienceOrbComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        if (!GameManager.Instance.IsPlaying()) return;

        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        #region Checking

        if (!SystemAPI.TryGetSingletonEntity<PlayerTagComponent>(out player))
        {
            Debug.Log($"Cant Found Player Entity in XPOrbMoveSystem!");
            return;
        }
        else if (!SystemAPI.TryGetSingleton<PickupExperienceOrbComponent>(out playerPickupRadiusComponent))
        {
            Debug.Log($"Cant found Pickup Experience Orb Component in XPOrbMoveSystem!");
            return;
        }
        else
        {
            playerPositionComponent = entityManager.GetComponentData<LocalTransform>(player);
        }

        #endregion

        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (localTransform, experienceOrbComponent, entity) in SystemAPI.Query<LocalTransform, ExperienceOrbComponent>().WithEntityAccess())
        {
            if (!experienceOrbComponent.hasBeenCollected && experienceOrbComponent.isBeingPulled)
            {
                float3 orbPosition  = localTransform.Position;
                float3 directionToPlayer = math.normalize(playerPositionComponent.Position - orbPosition);
                float deltaTime = SystemAPI.Time.DeltaTime;

                // Move orb toward player
                orbPosition += directionToPlayer * playerPickupRadiusComponent.pullForce * deltaTime;

                // Update the orb's position
                ecb.SetComponent(entity, new LocalTransform
                {
                    Position = orbPosition,
                    Rotation = quaternion.identity,
                    Scale = entityManager.GetComponentData<LocalTransform>(entity).Scale,
                });
            }
        }
    }
}
