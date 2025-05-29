using System.Collections.Generic;
using System;
using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;
using Unity.Collections;
using Unity.Mathematics;
using Unity.VisualScripting.Antlr3.Runtime.Misc;

public class UpgradeCard : MonoBehaviour
{
    [SerializeField] private UpgradeType upgradeCardType;
    [SerializeField] private TMP_Text level;
    [SerializeField] private TMP_Text cardName;
    [SerializeField] private Image cardImage;
    [SerializeField] private TMP_Text cardDescription;

    [SerializeField] private UpgradePublisherSO updateUISO;
    [SerializeField] private VoidPublisherSO togglePauseSO;

    private int ID;
    private WeaponType weaponType;
    private PassiveType passiveType;

    private EntityManager entityManager;

    private Dictionary<WeaponType, Action> weaponUpgradeHandlers;
    private Dictionary<PassiveType, Action> passiveUpgradeHandlers;

    private void Awake()
    {
        weaponUpgradeHandlers = new Dictionary<WeaponType, Action>
        {
            { WeaponType.SlimeBulletShooter, () => AddLevelEventToEntityWith<SlimeBulletShooterComponent, UpgradeEvent>() },
            { WeaponType.SlimeBeamShooter,  () => AddLevelEventToEntityWith<SlimeBeamShooterComponent, UpgradeEvent>() },
            { WeaponType.RadiantField,  () => AddLevelEventToEntityWith<RadiantFieldComponent, UpgradeEvent>() },
            { WeaponType.PawPrintPoisoner,  () => AddLevelEventToEntityWith<PawPrintPoisonerComponent, UpgradeEvent>() },
        };

        passiveUpgradeHandlers = new Dictionary<PassiveType, Action>
        {
            { PassiveType.Damage,      () => AddLevelEventToEntityWith < GenericDamageModifierComponent, UpgradeEvent >() },
            { PassiveType.Armor, () => AddLevelEventToEntityWith < ArmorComponent, UpgradeEvent >() },
            { PassiveType.MaxHealth,   () => AddLevelEventToEntityWith < MaxHealthComponent, UpgradeEvent >() },
            { PassiveType.MoveSpeed,   () => AddLevelEventToEntityWith < PlayerMovementSpeedComponent, UpgradeEvent >() },
            { PassiveType.HealthRegen,   () => AddLevelEventToEntityWith < HealthRegenComponent, UpgradeEvent >() },
            { PassiveType.PickupRadius,   () => AddLevelEventToEntityWith < PickupExperienceOrbComponent, UpgradeEvent >() },
            { PassiveType.AbilityHaste,   () => AddLevelEventToEntityWith < AbilityHasteComponent, UpgradeEvent >() },
        };
    }

    private void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    private void AddLevelEventToEntityWith<TComponent, TEvent>()
        where TComponent : unmanaged, IComponentData
        where TEvent : unmanaged, IComponentData
    {
        var query = entityManager.CreateEntityQuery(typeof(TComponent));
        using var entities = query.ToEntityArray(Unity.Collections.Allocator.Temp);

        if (entities.Length == 0)
        {
            Debug.LogWarning($"No entity with component {typeof(TComponent).Name} found.");
            return;
        }

        foreach (var entity in entities)
        {
            if (!entityManager.HasComponent<TEvent>(entity))
            {
                entityManager.AddComponent<TEvent>(entity);
                break; // Only level up one entity
            }
        }
    }

    public void SetCardInfo(UpgradeType type, WeaponType weaponType, PassiveType passiveType, int ID,
        string name, string description, Sprite image, int levelNumber)
    {
        this.ID = ID;
        upgradeCardType = type;
        if (levelNumber == 1)
            level.text = "NEW";
        else
            level.text = "Lv: " + levelNumber.ToString();
        cardName.text = name;
        if (image != null)
            cardImage.sprite = image;
        cardDescription.text = description;

        this.weaponType = weaponType;
        this.passiveType = passiveType;
    }

