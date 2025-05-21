using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[UpdateAfter(typeof(GameInitializationSystem))]
public partial struct ResetWeaponSystem : ISystem
{
    private EntityManager entityManager;

    public void OnCreate(ref SystemState state)
    {
        entityManager = state.EntityManager;
        state.RequireForUpdate<InitializationTrackerComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        if (SystemAPI.TryGetSingleton<InitializationTrackerComponent>(out var tracker) && !tracker.weaponSystemInitialized)
        {
            if (SystemAPI.TryGetSingleton<SlimeBulletShooterComponent>(out var slimeBulletShooterComponent))
            {
                slimeBulletShooterComponent.level = 0;

                int levelIndex = slimeBulletShooterComponent.level;
                var blobData = slimeBulletShooterComponent.Data;
                if (blobData.IsCreated && blobData.Value.Levels.Length > 0)
                {
                    ref var levelData = ref blobData.Value.Levels[levelIndex];

                    slimeBulletShooterComponent.timer = levelData.cooldown;
                }

                state.EntityManager.SetComponentData(SystemAPI.GetSingletonEntity<SlimeBulletShooterComponent>(), slimeBulletShooterComponent);
            }

            if (SystemAPI.TryGetSingleton<SlimeBeamShooterComponent>(out var slimeBeamShooterComponent))
            {
                slimeBeamShooterComponent.level = 0;

                int levelIndex = slimeBeamShooterComponent.level;
                var blobData = slimeBeamShooterComponent.Data;
                if (blobData.IsCreated && blobData.Value.Levels.Length > 0)
                {
                    ref var levelData = ref blobData.Value.Levels[levelIndex];

                    slimeBeamShooterComponent.timer = levelData.cooldown;
                }

                state.EntityManager.SetComponentData(SystemAPI.GetSingletonEntity<SlimeBeamShooterComponent>(), slimeBeamShooterComponent);
            }

            if (SystemAPI.TryGetSingleton<PawPrintPoisonerComponent>(out var pawPrintPoisonerComponent))
            {
                pawPrintPoisonerComponent.level = 0;

                int levelIndex = pawPrintPoisonerComponent.level;

                pawPrintPoisonerComponent.distanceTraveled = 0f;

                var blobData = pawPrintPoisonerComponent.Data;
                if (blobData.IsCreated && blobData.Value.Levels.Length > 0)
                {
                    ref var levelData = ref blobData.Value.Levels[levelIndex];

                    pawPrintPoisonerComponent.timer = 0;
                }

                state.EntityManager.SetComponentData(SystemAPI.GetSingletonEntity<PawPrintPoisonerComponent>(), pawPrintPoisonerComponent);
            }

            if (SystemAPI.TryGetSingleton<RadiantFieldComponent>(out var radiantFieldComponent))
            {
                radiantFieldComponent.currentLevel = 0;
                radiantFieldComponent.previousLevel = -1;

                var blobData = radiantFieldComponent.Data;
                if (blobData.IsCreated && blobData.Value.Levels.Length > 0)
                {
                    ref var levelData = ref blobData.Value.Levels[radiantFieldComponent.currentLevel];

                    radiantFieldComponent.timer = 0;

                    // Get the entity of the collider
                    Entity colliderEntity = SystemAPI.GetSingletonEntity<RadiantFieldComponent>();

                    // Update the scale of the collider
                    RefRW<LocalTransform> localTransform = SystemAPI.GetComponentRW<LocalTransform>(colliderEntity);

                    float newRadius = levelData.radius;
                    localTransform.ValueRW.Scale = newRadius;
                }

                state.EntityManager.SetComponentData(SystemAPI.GetSingletonEntity<RadiantFieldComponent>(), radiantFieldComponent);
            }

            tracker.weaponSystemInitialized = true;

            // Update
            state.EntityManager.SetComponentData(SystemAPI.GetSingletonEntity<InitializationTrackerComponent>(), tracker);
        }
    }
}
