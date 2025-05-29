using Unity.Entities;
using Unity.Collections;
using UnityEngine;
using System.IO;

public class PawPrintPoisonerAuthoring : MonoBehaviour
{
    public WeaponType weaponType = WeaponType.PawPrintPoisoner;

    public class Baker : Baker<PawPrintPoisonerAuthoring>
    {
        public override void Bake(PawPrintPoisonerAuthoring authoring)
        {
            string path = Path.Combine(Application.dataPath, "Data", $"{authoring.weaponType}.json");
            if (!File.Exists(path))
            {
                Debug.LogWarning($"{authoring.weaponType} JSON not found at path: {path}");
                return;
            }

            string jsonText = File.ReadAllText(path);
            PawPrintPoisonerJson weapon = JsonUtility.FromJson<PawPrintPoisonerJson>(jsonText);

            // Create a new builder that will use temporary memory to construct the blob asset
            var builder = new BlobBuilder(Allocator.Temp);

            // Construct the root object for the blob asset. Notice the use of `ref`.
            ref var root = ref builder.ConstructRoot<PawPrintPoisonerDataBlob>();

            // Now fill the constructed root with the data:
            var levels = builder.Allocate(ref root.Levels, weapon.levels.Length);
            for (int i = 0; i < weapon.levels.Length; i++)
            {
                var level = weapon.levels[i];

                levels[i] = new PawPrintPoisonerLevelData
                {
                    damagePerTick = level.damagePerTick,
                    cloudRadius = level.cloudRadius,
                    maximumCloudDuration = level.maximumCloudDuration,
                    bonusMoveSpeedPerTargetInTheCloudModifier = level.bonusMoveSpeedPerTargetInTheCloudModifier,
                };
            }

            // Now copy the data from the builder into its final place, which will
            // use the persistent allocator
            var blobReference = builder.CreateBlobAssetReference<PawPrintPoisonerDataBlob>(Allocator.Persistent);

            // Make sure to dispose the builder itself so all internal memory is disposed.
            builder.Dispose();

            // Register the Blob Asset to the Baker for de-duplication and reverting.
            AddBlobAsset<PawPrintPoisonerDataBlob>(ref blobReference, out var hash);

            Entity entity = GetEntity(TransformUsageFlags.None);


            AddComponent(entity, new PawPrintPoisonerComponent
            {
                Data = blobReference,
                timer = 0f,
                tick = weapon.tick,
                cooldown = weapon.cooldown,
                maximumClouds = weapon.maximumClouds,
                distanceToCreateACloud = weapon.distanceToCreateACloud,
                distanceTraveled = 0f,
            });

            AddComponent(entity, new WeaponComponent
            {
                WeaponType = authoring.weaponType,
                ID = weapon.id,
                DisplayName = weapon.name,
                Description = "Leaves a damaging poison cloud behind while moving.",
                Level = 0,
            });
        }
    }
}

// ---------- DOTS DATA DEFINITIONS ----------

[System.Serializable]
public class PawPrintPoisonerLevelJson
{
    public int damagePerTick;
    public float cloudRadius;
    public float maximumCloudDuration;
    public float bonusMoveSpeedPerTargetInTheCloudModifier;
}

[System.Serializable]
public class PawPrintPoisonerJson
{
    public int id;
    public string name;
    public float tick;
    public float cooldown;
    public int maximumClouds;
    public float distanceToCreateACloud;
    public PawPrintPoisonerLevelJson[] levels;
}

public struct PawPrintPoisonerComponent : IComponentData
{
    public float timer;
    public float tick;
    public float cooldown;
    public int maximumClouds;
    public float distanceToCreateACloud;
    public float distanceTraveled;
    public BlobAssetReference<PawPrintPoisonerDataBlob> Data;
}

public struct PawPrintPoisonerLevelData
{
    public int damagePerTick;
    public float cloudRadius;
    public float maximumCloudDuration;
    public float bonusMoveSpeedPerTargetInTheCloudModifier;
}

public struct PawPrintPoisonerDataBlob
{
    public BlobArray<PawPrintPoisonerLevelData> Levels;
}