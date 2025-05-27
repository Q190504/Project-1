using System.Collections.Generic;
using System;
using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeCard : MonoBehaviour
{
    [SerializeField] private UpgradeType upgradeCardType;
    [SerializeField] private TMP_Text level;
    [SerializeField] private TMP_Text cardName;
    [SerializeField] private Image cardImage;
    [SerializeField] private TMP_Text cardDescription;

    [SerializeField] private VoidPublisherSO onCardClickSO;

    private WeaponType weaponType;
    private PassiveType passiveType;

    private EntityManager entityManager;

    private Dictionary<WeaponType, Action> weaponUpgradeHandlers;
    private Dictionary<PassiveType, Action> passiveUpgradeHandlers;

    private void Awake()
    {
        weaponUpgradeHandlers = new Dictionary<WeaponType, Action>
        {
            { WeaponType.SlimeBulletShooter, () => AddLevelEventToEntityWith<SlimeBulletShooterComponent, SlimeBulletShooterLevelUpEvent>() },
            { WeaponType.SlimeBeamShooter,  () => AddLevelEventToEntityWith<SlimeBeamShooterComponent, SlimeBeamShooterLevelUpEvent>() },
            { WeaponType.RadiantField,  () => AddLevelEventToEntityWith<RadiantFieldComponent, RadiantFieldLevelUpEvent>() },
            { WeaponType.PawPrintPoisoner,  () => AddLevelEventToEntityWith<PawPrintPoisonerComponent, PawPrintPoisonerLevelUpEvent>() },
        };

        passiveUpgradeHandlers = new Dictionary<PassiveType, Action>
        {
            { PassiveType.Damage,      () => AddLevelEventToEntityWith < GenericDamageModifierComponent, DamageLevelUpEvent >() },
            { PassiveType.Armor, () => AddLevelEventToEntityWith < ArmorComponent, ArmorLevelUpEvent >() },
            { PassiveType.MaxHealth,   () => AddLevelEventToEntityWith < MaxHealthComponent, MaxHealthLevelUpEvent >() },
            { PassiveType.MoveSpeed,   () => AddLevelEventToEntityWith < PlayerMovementSpeedComponent, MoveSpeedLevelUpEvent >() },
            { PassiveType.HealthRegen,   () => AddLevelEventToEntityWith < HealthRegenComponent, HealthRegenLevelUpEvent >() },
            { PassiveType.PickupRadius,   () => AddLevelEventToEntityWith < PickupExperienceOrbComponent, PickupRadiusLevelUpEvent >() },
            { PassiveType.AbilityHaste,   () => AddLevelEventToEntityWith < AbilityHasteComponent, AbilityHasteLevelUpEvent >() },
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

    public void SetCardInfo(UpgradeType type, WeaponType weaponType, PassiveType passiveType, string name, string description, Sprite image, int levelNumber)
    {
        upgradeCardType = type;
        if (levelNumber == 0)
            level.text = "NEW";
        else
            level.text = "Lv: " + levelNumber.ToString();
        cardName.text = name;
        if (image == null)
            cardImage.sprite = image;
        cardDescription.text = description;

        this.weaponType = weaponType;
        this.passiveType = passiveType;
    }

    public void Select()
    {
        //Apply the selected card's upgrade
        if(upgradeCardType == UpgradeType.Weapon)
        {
            if (weaponUpgradeHandlers.TryGetValue(weaponType, out var handler))
                handler();
            else
                Debug.LogWarning($"No weapon handler for {weaponType}");
        }
        else
        {
            if (passiveUpgradeHandlers.TryGetValue(passiveType, out var handler))
                handler();
            else
                Debug.LogWarning($"No passive handler for {passiveType}");
        }

        // Unpause the game, close the upgrade panel
        onCardClickSO.RaiseEvent();
    }
}
