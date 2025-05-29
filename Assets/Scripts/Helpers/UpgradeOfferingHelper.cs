using UnityEngine;
using Unity.Collections;
using Unity.Entities;

public static class UpgradeOfferingHelper
{
    public static NativeList<UpgradeOptionStruct> GenerateOfferings(PlayerUpgradeSlots slots)
    {
        var random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1, int.MaxValue));
        var offerings = new NativeList<UpgradeOptionStruct>(Allocator.Temp);

        #region Query valid weapons

        NativeList<Entity> validWeapons = new NativeList<Entity>(Allocator.Temp);
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        EntityQuery weaponQuery = entityManager.CreateEntityQuery(typeof(WeaponComponent));
        if (weaponQuery.CalculateEntityCount() > 0)
        {
            NativeArray<Entity> weaponEntities = weaponQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity weapon in weaponEntities)
            {
                var weaponComponent = entityManager.GetComponentData<WeaponComponent>(weapon);
                int weaponID = weaponComponent.ID;

                bool existsInSlots = false;
                int currentLevel = 0;

                // Check if the weapon is already in the player's slots
                for (int i = 0; i < slots.weapons.Length; i++)
                {
                    if (slots.weapons[i].x == weaponID)
                    {
                        existsInSlots = true;
                        currentLevel = slots.weapons[i].y;
                        break;
                    }
                }

                // If weapon is in slots and level is below 5, or (not in slots at all && hasnt full slot)
                if ((existsInSlots && currentLevel < 5) || !existsInSlots && slots.weapons.Length < slots.maxWeaponSlots)
                {
                    validWeapons.Add(weapon);
                }

            }

            weaponEntities.Dispose();
        }

        #endregion

        #region Query valid passives

        NativeList<Entity> validPassives = new NativeList<Entity>(Allocator.Temp);
        EntityQuery passiveQuery = entityManager.CreateEntityQuery(typeof(PassiveComponent));
        if (passiveQuery.CalculateEntityCount() > 0)
        {
            NativeArray<Entity> passiveEntities = passiveQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity passive in passiveEntities)
            {
                var passiveComponent = entityManager.GetComponentData<PassiveComponent>(passive);

                int passiveID = passiveComponent.ID;

                bool existsInSlots = false;
                int currentLevel = 0;

                // Check if the passive is already in the player's slots
                for (int i = 0; i < slots.passives.Length; i++)
                {
                    if (slots.passives[i].x == passiveID)
                    {
                        existsInSlots = true;
                        currentLevel = slots.passives[i].y;
                        break;
                    }
                }

                // If passive is in slots and level is below 5, or (not in slots at all && hasnt full slot)
                if ((existsInSlots && currentLevel < 5) || !existsInSlots && slots.passives.Length < slots.maxPassvieSlots)
                {
                    validPassives.Add(passive);
                }
            }

            passiveEntities.Dispose();
        }

        #endregion

        #region combine valid weapons & passives into valid options

        NativeList<UpgradeOptionStruct> combined = new NativeList<UpgradeOptionStruct>(Allocator.Temp);

        // Add valid weapons to the combined list
        foreach (var weapon in validWeapons)
        {
            var weaponComponent = entityManager.GetComponentData<WeaponComponent>(weapon);
            UpgradeOptionStruct option = new UpgradeOptionStruct
            {
                CardType = UpgradeType.Weapon,
                WeaponType = weaponComponent.WeaponType,
                ID = weaponComponent.ID,
                DisplayName = weaponComponent.DisplayName,
                Description = weaponComponent.Description,
                CurrentLevel = weaponComponent.Level
            };

            combined.Add(option);
        }

        // Add valid passives to the combined list
        foreach (var passive in validPassives)
        {
            var passiveComponent = entityManager.GetComponentData<PassiveComponent>(passive);
            UpgradeOptionStruct option = new UpgradeOptionStruct
            {
                CardType = UpgradeType.Passive,
                PassiveType = passiveComponent.PassiveType,
                ID = passiveComponent.ID,
                DisplayName = passiveComponent.DisplayName,
                Description = passiveComponent.Description,
                CurrentLevel = passiveComponent.Level
            };

            combined.Add(option);
        }

        #endregion

        // Randomly select 3 from combined list
        while (offerings.Length < 3 && combined.Length > 0)
        {
            int index = random.NextInt(combined.Length);
            offerings.Add(combined[index]);
            combined.RemoveAt(index);
        }

        validWeapons.Dispose();
        validPassives.Dispose();
        combined.Dispose();

        return offerings;
    }
}
