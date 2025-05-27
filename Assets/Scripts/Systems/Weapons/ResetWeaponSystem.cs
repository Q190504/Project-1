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
                WeaponComponent weaponComponent = SystemAPI.GetSingleton<WeaponComponent>();
                weaponComponent.Level = 0;

                int levelIndex = weaponComponent.Level;
                var blobData = slimeBulletShooterComponent.Data;
                if (blobData.IsCreated && blobData.Value.Levels.Length > 0)
                {
                    ref var levelData = ref blobData.Value.Levels[levelIndex];

                    slimeBulletShooterComponent.timer = levelData.cooldown;
                }

                state.EntityManager.SetComponentData(SystemAPI.GetSingletonEntity<SlimeBulletShooterComponent>(), slimeBulletShooterComponent);
                state.EntityManager.SetComponentData(SystemAPI.GetSingletonEntity<WeaponComponent>(), slimeBulletShooterComponent);
            }

            if (SystemAPI.TryGetSingleton<SlimeBeamShooterComponent>(out var slimeBeamShooterComponent))
            {
                WeaponComponent weaponComponent = SystemAPI.GetSingleton<WeaponComponent>();
                weaponComponent.Level = 0;

                int levelIndex = weaponComponent.Level;
                var blobData = slimeBeamShooterComponent.Data;
                if (blobData.IsCreated && blobData.Value.Levels.Length > 0)
                {
                    ref var levelData = ref blobData.Value.Levels[levelIndex];

                    slimeBeamShooterComponent.timer = levelData.cooldown;
                }

                state.EntityManager.SetComponentData(SystemAPI.GetSingletonEntity<SlimeBeamShooterComponent>(), slimeBeamShooterComponent);
                state.EntityManager.SetComponentData(SystemAPI.GetSingletonEntity<WeaponComponent>(), slimeBeamShooterComponent);
            }

            if (SystemAPI.TryGetSingleton<PawPrintPoisonerComponent>(out var pawPrintPoisonerComponent))
            {
                WeaponComponent weaponComponent = SystemAPI.GetSingleton<WeaponComponent>();
                weaponComponent.Level = 0;

                int levelIndex = weaponComponent.Level;

                pawPrintPoisonerComponent.distanceTraveled = 0f;

                var blobData = pawPrintPoisonerComponent.Data;
                if (blobData.IsCreated && blobData.Value.Levels.Length > 0)
                {
                    ref var levelData = ref blobData.Value.Levels[levelIndex];

                    pawPrintPoisonerComponent.timer = 0;
                }

                state.EntityManager.SetComponentData(SystemAPI.GetSingletonEntity<SlimeBeamShooterComponent>(), pawPrintPoisonerComponent);
                state.EntityManager.SetComponentData(SystemAPI.GetSingletonEntity<PawPrintPoisonerComponent>(), pawPrintPoisonerComponent);
            }

            if (SystemAPI.TryGetSingleton<RadiantFieldComponent>(out var radiantFieldComponent))
            {
                WeaponComponent weaponComponent = SystemAPI.GetSingleton<WeaponComponent>();
                weaponComponent.Level = 0;

                int levelIndex = weaponComponent.Level;

                var blobData = radiantFieldComponent.Data;
                if (blobData.IsCreated && blobData.Value.Levels.Length > 0)
                {
                    ref var levelData = ref blobData.Value.Levels[levelIndex];

                    radiantFieldComponent.timer = 0;

                    // Get the entity of the collider
                    Entity colliderEntity = SystemAPI.GetSingletonEntity<RadiantFieldComponent>();

                    // Update the scale of the collider
                    RefRW<LocalTransform> localTransform = SystemAPI.GetComponentRW<LocalTransform>(colliderEntity);

                    float newRadius = levelData.radius;
                    localTransform.ValueRW.Scale = newRadius;
                }

                state.EntityManager.SetComponentData(SystemAPI.GetSingletonEntity<SlimeBeamShooterComponent>(), radiantFieldComponent);
                state.EntityManager.SetComponentData(SystemAPI.GetSingletonEntity<RadiantFieldComponent>(), radiantFieldComponent);
            }

            tracker.weaponSystemInitialized = true;

            // Update
            state.EntityManager.SetComponentData(SystemAPI.GetSingletonEntity<InitializationTrackerComponent>(), tracker);
        }
    }
}
