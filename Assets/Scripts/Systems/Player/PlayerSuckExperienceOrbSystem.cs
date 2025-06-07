using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using Unity.Mathematics;

[BurstCompile]
[UpdateAfter(typeof(PlayerMovementSystem))]
public partial struct PlayerSuckExperienceOrbSystem : ISystem
{
    private EntityManager entityManager;
    private Entity player;
    private LocalTransform playerPositionComponent;
    private PickupExperienceOrbComponent playerPickupRadiusComponent;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerTagComponent>();
        state.RequireForUpdate<ExperienceOrbComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        if (!GameManager.Instance.IsPlaying()) return;

        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        #region Checking

        if (!SystemAPI.TryGetSingletonEntity<PlayerTagComponent>(out player))
        {
            Debug.Log($"Cant Found Player Entity in PlayerSuckExperienceOrbSystem!");
            return;
        }
        else if (!SystemAPI.TryGetSingleton<PickupExperienceOrbComponent>(out playerPickupRadiusComponent))
        {
            Debug.Log($"Cant found Pickup Experience Orb Component in PlayerSuckExperienceOrbSystem!");
            return;
        }
        else
        {
            playerPositionComponent = entityManager.GetComponentData<LocalTransform>(player);
        }

        #endregion

        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

        NativeList<DistanceHit> hits = new NativeList<DistanceHit>(Allocator.Temp);

        CollisionFilter filter = new CollisionFilter
        {
            BelongsTo = 1 << 0,
            CollidesWith = 1 << 7,
            GroupIndex = 0
        };

        physicsWorldSingleton.OverlapSphere(playerPositionComponent.Position, playerPickupRadiusComponent.pickupRadius / 2,
        ref hits, filter);

        //DebugDrawSphere(playerPositionComponent.Position, playerPickupRadiusComponent.pickupRadius / 2, Color.magenta);

        foreach (var hit in hits)
        {
            // Check if the hit entity is an experience orb
            if (!SystemAPI.HasComponent<ExperienceOrbComponent>(hit.Entity))
                continue;

            ExperienceOrbComponent experienceOrbComponent = entityManager.GetComponentData<ExperienceOrbComponent>(hit.Entity);
            experienceOrbComponent.isBeingPulled = true;

            ecb.SetComponent(hit.Entity, experienceOrbComponent);
        }

        hits.Dispose();
    }

    void DebugDrawSphere(float3 center, float radius, Color color)
    {
        int segments = 16;
        for (int i = 0; i < segments; i++)
        {
            float angle1 = (i / (float)segments) * math.PI * 2;
            float angle2 = ((i + 1) / (float)segments) * math.PI * 2;

            float3 p1 = center + new float3(math.cos(angle1), math.sin(angle1), 0) * radius;
            float3 p2 = center + new float3(math.cos(angle2), math.sin(angle2), 0) * radius;

            Debug.DrawLine(p1, p2, color, 0.1f);
        }
    }
}