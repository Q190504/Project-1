using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class ProjectilesManager : MonoBehaviour
{
    private static ProjectilesManager _instance;

    [SerializeField] private int slimeBulletPrepare = 100;
    [SerializeField] private int slimeBeamPrepare = 4;
    [SerializeField] private int poisionCloudPrepare = 24;

    private EntityManager entityManager;
    private Entity slimeBulletPrefab;
    private Entity slimeBeamPrefab;
    private Entity poisionCloudPrefab;

    private NativeQueue<Entity> inactiveSlimeBullets;
    private NativeQueue<Entity> inactiveSlimeBeams;
    private NativeQueue<Entity> inactivePoisionClouds;
    private int slimeBulletCount = 0;
    private int slimeBeamCount = 0;
    private int poisionCloudCount = 0;

    public static ProjectilesManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindFirstObjectByType<ProjectilesManager>();
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(this.gameObject);

        
        inactiveSlimeBullets = new NativeQueue<Entity>(Allocator.Persistent);
        inactiveSlimeBeams = new NativeQueue<Entity>(Allocator.Persistent);
        inactivePoisionClouds = new NativeQueue<Entity>(Allocator.Persistent);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        // Get the baked entity prefab
        EntityQuery query = entityManager.CreateEntityQuery(typeof(SlimeBulletPrefabComponent));
        if (query.CalculateEntityCount() > 0)
        {
            slimeBulletPrefab = entityManager.GetComponentData<SlimeBulletPrefabComponent>(query.GetSingletonEntity()).slimeBulletPrefab;
        }
        else
        {
            Debug.LogError("Slime Bullet prefab not found! Make sure it's baked correctly.");
        }

        EntityQuery slimeBeamPrefabQuery = entityManager.CreateEntityQuery(typeof(SlimeBeamPrefabComponent));
        if (slimeBeamPrefabQuery.CalculateEntityCount() > 0)
        {
            slimeBeamPrefab = entityManager.GetComponentData<SlimeBeamPrefabComponent>(slimeBeamPrefabQuery.GetSingletonEntity()).slimeBeamPrefab;
        }
        else
        {
            Debug.LogError("Slime Beam prefab not found! Make sure it's baked correctly.");
        }

        EntityQuery poisonCloudPrefabQuery = entityManager.CreateEntityQuery(typeof(PawPrintPoisonCloudPrefabComponent));
        if (poisonCloudPrefabQuery.CalculateEntityCount() > 0)
        {
            poisionCloudPrefab = entityManager.GetComponentData<PawPrintPoisonCloudPrefabComponent>(poisonCloudPrefabQuery.GetSingletonEntity()).pawPrintPoisonCloudPrefab;
        }
        else
        {
            Debug.LogError("Paw Print Poison Cloud prefab not found! Make sure it's baked correctly.");
        }


        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        // Prepare the initial pool of entities
        PreparePoisonCloud(ecb);
        PrepareSlimeBeam(ecb);
        PrepareSlimeBullet(ecb);

        ecb.Playback(entityManager);
        ecb.Dispose();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnDestroy()
    {
        if (inactiveSlimeBullets.IsCreated)
            inactiveSlimeBullets.Dispose();

        if (inactiveSlimeBeams.IsCreated)
            inactiveSlimeBeams.Dispose();

        if (inactivePoisionClouds.IsCreated)
            inactivePoisionClouds.Dispose();
    }

    private void PrepareSlimeBullet(EntityCommandBuffer ecb)
    {
        if (slimeBulletPrefab == Entity.Null) return;

        for (int i = 0; i < slimeBulletPrepare; i++)
        {
            Entity slimeBulletInstance = entityManager.Instantiate(slimeBulletPrefab);
            SetEntityStatus(slimeBulletInstance, false, ecb, entityManager);
            inactiveSlimeBullets.Enqueue(slimeBulletInstance);
            slimeBulletCount++;
        }
    }

    private void PrepareSlimeBeam(EntityCommandBuffer ecb)
    {
        if (slimeBeamPrefab == Entity.Null) return;

        for (int i = 0; i < slimeBeamPrepare; i++)
        {
            Entity slimeBeamInstance = entityManager.Instantiate(slimeBeamPrefab);
            SetEntityStatus(slimeBeamInstance, false, ecb, entityManager);
            inactiveSlimeBeams.Enqueue(slimeBeamInstance);
            slimeBeamCount++;
        }
    }

    private void PreparePoisonCloud(EntityCommandBuffer ecb)
    {
        if (poisionCloudPrefab == Entity.Null) return;

        for (int i = 0; i < poisionCloudPrepare; i++)
        {
            Entity poisionCloudInstance = entityManager.Instantiate(poisionCloudPrefab);
            SetEntityStatus(poisionCloudInstance, false, ecb, entityManager);
            inactivePoisionClouds.Enqueue(poisionCloudInstance);
            poisionCloudCount++;
        }
    }

    public Entity TakeSlimeBullet(EntityCommandBuffer ecb)
    {
        if (inactiveSlimeBullets.IsEmpty())
            PrepareSlimeBullet(ecb);

        Entity slimeBulletInstance = inactiveSlimeBullets.Dequeue();
        slimeBulletCount--;
        SetEntityStatus(slimeBulletInstance, true, ecb, entityManager);
        return slimeBulletInstance;
    }

    public Entity TakeSlimeBeam(EntityCommandBuffer ecb)
    {
        if (inactiveSlimeBeams.IsEmpty())
            PrepareSlimeBeam(ecb);

        Entity slimeBeamInstance = inactiveSlimeBeams.Dequeue();
        slimeBeamCount--;
        SetEntityStatus(slimeBeamInstance, true, ecb, entityManager);
        return slimeBeamInstance;
    }

    public Entity TakePoisonCloud(EntityCommandBuffer ecb)
    {
        if (inactivePoisionClouds.IsEmpty())
            PreparePoisonCloud(ecb);

        Entity poisionCloudInstance = inactivePoisionClouds.Dequeue();
        poisionCloudCount--;
        SetEntityStatus(poisionCloudInstance, true, ecb, entityManager);
        return poisionCloudInstance;
    }

    public void ReturnSlimeBullet(Entity bullet, EntityCommandBuffer ecb)
    {
        if (!entityManager.Exists(bullet)) return;

        SetEntityStatus(bullet, false, ecb, entityManager);
        inactiveSlimeBullets.Enqueue(bullet);
        slimeBulletCount++;
    }

    public void ReturnSlimeBeam(Entity beam, EntityCommandBuffer ecb)
    {
        if (!entityManager.Exists(beam)) return;

        SetEntityStatus(beam, false, ecb, entityManager);
        inactiveSlimeBeams.Enqueue(beam);
        slimeBeamCount++;
    }

    public void ReturnPoisonCloud(Entity cloud, EntityCommandBuffer ecb)
    {
        if (!entityManager.Exists(cloud)) return;

        SetEntityStatus(cloud, false, ecb, entityManager);
        inactivePoisionClouds.Enqueue(cloud);
        poisionCloudCount++;
    }

    private void SetEntityStatus(Entity root, bool status, EntityCommandBuffer ecb, EntityManager entityManager)
    {
        DynamicBuffer<Child> children;

        if (status)
        {
            ecb.RemoveComponent<Disabled>(root);
            if (entityManager.HasComponent<Child>(root))
            {
                children = entityManager.GetBuffer<Child>(root);
                foreach (var child in children)
                {
                    ecb.RemoveComponent<Disabled>(child.Value);
                }
            }

        }
        else
        {
            ecb.AddComponent<Disabled>(root);

            if (entityManager.HasComponent<Child>(root))
            {
                children = entityManager.GetBuffer<Child>(root);
                foreach (var child in children)
                {
                    ecb.AddComponent<Disabled>(child.Value);
                }
            }
        }
    }

}
