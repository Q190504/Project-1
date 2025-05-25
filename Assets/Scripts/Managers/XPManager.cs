using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class XPManager : MonoBehaviour
{
    private static XPManager _instance;

    [SerializeField] private int orbPrepare;
    [SerializeField] private float spawnChance; // <= 1  Chance to spawn an orb when an enemy is defeated

    private EntityManager entityManager;
    private Entity orbPrefab;

    private NativeQueue<Entity> inactiveOrbs;
    private int orbCount = 0;


    public static XPManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindFirstObjectByType<XPManager>();
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(this.gameObject);

        inactiveOrbs = new NativeQueue<Entity>(Allocator.Persistent);
    }

    private void OnDestroy()
    {
        if (inactiveOrbs.IsCreated)
            inactiveOrbs.Dispose();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        // Get the baked entity prefab
        EntityQuery orbPrefabQuery = entityManager.CreateEntityQuery(typeof(EnemyPrefabComponent));
        if (orbPrefabQuery.CalculateEntityCount() > 0)
        {
            orbPrefab = entityManager.GetComponentData<ExperienceOrbPrefabComponent>(orbPrefabQuery.GetSingletonEntity()).experienceOrbPrefab;
        }
        else
        {
            Debug.LogError("Experience Orb prefab not found! Make sure it's baked correctly.");
        }

        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        Initialize();
        PrepareOrb(ecb);

        ecb.Playback(entityManager);
        ecb.Dispose();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Initialize()
    {

    }

    private void PrepareOrb(EntityCommandBuffer ecb)
    {
        if (orbPrefab == Entity.Null) return;

        for (int i = 0; i < orbPrepare; i++)
        {
            Entity orb = entityManager.Instantiate(orbPrefab);

            SetOrbStatus(orb, false, ecb, entityManager);
            inactiveOrbs.Enqueue(orb);
            orbCount++;
        }
    }

    public void TrySpawnExperienceOrb(Vector3 position, EntityCommandBuffer ecb)
    {
        if (Random.value < spawnChance)
        {
            SpawnOrb(position, ecb);
        }
    }

    public void SpawnOrb(Vector3 position, EntityCommandBuffer ecb)
    {
        if (!GameManager.Instance.IsPlaying())
            return;

        Entity orbInstance = Take(ecb);

        // Set the orb position
        ecb.SetComponent(orbInstance, new LocalTransform
        {
            Position = position,
            Rotation = Quaternion.identity,
            Scale = 1f
        });

        ExperienceOrbComponent experienceOrbComponent = entityManager.GetComponentData<ExperienceOrbComponent>(orbInstance);

        // Set the orb position
        ecb.SetComponent(orbInstance, new ExperienceOrbComponent
        {
            hasBeenCollected = false,
            experience = experienceOrbComponent.experience,
        });
    }

    public Entity Take(EntityCommandBuffer ecb)
    {
        if (inactiveOrbs.IsEmpty())
            PrepareOrb(ecb);

        Entity orb = inactiveOrbs.Dequeue();
        orbCount--;
        SetOrbStatus(orb, true, ecb, entityManager);
        return orb;
    }

    public void Return(Entity orb, EntityCommandBuffer ecb)
    {
        if (!entityManager.Exists(orb)) return;

        SetOrbStatus(orb, false, ecb, entityManager);
        inactiveOrbs.Enqueue(orb);
        orbCount++;
    }

    private void SetOrbStatus(Entity root, bool status, EntityCommandBuffer ecb, EntityManager entityManager)
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
