using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[UpdateAfter(typeof(GameInitializationSystem))]
public partial struct PassiveInitializationSystem : ISystem
{
    private EntityManager entityManager;

    public void OnCreate(ref SystemState state)
    {
        entityManager = state.EntityManager;
        state.RequireForUpdate<InitializationTrackerComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        if (SystemAPI.TryGetSingleton<InitializationTrackerComponent>(out var tracker) && !tracker.passiveSystemInitialized)
        {
            foreach (var passiveComponent in SystemAPI.Query<RefRW<PassiveComponent>>())
            {
                passiveComponent.ValueRW.Level = 0;
            }

            if (SystemAPI.TryGetSingletonEntity<PlayerTagComponent>(out var player))
            {
                RefRW<PlayerHealthComponent> healthComponent = SystemAPI.GetComponentRW<PlayerHealthComponent>(player);
                RefRW<PlayerMovementSpeedComponent> movementSpeedComponent = SystemAPI.GetComponentRW<PlayerMovementSpeedComponent>(player);

                // Armor
                Entity armorEntity = SystemAPI.GetSingletonEntity<ArmorComponent>();
                RefRW<ArmorComponent> armorComponent = SystemAPI.GetComponentRW<ArmorComponent>(armorEntity);

                // Generic Damage Modifier
                Entity damageEntity = SystemAPI.GetSingletonEntity<GenericDamageModifierComponent>();
                RefRW<GenericDamageModifierComponent> genericDamageModifierComponent
                    = SystemAPI.GetComponentRW<GenericDamageModifierComponent>(damageEntity);

                // Ability Haste
                Entity abilityHasteEntity = SystemAPI.GetSingletonEntity<AbilityHasteComponent>();
                RefRW<AbilityHasteComponent> abilityHasteComponent = SystemAPI.GetComponentRW<AbilityHasteComponent>(abilityHasteEntity);
                
                // Pickup Radius for Experience Orbs
                Entity pickupExperienceOrbEntity = SystemAPI.GetSingletonEntity<PickupExperienceOrbComponent>();
                RefRW<PickupExperienceOrbComponent> pickupExperienceOrbComponent = SystemAPI.GetComponentRW<PickupExperienceOrbComponent>(pickupExperienceOrbEntity);

                // Health Regen
                Entity healthRegenEntity = SystemAPI.GetSingletonEntity<HealthRegenComponent>();
                RefRW<HealthRegenComponent> healthRegenComponent = SystemAPI.GetComponentRW<HealthRegenComponent>(healthRegenEntity);

                // Initialize health
                healthComponent.ValueRW.maxHealth = healthComponent.ValueRO.baseMaxHealth;
                healthComponent.ValueRW.currentHealth = healthComponent.ValueRO.maxHealth;

                // Initialize armor
                armorComponent.ValueRW.armorValue = 0;

                // Initialize generic damage modifier
                genericDamageModifierComponent.ValueRW.genericDamageModifierValue = 0;

                // Initialize movement speed
                movementSpeedComponent.ValueRW.currentSpeed = movementSpeedComponent.ValueRO.baseSpeed;
                movementSpeedComponent.ValueRW.totalSpeed = movementSpeedComponent.ValueRO.currentSpeed;

                // Initialize health regen
                healthRegenComponent.ValueRW.healthRegenValue = 0;

                // Initialize ability haste
                abilityHasteComponent.ValueRW.abilityHasteValue = 0;

                // Initialize experience orb pickup
                pickupExperienceOrbComponent.ValueRW.pickupRadius = pickupExperienceOrbComponent.ValueRO.basePickupRadius;
            }

            tracker.passiveSystemInitialized = true;

            // Update
            state.EntityManager.SetComponentData(SystemAPI.GetSingletonEntity<InitializationTrackerComponent>(), tracker);
        }
    }
}
