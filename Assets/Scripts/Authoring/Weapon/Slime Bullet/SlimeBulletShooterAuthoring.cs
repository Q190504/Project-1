using Unity.Entities;
using Unity.Collections;
using UnityEngine;
using System.IO;

public class SlimeBulletShooterAuthoring : MonoBehaviour
{
    public WeaponType weaponId = WeaponType.SlimeBulletShooter;

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

            // Create a new builder that will use temporary memory to construct the blob asset
            var builder = new BlobBuilder(Allocator.Temp);

            // Construct the root object for the blob asset. Notice the use of `ref`.
            ref var root = ref builder.ConstructRoot<SlimeBulletShooterDataBlob>();

            // Now fill the constructed root with the data:
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

            // Now copy the data from the builder into its final place, which will
            // use the persistent allocator
            var blobReference = builder.CreateBlobAssetReference<SlimeBulletShooterDataBlob>(Allocator.Persistent);

            // Make sure to dispose the builder itself so all internal memory is disposed.
            builder.Dispose();

            // Register the Blob Asset to the Baker for de-duplication and reverting.
            AddBlobAsset<SlimeBulletShooterDataBlob>(ref blobReference, out var hash);

            Entity entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new SlimeBulletShooterComponent
            {
                Data = blobReference,
                timer = 2f,
                isSlimeFrenzyActive = false,
            });

            AddComponent(entity, new WeaponComponent
            {
                WeaponType = authoring.weaponId,
                ID = weapon.id,
                DisplayName = weapon.name,
                Description = "Fires slime bullet in the target direction.",
                Level = 0,
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
    public int id;
    public string name;
    public SlimeBulletShooterLevelJson[] levels;
}

public struct SlimeBulletShooterComponent : IComponentData
{
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
