using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[UpdateAfter(typeof(GameInitializationSystem))]
//[BurstCompile]
public partial struct PlayerUpgradeSlotsInitializationSystem : ISystem
{
    private EntityManager entityManager;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerUpgradeSlots>();
        entityManager = state.EntityManager;

        // Ensure InitializationTracker exists
        if (!SystemAPI.HasSingleton<InitializationTrackerComponent>())
        {
            var trackerEntity = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponentData(trackerEntity, new InitializationTrackerComponent());
        }
    }

    public void OnUpdate(ref SystemState state)
    {
        if (SystemAPI.TryGetSingletonEntity<PlayerUpgradeSlots>(out Entity player) && GameManager.Instance.IsInitializing())
        {
            // Track Initialization Progress
            if (SystemAPI.TryGetSingleton<InitializationTrackerComponent>(out var initializationTrackerComponent) 
                && !initializationTrackerComponent.playerUpgradeSlotsInitialized)
            {
                var playerUpgradeSlots = SystemAPI.GetComponent<PlayerUpgradeSlots>(player);

                playerUpgradeSlots.passives.Clear();
                playerUpgradeSlots.weapons.Clear();

                FixedList64Bytes<int2> newWeaponList = playerUpgradeSlots.weapons;
                int2 defaultWeapon = new int2(playerUpgradeSlots.defaultWeaponId, 1);
                newWeaponList.Add(defaultWeapon);

                foreach (var weaponComponent in SystemAPI.Query<RefRW<WeaponComponent>>())
                {
                    if (weaponComponent.ValueRO.WeaponType == playerUpgradeSlots.defaultWeaponType)
                    {
                        weaponComponent.ValueRW.Level++;

                        // Apply changes back to PlayerUpgradeSlots
                        state.EntityManager.SetComponentData(player, new PlayerUpgradeSlots
                        {
                            defaultWeaponId = playerUpgradeSlots.defaultWeaponId,
                            defaultWeaponType = playerUpgradeSlots.defaultWeaponType,
                            maxWeaponSlots = playerUpgradeSlots.maxWeaponSlots,
                            weapons = newWeaponList,
                            maxPassvieSlots = playerUpgradeSlots.maxPassvieSlots,
                            passives = playerUpgradeSlots.passives,
                        });

                        // Update UI
                        UpgradeEventArgs upgradeEventArgs = new UpgradeEventArgs
                        {
                            upgradeType = UpgradeType.Weapon,
                            weaponType = playerUpgradeSlots.defaultWeaponType,
                            passiveType = PassiveType.None,
                            id = playerUpgradeSlots.defaultWeaponId,
                            level = weaponComponent.ValueRO.Level,
                        };

                        GamePlayUIManager.Instance.UpdateSlots(upgradeEventArgs);

                        // Update tracker
                        initializationTrackerComponent.playerUpgradeSlotsInitialized = true;
                        state.EntityManager.SetComponentData(SystemAPI.GetSingletonEntity<InitializationTrackerComponent>(), 
                            initializationTrackerComponent);

                        return;
                    }
                }
            }
        }
    }
}