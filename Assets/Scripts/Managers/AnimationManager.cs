using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    private static AnimationManager _instance;

    [Header("Visual Prefabs")]
    [SerializeField] private GameObject creepPrefab;
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private GameObject poisonCloudPrefab;

    private int cloudPrepare;
    private int poisonCloudCount;
    private List<GameObject> inactivePoisonCloudGameObjects;

    private int creepPrepare;
    private int creepCount;
    private List<GameObject> inactiveCreepGameObjects;

    [SerializeField] private int hitEffectPrepare;
    private int hitEffectCount;
    private List<GameObject> inactiveHitEffectGameObjects;

    public static AnimationManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindFirstObjectByType<AnimationManager>();
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
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void PrepareCreep()
    {
        if (creepPrefab == null) return;

        for (int i = 0; i < creepPrepare; i++)
        {
            GameObject creep = Object.Instantiate(creepPrefab);
            creep.gameObject.SetActive(false);
            inactiveCreepGameObjects.Add(creep);
            creepCount++;
        }
    }

    public GameObject TakeCreep()
    {
        if (inactiveCreepGameObjects.Count == 0)
            PrepareCreep();

        GameObject creep = inactiveCreepGameObjects[0];
        inactiveCreepGameObjects.RemoveAt(0);
        creepCount--;
        creep.gameObject.SetActive(true);
        return creep;
    }

    public void ReturnCreep(GameObject creep)
    {
        creep.gameObject.SetActive(false);
        inactiveCreepGameObjects.Add(creep);
        creepCount++;
    }

    private void PrepareHitEffect()
    {
        if (hitEffectPrefab == null) return;

        for (int i = 0; i < hitEffectPrepare; i++)
        {
            GameObject hitEffect = Object.Instantiate(hitEffectPrefab);
            hitEffect.gameObject.SetActive(false);
            inactiveHitEffectGameObjects.Add(hitEffect);
            hitEffectCount++;
        }
    }

    public GameObject TakeHitEffect()
    {
        if (inactiveHitEffectGameObjects.Count == 0)
            PrepareHitEffect();

        GameObject hitEffect = inactiveHitEffectGameObjects[0];
        inactiveHitEffectGameObjects.RemoveAt(0);
        hitEffectCount--;
        hitEffect.gameObject.SetActive(true);
        return hitEffect;
    }

    public void ReturnHitEffect(GameObject hitEffect)
    {
        hitEffect.gameObject.SetActive(false);
        inactiveHitEffectGameObjects.Add(hitEffect);
        hitEffectCount++;
    }

    private void PreparePoisonCloud()
    {
        if (poisonCloudPrefab == null) return;

        for (int i = 0; i < cloudPrepare; i++)
        {
            GameObject cloud = Object.Instantiate(poisonCloudPrefab);
            cloud.gameObject.SetActive(false);
            inactivePoisonCloudGameObjects.Add(cloud);
            poisonCloudCount++;
        }
    }

    public GameObject TakePoisonCloud()
    {
        if (inactivePoisonCloudGameObjects.Count == 0)
            PreparePoisonCloud();

        GameObject cloud = inactivePoisonCloudGameObjects[0];
        inactivePoisonCloudGameObjects.RemoveAt(0);
        poisonCloudCount--;
        cloud.gameObject.SetActive(true);
        return cloud;
    }

    public void ReturnPoisonCloud(GameObject cloud)
    {
        cloud.gameObject.SetActive(false);
        inactivePoisonCloudGameObjects.Add(cloud);
        poisonCloudCount++;
    }

    public void Initialize()
    {
        inactiveCreepGameObjects = new List<GameObject>();
        inactiveHitEffectGameObjects = new List<GameObject>();
        inactivePoisonCloudGameObjects = new List<GameObject>();

        creepPrepare = EnemyManager.Instance.GetCreepPrepare();
        cloudPrepare = ProjectilesManager.Instance.GetPoisionCloudPrepare();

        PrepareCreep();
        PrepareHitEffect();
    }
}
