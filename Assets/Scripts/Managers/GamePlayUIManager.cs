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
    public Image skill2Image;

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
            else if(hpBar.value / hpBar.maxValue <= GameManager.Instance.SKILL_1_THRESHOLD && hpBar.value / hpBar.maxValue > GameManager.Instance.SKILL_2_THRESHOLD)
            {
                fillImage.color = Color.yellow;
                SetImageOpacity(skill1Image, 1);
                SetImageOpacity(skill2Image, 0.5f);
            }
            else if(hpBar.value / hpBar.maxValue <= GameManager.Instance.SKILL_2_THRESHOLD)
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
    public void SetImageOpacity(Image image,float alpha)
    {
        Color color = image.color;
        color.a = Mathf.Clamp01(alpha);
        image.color = color;
    }

    public void SetSkill2ImageOpacityUp()
    {
        SetImageOpacity(skill2Image, 1);
    }

    public void SetSkill1ImageOpacityDown()
    {
        SetImageOpacity(skill1Image, 0.5f);
    }
}
