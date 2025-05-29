using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class GamePlayUIManager : MonoBehaviour
{
    private static GamePlayUIManager _instance;

    [Header("Panels")]
    public GameObject titlePanel;
    public GameObject upgradePanel;
    public GameObject endGamePanel;
    public GameObject settingPanel;
    public GameObject audioSettingPanel;
    public GameObject creditPanel;
    public GameObject comfirmExitPanel;

    [Header("Texts")]
    public TMP_Text currentLevelText;
    public TMP_Text countdownSelectionText;
    public TMP_Text inGameTimeText;
    public TMP_Text inGameEnemyKilledText;
    public TMP_Text endGamePanelTitleText;
    public TMP_Text endGameTimeText;
    public TMP_Text endGameEnemyKilledText;

    [Header("Bars")]
    public Slider hpBar;
    public TMP_Text hpText;
    public Slider xpBar;
    public TMP_Text xpText;

    public Slider leftCountdownBar;
    public Slider rightCountdownBar;

    [Header("Skills")]
    public Image skill1Image;
    public Image skill1CoodownImage;
    public TMP_Text skill1CoodownText;
    public Image skill2Image;
    public Image skill2CoodownImage;
    public TMP_Text skill2CoodownText;

    [Header("Weapons")]
    [SerializeField] private List<UpgradeSlot> weaponSlots;
    private int currentEmptyWeaponSlotIndex;

    [Header("Passives")]
    [SerializeField] private List<UpgradeSlot> passiveSlots;
    private int currentEmptyPassiveSlotIndex;

    [Header("Effects")]
    public GameObject effectImagePrefab;
    public int stunEffectIndex = -1;
    public Sprite stunEffectSprite;
    public int frenzyEffectIndex = -1;
    public Sprite frenzyEffectSprite;
    public Transform effectsLayout;
    private List<GameObject> effectImageList = new List<GameObject>();

    [Header("Cards")]
    public Transform cardLayout;
    public UpgradeCard passiveCardPrefab;
    public UpgradeCard weaponCardPrefab;
    public GameObjectPublisherSO addCardSO;

    [Header("Stats Icon")]
    public Sprite damageIcon;
    public Sprite maxHPIcon;
    public Sprite moveSpeedIcon;
    public Sprite healthRegenIcon;
    public Sprite pickupRadiusIcon;
    public Sprite armorIcon;
    public Sprite abilityHasteIcon;

    [Header("Weapon Icon")]
    public Sprite slimeBulletShooterIcon;
    public Sprite slimeBeamShooterIcon;
    public Sprite pawPrintPoisonerIcon;
    public Sprite radiantFieldIcon;

    private Entity player;
    private EntityManager entityManager;
    PlayerInputComponent playerInput;

    private Dictionary<PassiveType, Sprite> passiveIcons;
    private Dictionary<WeaponType, Sprite> weaponIcons;

    public static GamePlayUIManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindFirstObjectByType<GamePlayUIManager>();
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(this.gameObject);

        passiveIcons = new Dictionary<PassiveType, Sprite>
        {
            { PassiveType.Damage, damageIcon },
            { PassiveType.MaxHealth, maxHPIcon },
            { PassiveType.MoveSpeed, moveSpeedIcon },
            { PassiveType.HealthRegen, healthRegenIcon },
            { PassiveType.PickupRadius, pickupRadiusIcon },
            { PassiveType.Armor, armorIcon },
            { PassiveType.AbilityHaste, abilityHasteIcon }
        };

        weaponIcons = new Dictionary<WeaponType, Sprite>
        {
            { WeaponType.SlimeBulletShooter, slimeBulletShooterIcon },
            { WeaponType.SlimeBeamShooter, slimeBeamShooterIcon },
            { WeaponType.PawPrintPoisoner, pawPrintPoisonerIcon },
            { WeaponType.RadiantField, radiantFieldIcon }
        };
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        EntityQuery playerQuery = entityManager.CreateEntityQuery(typeof(PlayerTagComponent));
        if (playerQuery.CalculateEntityCount() == 0)
        {
            Debug.LogError("[GamePlayUIManager] Player not found in the scene.");
            return;
        }
        player = playerQuery.GetSingletonEntity();

        OpenApplication();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.IsPlaying())
        {
            playerInput = entityManager.GetComponentData<PlayerInputComponent>(player);

            if (playerInput.isEscPressed)
            {
                SetSettingPanel(!settingPanel.activeSelf);
            }
        }
    }

    public void UpdateHPBar(int currentHP, int maxHP)
    {
        hpBar.maxValue = maxHP;
        hpBar.value = currentHP;

        hpText.text = $"{hpBar.value}/{hpBar.maxValue}";
    }

    public void UpdateXPBar(int currentLevel, int experience, int experienceToNextLevel)
    {
        currentLevelText.text = currentLevel.ToString();
        xpBar.maxValue = experienceToNextLevel;
        xpBar.value = experience;

        xpText.text = $"{xpBar.value}/{xpBar.maxValue}";
    }

    // Set opacity (0 = fully transparent, 1 = fully opaque)
    public void SetImageOpacity(Image image, float alpha)
    {
        Color color = image.color;
        color.a = Mathf.Clamp01(alpha);
        image.color = color;
    }

    public void SetSkill2ImageOpacity(bool status)
    {
        if (status)
            SetImageOpacity(skill2Image, 1);
        else
            SetImageOpacity(skill2Image, 0.5f);
    }

    public void SetSkill1ImageOpacity(bool status)
    {
        if (status)
            SetImageOpacity(skill1Image, 1);
        else
            SetImageOpacity(skill1Image, 0.5f);
    }

    public void SetSkill1CooldownUI(bool status)
    {
        skill1CoodownImage.gameObject.SetActive(status);
        skill1CoodownText.gameObject.SetActive(status);
    }

    public void SetSkill2CooldownUI(bool status)
    {
        skill2CoodownImage.gameObject.SetActive(status);
        skill2CoodownText.gameObject.SetActive(status);
    }

    public void UpdateSkill1CooldownUI(float timeRemaining, float cooldownTime)
    {
        skill1CoodownImage.fillAmount = timeRemaining / cooldownTime;
        skill1CoodownText.text = ((int)timeRemaining).ToString();
    }

    public void UpdateSkill2CooldownUI(float timeRemaining, float cooldownTime)
    {
        skill2CoodownImage.fillAmount = timeRemaining / cooldownTime;
        skill2CoodownText.text = ((int)timeRemaining).ToString();
    }

    public void AddStunEffectImage()
    {
        Image[] images = effectImagePrefab.GetComponentsInChildren<Image>();
        if (images.Length > 1)
            images[0].sprite = stunEffectSprite;

        GameObject stunEffectImage = GameObject.Instantiate<GameObject>(effectImagePrefab, effectsLayout);
        effectImageList.Add(stunEffectImage);

        if (stunEffectIndex == -1)
            stunEffectIndex = effectImageList.Count - 1;
    }

    public void AddFrenzyEffectImage()
    {
        Image[] images = effectImagePrefab.GetComponentsInChildren<Image>();
        if (images.Length > 1)
            images[0].sprite = frenzyEffectSprite;

        GameObject frenzyEffectImage = GameObject.Instantiate<GameObject>(effectImagePrefab, effectsLayout);
        effectImageList.Add(frenzyEffectImage);

        if (frenzyEffectIndex == -1)
            frenzyEffectIndex = effectImageList.Count - 1;
    }

    public void UpdateEffectDurationUI(int imageIndex, float timeRemaining, float cooldownTime)
    {
        Image[] images = effectImageList[imageIndex].GetComponentsInChildren<Image>();
        if (images.Length > 1)
            images[1].fillAmount = timeRemaining / cooldownTime;
    }

    public void RemoveEffectImage(ref int imageIndex)
    {
        Destroy(effectImageList[imageIndex].gameObject);
        imageIndex = -1;
    }

    public void SetSettingPanel(bool status)
    {
        settingPanel.SetActive(status);
    }

    public void SetAudioSettingPanel(bool status)
    {
        audioSettingPanel.SetActive(status);
    }

    public void SetTitlePanel(bool status)
    {
        titlePanel.SetActive(status);
    }

    public void SetTime(double time)
    {
        inGameTimeText.text = $"{(int)time / 60:D2} : {(int)time % 60:D2}";
    }

    public void SetEnemyKilled(int enemyKilled)
    {
        inGameEnemyKilledText.text = enemyKilled.ToString();
    }

    public void SetCreditPanel(bool status)
    {
        creditPanel.SetActive(status);
    }

    public void OpenEndGamePanel(bool result)
    {
        if (result)
        {
            endGamePanelTitleText.text = "VICTORY";
        }
        else
        {
            endGamePanelTitleText.text = "DEFEATED";
        }

        endGameTimeText.text = inGameTimeText.text;
        endGameEnemyKilledText.text = inGameEnemyKilledText.text;

        endGamePanel.SetActive(true);
    }

    public void CloseEndGamePanel()
    {
        endGameTimeText.text = "00 : 00";
        endGameEnemyKilledText.text = "0";
        endGamePanel.SetActive(false);
    }

    public void OnStartGame()
    {
        SetSettingPanel(false);
        SetTitlePanel(false);
        CloseEndGamePanel();
        SetConfirmExitPanel(false);
        CloseUpgradePanel();
        ClearSlots();
    }

    public void SetConfirmExitPanel(bool status)
    {
        comfirmExitPanel.SetActive(status);
    }

    public void OpenUpgradePanel(NativeList<UpgradeOptionStruct> upgradeOptions)
    {
        ClearCards();

        // Add cards
        foreach (var upgradeOption in upgradeOptions)
        {
            AddCard(upgradeOption.CardType, upgradeOption.WeaponType, upgradeOption.PassiveType, 
                upgradeOption.ID, upgradeOption.CurrentLevel + 1, upgradeOption.DisplayName.ToString(),
                upgradeOption.Description.ToString());
        }

        upgradePanel.SetActive(true);
    }

    public void CloseUpgradePanel()
    {
        upgradePanel.SetActive(false);
        ClearCards();
    }

    public void AddCard(UpgradeType upgradeType, WeaponType weaponType, PassiveType passiveType, int ID, int level,
        string name, string description)
    {
        Sprite image = null;
        UpgradeCard upgradeCard = null;

        if (upgradeType == UpgradeType.Passive)
        {
            upgradeCard = Instantiate(passiveCardPrefab, cardLayout);
            passiveIcons.TryGetValue(passiveType, out image);
        }
        else if (upgradeType == UpgradeType.Weapon)
        {
            upgradeCard = Instantiate(weaponCardPrefab, cardLayout);
            weaponIcons.TryGetValue(weaponType, out image);
        }

        if (image == null)
        {
            if (upgradeType == UpgradeType.Passive)
                Debug.LogWarning($"[GamePlayUIManager] Unknown type for passive: {passiveType}");
            else if (upgradeType == UpgradeType.Weapon)
                Debug.LogWarning($"[GamePlayUIManager] Unknown type for weapon: {weaponType}");
            else
                Debug.LogWarning($"[GamePlayUIManager] Unknown type for image: {name}");
        }

        upgradeCard.SetCardInfo(upgradeType, weaponType, passiveType, ID, name, description, image, level);
        addCardSO.RaiseEvent(upgradeCard.gameObject);
    }

    public void ClearCards()
    {
        if (cardLayout.childCount > 0)
            foreach (Transform child in cardLayout)
                Destroy(child.gameObject);
    }

    public void UpdateCountdown(float timeLeft, float totalTime)
    {
        leftCountdownBar.maxValue = totalTime;
        leftCountdownBar.value = timeLeft;

        rightCountdownBar.maxValue = totalTime;
        rightCountdownBar.value = timeLeft;

        countdownSelectionText.text = $"{(int)timeLeft}";
    }

    public void UpdateSlots(UpgradeEventArgs upgradeEventArgs)
    {
        if (upgradeEventArgs.upgradeType == UpgradeType.Weapon)
        {
            foreach (UpgradeSlot slot in weaponSlots)
            {
                if (slot.ID == upgradeEventArgs.id)
                {
                    slot.SetLevel(upgradeEventArgs.level);
                    return;
                }
            }

            // set the current empty one if not found
            if (weaponIcons.TryGetValue(upgradeEventArgs.weaponType, out Sprite iconSprite))
                weaponSlots[currentEmptyWeaponSlotIndex].SetSlotInfo(upgradeEventArgs.id, iconSprite, upgradeEventArgs.level);
            else
            {
                Debug.LogWarning($"[GamePlayUIManager] Unknown weapon type: {upgradeEventArgs.weaponType}, using null sprite.");
                weaponSlots[currentEmptyWeaponSlotIndex].SetSlotInfo(upgradeEventArgs.id, null, upgradeEventArgs.level);
            }

            currentEmptyWeaponSlotIndex++;
        }
        else if (upgradeEventArgs.upgradeType == UpgradeType.Passive)
        {
            foreach (UpgradeSlot slot in passiveSlots)
            {
                if (slot.ID == upgradeEventArgs.id)
                {
                    slot.SetLevel(upgradeEventArgs.level);
                    return;
                }
            }

            // set the current empty one if not found
            if (passiveIcons.TryGetValue(upgradeEventArgs.passiveType, out Sprite iconSprite))
                passiveSlots[currentEmptyPassiveSlotIndex].SetSlotInfo(upgradeEventArgs.id, iconSprite, upgradeEventArgs.level);
            else
            {
                Debug.LogWarning($"[GamePlayUIManager] Unknown passive type: {upgradeEventArgs.passiveType}, using null sprite.");
                passiveSlots[currentEmptyPassiveSlotIndex].SetSlotInfo(upgradeEventArgs.id, null, upgradeEventArgs.level);
            }

            currentEmptyPassiveSlotIndex++;
        }
        else
        {
            Debug.LogError($"[GamePlayUIManager] Unknown upgrade type: {upgradeEventArgs.upgradeType}");
        }
    }

    public void ClearSlots()
    {
        foreach (UpgradeSlot slot in weaponSlots)
        {
            slot.ClearSlotInfo();
        }

        currentEmptyWeaponSlotIndex = 0;

        foreach (UpgradeSlot slot in passiveSlots)
        {
            slot.ClearSlotInfo();
        }

        currentEmptyPassiveSlotIndex = 0;
    }

    public void OpenApplication()
    {
        SetSettingPanel(false);
        SetTitlePanel(true);
        CloseEndGamePanel();
        SetConfirmExitPanel(false);
        CloseUpgradePanel();
        ClearSlots();
    }
}
