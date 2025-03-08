using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    private static BulletManager _instance;

    private int slimeBulletPrepare;

    private EntityManager entityManager;
    private Entity slimeBulletPrefab;
    private NativeQueue<Entity> inactiveSlimeBullets;
    private int slimeBulletCount = 0;

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

        PrepareBullet();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnDestroy()
    {
        if (inactiveSlimeBullets.IsCreated)
            inactiveSlimeBullets.Dispose();
    }

    private void PrepareBullet()
    {
        for (int i = 0; i < slimeBulletPrepare; i++)
        {
            Entity slimeBulletInstance = entityManager.Instantiate(slimeBulletPrefab);
            entityManager.AddComponent<Disabled>(slimeBulletInstance);
            inactiveSlimeBullets.Enqueue(slimeBulletInstance);
            slimeBulletCount++;
        }
    }
    public Entity Take()
    {
        if (inactiveSlimeBullets.IsEmpty())
            PrepareBullet();

        Entity slimeBulletInstance = inactiveSlimeBullets.Dequeue();
        slimeBulletCount--;
        entityManager.RemoveComponent<Disabled>(slimeBulletInstance);
        return slimeBulletInstance;
    }
    public void Return(Entity bullet)
    {
        if (!entityManager.Exists(bullet)) return;

        entityManager.AddComponent<Disabled>(bullet);
        inactiveSlimeBullets.Enqueue(bullet);
        slimeBulletCount++;
    }

    public void SetBulletPrepare(float bulletPrepareAmount)
    {
        slimeBulletPrepare = (int)bulletPrepareAmount;
    }
}
