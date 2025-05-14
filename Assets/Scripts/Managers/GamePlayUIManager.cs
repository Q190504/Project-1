using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class GamePlayUIManager : MonoBehaviour
{
    private static GamePlayUIManager _instance;

    [Header("Bars")]
    public Slider hpBar;
    public TMP_Text hpText;
    public Slider xpBar;
    public TMP_Text xpText;

    [Header("Skills")]
    public Image skill1Image;
    public Image skill1CoodownImage;
    public TMP_Text skill1CoodownText;
    public Image skill2Image;
    public Image skill2CoodownImage;
    public TMP_Text skill2CoodownText;

    [Header("Weapons")]
    public Image basicWeaponImage;
    public Image weapon1Image;
    public Image weapon2Image;
    public Image weapon3Image;
    public Image weapon4Image;

    [Header("Stats")]
    public Image stats1;
    public Image stats2;
    public Image stats3;
    public Image stats4;
    public Image stats5;


    [Header("Effects")]
    public GameObject effectImagePrefab;
    public int stunEffectIndex = -1;
    public Sprite stunEffectSprite;
    public int frenzyEffectIndex = -1;
    public Sprite frenzyEffectSprite;
    public Transform effectsLayout;
    private List<GameObject> effectImageList = new List<GameObject>();

    private Entity player;
    private EntityManager entityManager;

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
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        EntityQuery playerQuery = entityManager.CreateEntityQuery(typeof(PlayerTagComponent));
        if (playerQuery.CalculateEntityCount() > 0)
            player = playerQuery.GetSingletonEntity();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateHPBar(int currentHP, int maxHP)
    {
        hpBar.maxValue = maxHP;
        hpBar.value = currentHP;

        hpText.text = $"{hpBar.value} / {hpBar.maxValue}";

        Image fillImage = hpBar.fillRect.GetComponent<Image>();

        if (fillImage != null)
        {
            if (hpBar.value / hpBar.maxValue > GameManager.Instance.SKILL_1_THRESHOLD)
            {
                fillImage.color = Color.green;
                SetImageOpacity(skill1Image, 0.5f);
                SetImageOpacity(skill2Image, 0.5f);
            }
            else if (hpBar.value / hpBar.maxValue <= GameManager.Instance.SKILL_1_THRESHOLD && hpBar.value / hpBar.maxValue > GameManager.Instance.SKILL_2_THRESHOLD)
            {
                fillImage.color = Color.yellow;
                SetImageOpacity(skill1Image, 1);
                SetImageOpacity(skill2Image, 0.5f);
            }
            else if (hpBar.value / hpBar.maxValue <= GameManager.Instance.SKILL_2_THRESHOLD)
            {
                fillImage.color = Color.red;
                SetImageOpacity(skill1Image, 0.5f);
            }
        }
    }

    public void UpdateXPBar(int currentXP, int maxXP)
    {
        xpBar.maxValue = currentXP;
        xpBar.value = maxXP;

        hpText.text = $"{xpBar.value} / {xpBar.maxValue}";
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
        if(status)
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
}
