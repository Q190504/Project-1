using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    private static BulletManager _instance;

    [SerializeField] private int slimeBulletPrepare = 100;
    [SerializeField] private int slimeBeamPrepare = 4;

    private EntityManager entityManager;
    private Entity slimeBulletPrefab;
    private Entity slimeBeamPrefab;

    private NativeQueue<Entity> inactiveSlimeBullets;
    private NativeQueue<Entity> inactiveSlimeBeams;
    private int slimeBulletCount = 0;
    private int slimeBeamCount = 0;

    public static BulletManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindFirstObjectByType<BulletManager>();
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

        Prepare();
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
    }

    private void Prepare()
    {
        if (slimeBulletPrefab == Entity.Null) return;

        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        for (int i = 0; i < slimeBulletPrepare; i++)
        {
            Entity slimeBulletInstance = entityManager.Instantiate(slimeBulletPrefab);
            ecb.AddComponent<Disabled>(slimeBulletInstance);
            inactiveSlimeBullets.Enqueue(slimeBulletInstance);
            slimeBulletCount++;
        }

        for (int i = 0; i < slimeBeamPrepare; i++)
        {
            Entity slimeBeamInstance = entityManager.Instantiate(slimeBeamPrefab);
            ecb.AddComponent<Disabled>(slimeBeamInstance);
            inactiveSlimeBeams.Enqueue(slimeBeamInstance);
            slimeBeamCount++;
        }

        ecb.Playback(entityManager);
        ecb.Dispose();
    }
    public Entity Take(EntityCommandBuffer ecb)
    {
        if (inactiveSlimeBullets.IsEmpty())
            Prepare();

        Entity slimeBulletInstance = inactiveSlimeBullets.Dequeue();
        slimeBulletCount--;
        ecb.RemoveComponent<Disabled>(slimeBulletInstance);
        return slimeBulletInstance;
    }

    public Entity TakeSlimeBeam(EntityCommandBuffer ecb)
    {
        if (inactiveSlimeBeams.IsEmpty())
            Prepare();

        Entity slimeBeamInstance = inactiveSlimeBeams.Dequeue();
        slimeBeamCount--;
        ecb.RemoveComponent<Disabled>(slimeBeamInstance);
        return slimeBeamInstance;
    }

    public void Return(Entity bullet, EntityCommandBuffer ecb)
    {
        if (!entityManager.Exists(bullet)) return;

        ecb.AddComponent<Disabled>(bullet);
        inactiveSlimeBullets.Enqueue(bullet);
        slimeBulletCount++;
    }

    public void ReturnSlimeBeam(Entity beam, EntityCommandBuffer ecb)
    {
        if (!entityManager.Exists(beam)) return;

        ecb.AddComponent<Disabled>(beam);
        inactiveSlimeBeams.Enqueue(beam);
        slimeBeamCount++;
    }
}
