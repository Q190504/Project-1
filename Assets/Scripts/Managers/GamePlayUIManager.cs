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
            if (hpBar.value / hpBar.maxValue > GameManager.SKILL_1_THRESHOLD)
            {
                fillImage.color = Color.green;
            }
            else if(hpBar.value / hpBar.maxValue <= GameManager.SKILL_1_THRESHOLD && hpBar.value / hpBar.maxValue > GameManager.SKILL_2_THRESHOLD)
            {
                fillImage.color = Color.yellow;
            }
            else if(hpBar.value / hpBar.maxValue <= GameManager.SKILL_2_THRESHOLD)
            {
                fillImage.color = Color.red;
            }
        }
    }

    public void UpdateXPBar(int currentXP, int maxXP)
    {
        xpBar.maxValue = currentXP;
        xpBar.value = maxXP;

        hpText.text = $"{xpBar.value} / {xpBar.maxValue}";
    }
}
