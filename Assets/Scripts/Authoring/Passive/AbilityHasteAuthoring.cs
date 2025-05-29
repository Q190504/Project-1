using Unity.Entities;
using UnityEngine;

public class AbilityHasteAuthoring : MonoBehaviour
{
    public int ID;
    public float abilityHasteValue;

    public int currentLevel;
    public float increment;

    public class Baker : Baker<AbilityHasteAuthoring>
    {
        public override void Bake(AbilityHasteAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new AbilityHasteComponent
            {
                abilityHasteValue = authoring.abilityHasteValue,

                increment = authoring.increment,
            });

            AddComponent(entity, new PassiveComponent
            {
                PassiveType = PassiveType.AbilityHaste,
                ID = authoring.ID,
                Level = authoring.currentLevel,
                MaxLevel = 5,
                DisplayName = "Ability Haste",
                Description = "Reduces the cooldown of all Weapons and Abilities.",
            });
        }
    }
}

public struct AbilityHasteComponent : IComponentData
{
    public float abilityHasteValue;

    public float increment;
}
