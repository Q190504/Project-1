using Unity.Entities;
using UnityEngine;
using Unity.Collections;
using System.IO;
using Unity.Mathematics;

public class PlayerUpgradeSlotsAuthoring : MonoBehaviour
{
    public WeaponType defaultWeaponType = WeaponType.SlimeBulletShooter;
    public int maxWeaponSlots;
    public int maxPassvieSlots;


    class Baker : Baker<PlayerUpgradeSlotsAuthoring>
    {
        public override void Bake(PlayerUpgradeSlotsAuthoring authoring)
        {
            string path = Path.Combine(Application.dataPath, "Data", $"{authoring.defaultWeaponType}.json");
            if (!File.Exists(path))
            {
                Debug.LogWarning($"{authoring.defaultWeaponType} JSON not found at path: {path}");
                return;
            }

            string jsonText = File.ReadAllText(path);
            SlimeBulletShooterJson defaultWeapon = JsonUtility.FromJson<SlimeBulletShooterJson>(jsonText);

            FixedList64Bytes<int2> WeaponIDs = new FixedList64Bytes<int2>();
            WeaponIDs.Add(defaultWeapon.id);

            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new PlayerUpgradeSlots
            {
                defaultWeaponId = defaultWeapon.id,
                defaultWeaponType = authoring.defaultWeaponType,
                maxWeaponSlots = authoring.maxWeaponSlots,
                weapons = WeaponIDs,
                passives = new FixedList64Bytes<int2>(),
                maxPassvieSlots = authoring.maxPassvieSlots,
            });
        }
    }
}

public struct PlayerUpgradeSlots : IComponentData
{
    public int defaultWeaponId;
    public WeaponType defaultWeaponType;
    public FixedList64Bytes<int2> weapons;      // int2: id, level
    public int maxWeaponSlots;
    public FixedList64Bytes<int2> passives;     // int2: id, level
    public int maxPassvieSlots;
}
