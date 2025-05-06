using Unity.Entities;
using Unity.Collections;
using UnityEngine;
using System.IO;

public class SlimeBulletShooterAuthoring : MonoBehaviour
{
    public WeaponType weaponId = WeaponType.SlimeBulletShooter;

    [WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
    public class Baker : Baker<SlimeBulletShooterAuthoring>
    {
        public override void Bake(SlimeBulletShooterAuthoring authoring)
        {
            string path = Path.Combine(Application.dataPath, "Data", $"{authoring.weaponId}.json");
            if (!File.Exists(path))
            {
                Debug.LogWarning($"{authoring.weaponId} JSON not found at path: {path}");
                return;
            }

            string jsonText = File.ReadAllText(path);
            SlimeBulletShooterJson weapon = JsonUtility.FromJson<SlimeBulletShooterJson>(jsonText);

            using var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<SlimeBulletShooterDataBlob>();

            var levels = builder.Allocate(ref root.Levels, weapon.levels.Length);
            for (int i = 0; i < weapon.levels.Length; i++)
            {
                var level = weapon.levels[i];

                levels[i] = new SlimeBulletShooterLevelData
                {
                    delay = level.delay,
                    damage = level.damage,
                    cooldown = level.cooldown,
                    bulletCount = level.bulletCount,
                    minimumDistance = level.minimumDistance,
                    minimumDistanceBetweenBullets = level.minimumDistanceBetweenBullets,
                    maximumDistanceBetweenBullets = level.maximumDistanceBetweenBullets,
                    previousDistance = 0f,
                    passthroughDamageModifier = level.passthroughDamageModifier,
                    moveSpeed = level.moveSpeed,
                    existDuration = level.existDuration,
                    bonusDamagePercent = level.bonusDamagePercent,
                    slowModifier = level.slowModifier,
                    slowRadius = level.slowRadius
                };
            }
         
            var blob = builder.CreateBlobAssetReference<SlimeBulletShooterDataBlob>(Allocator.Temp);

            AddComponent(GetEntity(TransformUsageFlags.None), new SlimeBulletShooterComponent
            {
                Data = blob,
                timer = 2f,
                isSlimeFrenzyActive = false,
                level = 0,
            });
        }
    }
}

// ---------- DOTS DATA DEFINITIONS ----------

[System.Serializable]
public class SlimeBulletShooterLevelJson
{
    public float delay;
    public int damage;
    public float cooldown;
    public int bulletCount;
    public float minimumDistance;
    public float minimumDistanceBetweenBullets;
    public float maximumDistanceBetweenBullets;
    public float passthroughDamageModifier;
    public float moveSpeed;
    public float existDuration;
    public float bonusDamagePercent;
    public float slowModifier;
    public float slowRadius;
}

[System.Serializable]
public class SlimeBulletShooterJson
{
    public string id;
    public string name;
    public SlimeBulletShooterLevelJson[] levels;
}

public struct SlimeBulletShooterComponent : IComponentData
{
    public int level;
    public float timer;
    public bool isSlimeFrenzyActive;
    public BlobAssetReference<SlimeBulletShooterDataBlob> Data;
}

public struct SlimeBulletShooterLevelData
{
    public float delay;
    public int damage;
    public float cooldown;
    public int bulletCount;
    public float minimumDistance;
    public float minimumDistanceBetweenBullets;
    public float maximumDistanceBetweenBullets;
    public float previousDistance;
    public float passthroughDamageModifier;
    public float moveSpeed;
    public float existDuration;
    public float bonusDamagePercent;
    public float slowModifier;
    public float slowRadius;
}

public struct SlimeBulletShooterDataBlob
{
    public BlobArray<SlimeBulletShooterLevelData> Levels;
}
