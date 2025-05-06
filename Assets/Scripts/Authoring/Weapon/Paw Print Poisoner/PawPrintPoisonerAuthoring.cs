using Unity.Entities;
using Unity.Collections;
using UnityEngine;
using System.IO;

public class PawPrintPoisonerAuthoring : MonoBehaviour
{
    public WeaponType weaponId = WeaponType.PawPrintPoisoner; 

    [WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
    public class Baker : Baker<PawPrintPoisonerAuthoring>
    {
        public override void Bake(PawPrintPoisonerAuthoring authoring)
        {
            string path = Path.Combine(Application.dataPath, "Data", $"{authoring.weaponId}.json");
            if (!File.Exists(path))
            {
                Debug.LogWarning($"{authoring.weaponId} JSON not found at path: {path}");
                return;
            }

            string jsonText = File.ReadAllText(path);
            PawPrintPoisonerJson weapon = JsonUtility.FromJson<PawPrintPoisonerJson>(jsonText);

            using var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<PawPrintPoisonerDataBlob>();

            var levels = builder.Allocate(ref root.Levels, weapon.levels.Length);
            for (int i = 0; i < weapon.levels.Length; i++)
            {
                var level = weapon.levels[i];

                levels[i] = new PawPrintPoisonerLevelData
                {
                    damagePerTick = level.damagePerTick,
                    cloudSize = level.cloudSize,
                    maximumCloudDuration = level.maximumCloudDuration,
                    bonusMoveSpeedPerTargetInTheCloud = level.bonusMoveSpeedPerTargetInTheCloud,
                };
            }

            var blob = builder.CreateBlobAssetReference<PawPrintPoisonerDataBlob>(Allocator.Temp);

            AddComponent(GetEntity(TransformUsageFlags.None), new PawPrintPoisonerComponent
            {
                Data = blob,
                level = 1, //TO DO: SET TO 0
                timer = 0f,
                tick = weapon.tick,
                cooldown = weapon.cooldown,
                maximumClouds = weapon.maximumClouds,
                distanceToCreateACloud = weapon.distanceToCreateACloud,
                distanceTraveled = 0f,
            });
        }
    }
}

// ---------- DOTS DATA DEFINITIONS ----------

[System.Serializable]
public class PawPrintPoisonerLevelJson
{
    public int damagePerTick;
    public float cloudSize;
    public float maximumCloudDuration;
    public float bonusMoveSpeedPerTargetInTheCloud;
}

[System.Serializable]
public class PawPrintPoisonerJson
{
    public string id;
    public string name;
    public float tick;
    public float cooldown;
    public int maximumClouds;
    public float distanceToCreateACloud;
    public PawPrintPoisonerLevelJson[] levels;
}

public struct PawPrintPoisonerComponent : IComponentData
{
    public int level;
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
    public float cloudSize;
    public float maximumCloudDuration;
    public float bonusMoveSpeedPerTargetInTheCloud;
}

public struct PawPrintPoisonerDataBlob
{
    public BlobArray<PawPrintPoisonerLevelData> Levels;
}