    public void Select()
    {
        Entity playerUpgradeSlotsEntity;
        PlayerUpgradeSlots playerUpgradeSlots;
        int newLevel = 0;
        EntityQuery query = entityManager.CreateEntityQuery(typeof(PlayerUpgradeSlots));
        if (query.CalculateEntityCount() > 0)
        {
            playerUpgradeSlotsEntity = query.GetSingletonEntity();
            playerUpgradeSlots = entityManager.GetComponentData<PlayerUpgradeSlots>(playerUpgradeSlotsEntity);
        }
        else
        {
            Debug.LogError("Player Upgrade Slots not found when apply ugrade. Stop apply ugrade");
            return;
        }

        //Apply the selected card's upgrade
        if (upgradeCardType == UpgradeType.Weapon)
        {
            if (weaponUpgradeHandlers.TryGetValue(weaponType, out var handler))
            {
                // Apply upgrade to component
                handler();

                #region Update UI

                FixedList64Bytes<int2> weapons = playerUpgradeSlots.weapons;

                bool found = false;

                // Check if the weapon ID already exists in the list
                for (int i = 0; i < weapons.Length; i++)
                {
                    if (weapons[i].x == ID)
                    {
                        newLevel = weapons[i].y + 1;

                        weapons[i] = new int2(ID, newLevel);

                        found = true;
                        break;
                    }
                }

                // If not found, add new weapon ID with level 1 to the PlayerUpgradeSlots
                if (!found)
                {
                    newLevel = 1;
                    // Add new passive ID with level 1
                    weapons.Add(new int2(ID, newLevel));
                }

                // Apply changes back to PlayerUpgradeSlots
                entityManager.SetComponentData(playerUpgradeSlotsEntity, new PlayerUpgradeSlots
                {
                    defaultWeaponId = playerUpgradeSlots.defaultWeaponId,
                    defaultWeaponType = playerUpgradeSlots.defaultWeaponType,
                    maxWeaponSlots = playerUpgradeSlots.maxWeaponSlots,
                    weapons = weapons,
                    maxPassvieSlots = playerUpgradeSlots.maxPassvieSlots,
                    passives = playerUpgradeSlots.passives,
                });

                #endregion
            }
            else
            {
                Debug.LogWarning($"No weapon handler for {weaponType}. Stop applying upgrade & update UI");
                return;
            }
        }
        else
        {
            if (passiveUpgradeHandlers.TryGetValue(passiveType, out var handler))
            {
                // Apply upgrade to component
                handler();

                #region Update UI

                FixedList64Bytes<int2> passives = playerUpgradeSlots.passives;

                bool found = false;

                // Check if the passive ID already exists in the list
                for (int i = 0; i < passives.Length; i++)
                {
                    if (passives[i].x == ID)
                    {
                        newLevel = passives[i].y + 1;

                        // Update the level
                        passives[i] = new int2(ID, newLevel);

                        found = true;
                        break; // Exit loop after updating
                    }
                }

                if (!found)
                {
                    newLevel = 1;
                    // Add new passive ID with level 1
                    passives.Add(new int2(ID, newLevel));
                }

                // Always apply updated data back to the component
                entityManager.SetComponentData(playerUpgradeSlotsEntity, new PlayerUpgradeSlots
                {
                    defaultWeaponId = playerUpgradeSlots.defaultWeaponId,
                    defaultWeaponType = playerUpgradeSlots.defaultWeaponType,
                    maxWeaponSlots = playerUpgradeSlots.maxWeaponSlots,
                    weapons = playerUpgradeSlots.weapons,
                    maxPassvieSlots = playerUpgradeSlots.maxPassvieSlots,
                    passives = passives,
                });

                #endregion
            }
            else
            {
                Debug.LogWarning($"No weapon handler for {weaponType}. Stop applying upgrade & update UI");
                return;
            }
        }

        UpgradeEventArgs upgradeEventArgs = new UpgradeEventArgs
        {
            upgradeType = upgradeCardType,
            weaponType = weaponType,
            passiveType = passiveType,
            id = ID,
            level = newLevel,
        };

        updateUISO.RaiseEvent(upgradeEventArgs);
        togglePauseSO.RaiseEvent();
    }
}
