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
    [SerializeField] private GameObject slimeBeamPrefab;
    [SerializeField] private GameObject slimeBulletSlowZonePrefab;

    private int cloudPrepare;
    private int poisonCloudCount;
    private List<GameObject> inactivePoisonCloudGameObjects;

    private int creepPrepare;
    private int creepCount;
    private List<GameObject> inactiveCreepGameObjects;

    private int slimeBeamPrepare;
    private int slimeBeamCount;
    private List<GameObject> inactiveSlimeBeamGameObjects;

    [SerializeField] private int slimeBulletSlowZonePrepare;
    private int slimeBulletSlowZoneCount;
    private List<GameObject> inactiveSlimeBulletSlowZoneGameObjects;

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
        if (creepCount == 0)
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
        if (hitEffectCount == 0)
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
        if (poisonCloudCount == 0)
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

    private void PrepareSlimeBeam()
    {
        if (slimeBeamPrefab == null) return;

        for (int i = 0; i < slimeBeamPrepare; i++)
        {
            GameObject slimeBeam = Object.Instantiate(slimeBeamPrefab);
            slimeBeam.gameObject.SetActive(false);
            inactiveSlimeBeamGameObjects.Add(slimeBeam);
            slimeBeamCount++;
        }
    }

    public GameObject TakeSlimeBeam()
    {
        if (slimeBeamCount == 0)
            PrepareSlimeBeam();

        GameObject beam = inactiveSlimeBeamGameObjects[0];
        inactiveSlimeBeamGameObjects.RemoveAt(0);
        slimeBeamCount--;
        beam.gameObject.SetActive(true);
        return beam;
    }

    public void ReturnSlimeBeam(GameObject beam)
    {
        beam.gameObject.SetActive(false);
        inactiveSlimeBeamGameObjects.Add(beam);
        slimeBeamCount++;
    }

    private void PrepareSlimeBulletSlowZone()
    {
        if (slimeBulletSlowZonePrefab == null) return;

        for (int i = 0; i < slimeBulletSlowZonePrepare; i++)
        {
            GameObject slimeBulletSlowZone = Object.Instantiate(slimeBulletSlowZonePrefab);
            slimeBulletSlowZone.gameObject.SetActive(false);
            inactiveSlimeBulletSlowZoneGameObjects.Add(slimeBulletSlowZone);
            slimeBulletSlowZoneCount++;
        }
    }

    public GameObject TakeSlimeBulletSlowZone()
    {
        if (slimeBulletSlowZoneCount == 0)
            PrepareSlimeBulletSlowZone();

        GameObject slimeBulletSlowZone = inactiveSlimeBulletSlowZoneGameObjects[0];
        inactiveSlimeBulletSlowZoneGameObjects.RemoveAt(0);
        slimeBulletSlowZoneCount--;
        slimeBulletSlowZone.gameObject.SetActive(true);
        return slimeBulletSlowZone;
    }

    public void ReturnSlimeBulletSlowZone(GameObject slimeBulletSlowZone)
    {
        slimeBulletSlowZone.gameObject.SetActive(false);
        inactiveSlimeBulletSlowZoneGameObjects.Add(slimeBulletSlowZone);
        slimeBulletSlowZoneCount++;
    }

    public void Initialize()
    {
        inactiveCreepGameObjects = new List<GameObject>();
        inactiveHitEffectGameObjects = new List<GameObject>();
        inactivePoisonCloudGameObjects = new List<GameObject>();
        inactiveSlimeBeamGameObjects = new List<GameObject>();
        inactiveSlimeBulletSlowZoneGameObjects = new List<GameObject>();

        creepPrepare = EnemyManager.Instance.GetCreepPrepare();
        cloudPrepare = ProjectilesManager.Instance.GetPoisionCloudPrepare();
        slimeBeamPrepare = ProjectilesManager.Instance.GetSlimeBeamPrepare();

        PrepareCreep();
        PrepareHitEffect();
        PrepareSlimeBeam();
        PreparePoisonCloud();
        PrepareSlimeBulletSlowZone();
    }
}
