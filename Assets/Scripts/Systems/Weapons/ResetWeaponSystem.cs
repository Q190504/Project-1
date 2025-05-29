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
        if (!GameManager.Instance.IsInitializing())
            return;

        if (SystemAPI.TryGetSingleton<InitializationTrackerComponent>(out var initializationTrackerComponent) && !initializationTrackerComponent.weaponsInitialized)
        {
            if (SystemAPI.TryGetSingletonEntity<SlimeBulletShooterComponent>(out var slimeBulletShooterEntity))
            {
                SlimeBulletShooterComponent slimeBulletShooterComponent
                    = SystemAPI.GetComponent<SlimeBulletShooterComponent>(slimeBulletShooterEntity);

                WeaponComponent weaponComponent = SystemAPI.GetComponent<WeaponComponent>(slimeBulletShooterEntity);

                weaponComponent.Level = 0;

                int levelIndex = weaponComponent.Level;
                var blobData = slimeBulletShooterComponent.Data;
                if (blobData.IsCreated && blobData.Value.Levels.Length > 0)
                {
                    ref var levelData = ref blobData.Value.Levels[levelIndex];

                    slimeBulletShooterComponent.timer = levelData.cooldown;
                }

                state.EntityManager.SetComponentData(slimeBulletShooterEntity, slimeBulletShooterComponent);
                state.EntityManager.SetComponentData(slimeBulletShooterEntity, slimeBulletShooterComponent);
            }

            if (SystemAPI.TryGetSingletonEntity<SlimeBeamShooterComponent>(out var slimeBeamShooterEntity))
            {
                SlimeBeamShooterComponent slimeBeamShooterComponent
                    = SystemAPI.GetComponent<SlimeBeamShooterComponent>(slimeBeamShooterEntity);

                WeaponComponent weaponComponent = SystemAPI.GetComponent<WeaponComponent>(slimeBeamShooterEntity);
                weaponComponent.Level = 0;

                int levelIndex = weaponComponent.Level;
                var blobData = slimeBeamShooterComponent.Data;
                if (blobData.IsCreated && blobData.Value.Levels.Length > 0)
                {
                    ref var levelData = ref blobData.Value.Levels[levelIndex];

                    slimeBeamShooterComponent.timer = levelData.cooldown;
                }

                state.EntityManager.SetComponentData(slimeBeamShooterEntity, slimeBeamShooterComponent);
                state.EntityManager.SetComponentData(slimeBeamShooterEntity, slimeBeamShooterComponent);
            }

            if (SystemAPI.TryGetSingletonEntity<PawPrintPoisonerComponent>(out var pawPrintPoisonerEntity))
            {
                PawPrintPoisonerComponent pawPrintPoisonerComponent
                    = SystemAPI.GetComponent<PawPrintPoisonerComponent>(pawPrintPoisonerEntity);

                WeaponComponent weaponComponent = SystemAPI.GetComponent<WeaponComponent>(pawPrintPoisonerEntity);
               
                weaponComponent.Level = 0;

                int levelIndex = weaponComponent.Level;

                pawPrintPoisonerComponent.distanceTraveled = 0f;

                var blobData = pawPrintPoisonerComponent.Data;
                if (blobData.IsCreated && blobData.Value.Levels.Length > 0)
                {
                    ref var levelData = ref blobData.Value.Levels[levelIndex];

                    pawPrintPoisonerComponent.timer = 0;
                }

                state.EntityManager.SetComponentData(pawPrintPoisonerEntity, pawPrintPoisonerComponent);
                state.EntityManager.SetComponentData(pawPrintPoisonerEntity, pawPrintPoisonerComponent);
            }

            if (SystemAPI.TryGetSingletonEntity<RadiantFieldComponent>(out var radiantFieldEntity))
            {
                RadiantFieldComponent radiantFieldComponent
                    = SystemAPI.GetComponent<RadiantFieldComponent>(radiantFieldEntity);

                WeaponComponent weaponComponent = SystemAPI.GetComponent<WeaponComponent>(radiantFieldEntity);

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

                state.EntityManager.SetComponentData(radiantFieldEntity, radiantFieldComponent);
                state.EntityManager.SetComponentData(radiantFieldEntity, radiantFieldComponent);
            }

            initializationTrackerComponent.weaponsInitialized = true;

            // Update
            state.EntityManager.SetComponentData(SystemAPI.GetSingletonEntity<InitializationTrackerComponent>(), 
                initializationTrackerComponent);
        }
    }
}
