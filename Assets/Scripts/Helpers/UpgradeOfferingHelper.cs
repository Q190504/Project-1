using Unity.Collections;
using Unity.Entities;

public static class UpgradeOfferingHelper
{
    public static NativeList<UpgradeOption> GenerateOfferings(PlayerUpgradeSlots slots)
    {
        var random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1, int.MaxValue));
        var offerings = new NativeList<UpgradeOption>(Allocator.Temp);

        NativeList<Entity> validWeapons = new NativeList<Entity>(Allocator.Temp);
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        EntityQuery weaponQuery = entityManager.CreateEntityQuery(typeof(WeaponComponent));
        if (weaponQuery.CalculateEntityCount() > 0)
        {
            NativeArray<Entity> weaponEntities = weaponQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity weapon in weaponEntities)
            {
                var weaponComponent = entityManager.GetComponentData<WeaponComponent>(weapon);

                // If the weapon is in the player's slots and has a level less than 5, or if it's not in the slots
                if (weaponComponent.Level < 5 && slots.WeaponIDs.Contains(weaponComponent.ID) ||
                   !slots.WeaponIDs.Contains(weaponComponent.ID))
                    validWeapons.Add(weapon);
            }

            weaponEntities.Dispose();
        }

        NativeList<Entity> validPassives = new NativeList<Entity>(Allocator.Temp);
        EntityQuery passiveQuery = entityManager.CreateEntityQuery(typeof(PassiveComponent));
        if (passiveQuery.CalculateEntityCount() > 0)
        {
            NativeArray<Entity> passiveEntities = passiveQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity passive in passiveEntities)
            {
                var passiveComponent = entityManager.GetComponentData<WeaponComponent>(passive);

                // If the passive is in the player's slots and has a level less than 5, or if it's not in the slots
                if (passiveComponent.Level < 5 && slots.PassiveIDs.Contains(passiveComponent.ID) ||
                   !slots.PassiveIDs.Contains(passiveComponent.ID))
                    validPassives.Add(passive);
            }

            passiveEntities.Dispose();
        }

        NativeList<UpgradeOption> combined = new NativeList<UpgradeOption>(Allocator.Temp);

        // Add valid weapons to the combined list
        foreach(var weapon in validWeapons)
        {
            var weaponComponent = entityManager.GetComponentData<WeaponComponent>(weapon);
            UpgradeOption option = new UpgradeOption
            {
                Type = UpgradeType.Weapon,
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
            UpgradeOption option = new UpgradeOption
            {
                Type = UpgradeType.Passive,
                ID = passiveComponent.ID,
                DisplayName = passiveComponent.DisplayName,
                Description = passiveComponent.Description,
                CurrentLevel = passiveComponent.Level
            };

            combined.Add(option);
        }

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
