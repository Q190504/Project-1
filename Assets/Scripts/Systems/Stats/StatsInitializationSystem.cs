using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[UpdateAfter(typeof(GameInitializationSystem))]
public partial struct StatsInitializationSystem : ISystem
{
    private EntityManager entityManager;

    public void OnCreate(ref SystemState state)
    {
        entityManager = state.EntityManager;
        state.RequireForUpdate<InitializationTrackerComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        if (SystemAPI.TryGetSingleton<InitializationTrackerComponent>(out var tracker) && !tracker.statsSystemInitialized)
        {
            if (SystemAPI.TryGetSingletonEntity<PlayerTagComponent>(out var player))
            {
                RefRW<PlayerHealthComponent> healthComponent = SystemAPI.GetComponentRW<PlayerHealthComponent>(player);
                RefRW<ArmorComponent> armorComponent = SystemAPI.GetComponentRW<ArmorComponent>(player);
                RefRW<GenericDamageModifierComponent> genericDamageModifierComponent = SystemAPI.GetComponentRW<GenericDamageModifierComponent>(player);
                RefRW<PlayerMovementSpeedComponent> movementSpeedComponent = SystemAPI.GetComponentRW<PlayerMovementSpeedComponent>(player);
                RefRW<AbilityHasteComponent> abilityHasteComponent = SystemAPI.GetComponentRW<AbilityHasteComponent>(player);
                RefRW<PickupExperienceOrbComponent> pickupExperienceOrbComponent = SystemAPI.GetComponentRW<PickupExperienceOrbComponent>(player);
                RefRW<HealthRegenComponent> healthRegenComponent = SystemAPI.GetComponentRW<HealthRegenComponent>(player);

                // Initialize health
                healthComponent.ValueRW.currentLevel = 0;
                healthComponent.ValueRW.maxHealth = healthComponent.ValueRO.baseMaxHealth;
                healthComponent.ValueRW.currentHealth = healthComponent.ValueRO.maxHealth;

                // Initialize armor
                armorComponent.ValueRW.currentLevel = 0;
                armorComponent.ValueRW.armorVaule = 0;

                // Initialize generic damage modifier
                genericDamageModifierComponent.ValueRW.currentLevel = 0;
                genericDamageModifierComponent.ValueRW.genericDamageModifierValue = 0;

                // Initialize movement speed
                movementSpeedComponent.ValueRW.currentLevel = 0;
                movementSpeedComponent.ValueRW.currentSpeed = movementSpeedComponent.ValueRO.baseSpeed;
                movementSpeedComponent.ValueRW.totalSpeed = movementSpeedComponent.ValueRO.currentSpeed;

                // Initialize health regen
                healthRegenComponent.ValueRW.currentLevel = 0;
                healthRegenComponent.ValueRW.healthRegenValue = 0;

                // Initialize ability haste
                abilityHasteComponent.ValueRW.currentLevel = 0;
                abilityHasteComponent.ValueRW.abilityHasteValue = 0;

                // Initialize experience orb pickup
                pickupExperienceOrbComponent.ValueRW.currentLevel = 0;
                pickupExperienceOrbComponent.ValueRW.pickupRadius = pickupExperienceOrbComponent.ValueRO.basePickupRadius;
            }

            tracker.statsSystemInitialized = true;

            // Update
            state.EntityManager.SetComponentData(SystemAPI.GetSingletonEntity<InitializationTrackerComponent>(), tracker);
        }
    }
}
