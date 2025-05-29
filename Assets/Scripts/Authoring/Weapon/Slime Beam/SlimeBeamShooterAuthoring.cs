using Unity.Entities;
using Unity.Collections;
using UnityEngine;
using System.IO;

public class SlimeBeamShooterAuthoring : MonoBehaviour
{
    public WeaponType weaponType = WeaponType.SlimeBeamShooter;

    public class Baker : Baker<SlimeBeamShooterAuthoring>
    {
        public override void Bake(SlimeBeamShooterAuthoring authoring)
        {
            string path = Path.Combine(Application.dataPath, "Data", $"{authoring.weaponType}.json");
            if (!File.Exists(path))
            {
                Debug.LogWarning($"{authoring.weaponType} JSON not found at path: {path}");
                return;
            }

            string jsonText = File.ReadAllText(path);
            SlimeBeamShooterJson weapon = JsonUtility.FromJson<SlimeBeamShooterJson>(jsonText);

            // Create a new builder that will use temporary memory to construct the blob asset
            var builder = new BlobBuilder(Allocator.Temp);

            // Construct the root object for the blob asset. Notice the use of `ref`.
            ref var root = ref builder.ConstructRoot<SlimeBeamShooterDataBlob>();

            // Now fill the constructed root with the data:
            var levels = builder.Allocate(ref root.Levels, weapon.levels.Length);
            for (int i = 0; i < weapon.levels.Length; i++)
            {
                var level = weapon.levels[i];

                levels[i] = new SlimeBeamShooterLevelData
                {
                    damage = level.damage,
                    cooldown = level.cooldown,
                    range = level.range,
                    timeBetween = level.timeBetween,
                };
            }

            // Now copy the data from the builder into its final place, which will
            // use the persistent allocator
            var blobReference = builder.CreateBlobAssetReference<SlimeBeamShooterDataBlob>(Allocator.Persistent);

            // Make sure to dispose the builder itself so all internal memory is disposed.
            builder.Dispose();

            // Register the Blob Asset to the Baker for de-duplication and reverting.
            AddBlobAsset<SlimeBeamShooterDataBlob>(ref blobReference, out var hash);

            Entity entity = GetEntity(TransformUsageFlags.None);


            AddComponent(entity, new SlimeBeamShooterComponent
            {
                Data = blobReference,
                timer = 2f,
                beamCount = 0,
                timeBetween = 0,
                //level = 0,
                spawnOffsetPositon = weapon.spawnOffsetPositon,
            });

            AddComponent(entity, new WeaponComponent
            {
                WeaponType = authoring.weaponType,
                ID = weapon.id,
                DisplayName = weapon.name,
                Description = "Fires slime beams in four directions.",
                Level = 0,
            });
        }
    }
}

// ---------- DOTS DATA DEFINITIONS ----------

[System.Serializable]
public class SlimeBeamShooterLevelJson
{
    public int damage;
    public float cooldown;
    public float range;
    public float timeBetween;
}

[System.Serializable]
public class SlimeBeamShooterJson
{
    public int id;
    public string name;
    public float spawnOffsetPositon;
    public SlimeBeamShooterLevelJson[] levels;
}

public struct SlimeBeamShooterComponent : IComponentData
{
    //public int level;
    public float timer;
    public float timeBetween;
    public int beamCount;
    public float spawnOffsetPositon;
    public BlobAssetReference<SlimeBeamShooterDataBlob> Data;
}

public struct SlimeBeamShooterLevelData
{
    public int damage;
    public float cooldown;
    public float range;
    public float timeBetween;
}

public struct SlimeBeamShooterDataBlob
{
    public BlobString Name;
    public BlobArray<SlimeBeamShooterLevelData> Levels;
}