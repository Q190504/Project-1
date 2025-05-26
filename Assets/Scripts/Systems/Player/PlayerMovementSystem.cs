using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[UpdateAfter(typeof(PawPrintPoisonCloudBoostSpeedSystem))]
[UpdateAfter(typeof(GameInitializationSystem))]
public partial struct PlayerMovementSystem : ISystem
{
    private EntityManager entityManager;
    private Entity player;
    private PlayerTagComponent playerTagComponent;
    private PlayerInputComponent playerInput;
    private PlayerMovementSpeedComponent playerMovement;
    private PhysicsVelocity physicsVelocity;
    private SlimeFrenzyComponent slimeFrenzyComponent;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerTagComponent>();
        state.RequireForUpdate<PlayerInputComponent>();
        state.RequireForUpdate<PlayerMovementSpeedComponent>();
        state.RequireForUpdate<PhysicsVelocity>();
        state.RequireForUpdate<SlimeFrenzyComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        #region Checking

        if (!SystemAPI.TryGetSingletonEntity<PlayerTagComponent>(out player))
        {
            Debug.Log($"Cant Found Player Entity in PlayerMovementSystem!");
            return;
        }
        else
        {
            playerTagComponent = entityManager.GetComponentData<PlayerTagComponent>(player);            
        }

        if (!entityManager.HasComponent<PlayerInputComponent>(player))
        {
            Debug.Log($"Cant Found Player Input Component in PlayerMovementSystem!");
            return;
        }
        else
        {
            playerInput = entityManager.GetComponentData<PlayerInputComponent>(player);
        }

        if (!entityManager.HasComponent<PlayerMovementSpeedComponent>(player))
        {
            Debug.Log($"Cant Found Player Movement Component in PlayerMovementSystem!");
            return;
        }
        else
        {
            playerMovement = entityManager.GetComponentData<PlayerMovementSpeedComponent>(player);
        }

        if (!entityManager.HasComponent<PhysicsVelocity>(player))
        {
            Debug.Log($"Cant Found Physics Velocity in PlayerMovementSystem!");
            return;
        }
        else
        {
            physicsVelocity = entityManager.GetComponentData<PhysicsVelocity>(player);
        }

        if (!entityManager.HasComponent<SlimeFrenzyComponent>(player))
        {
            Debug.Log($"Cant Found Slime Frenzy Component in PlayerMovementSystem!");
            return;
        }
        else
        {
            slimeFrenzyComponent = entityManager.GetComponentData<SlimeFrenzyComponent>(player);
        }

        #endregion

        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        // Track Initialization Progress
        if (SystemAPI.TryGetSingleton<InitializationTrackerComponent>(out var tracker))
        {
            if (!tracker.playerPositionSystemInitialized)
            {
                LocalTransform playerTransform = entityManager.GetComponentData<LocalTransform>(player);

                playerTransform.Position = GameManager.Instance.GetPlayerInitialPosition();

                ecb.SetComponent(player, playerTransform);

                // Update tracker
                tracker.playerPositionSystemInitialized = true;
                ecb.SetComponent(SystemAPI.GetSingletonEntity<InitializationTrackerComponent>(), tracker);
            }
        }

        float3 targetVelocity;

        if (!GameManager.Instance.IsPlaying())
            targetVelocity = float3.zero; 
        else if(playerTagComponent.isStunned)
            targetVelocity = float3.zero;
        else if (playerTagComponent.isFrenzing)
            targetVelocity = new float3(playerInput.moveInput.x, playerInput.moveInput.y, 0)
                * (playerMovement.totalSpeed + playerMovement.totalSpeed * slimeFrenzyComponent.bonusMovementSpeedPercent);
        else
            targetVelocity = new float3(playerInput.moveInput.x, playerInput.moveInput.y, 0) * playerMovement.totalSpeed;

        physicsVelocity.Linear = math.lerp(physicsVelocity.Linear, targetVelocity, playerMovement.smoothTime);

        ecb.SetComponent(player, physicsVelocity);
    }
}
