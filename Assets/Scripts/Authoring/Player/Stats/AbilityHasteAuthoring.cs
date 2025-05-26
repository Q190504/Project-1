using Unity.Entities;
using UnityEngine;

public class AbilityHasteAuthoring : MonoBehaviour
{
    public float abilityHasteValue;

    public int currentLevel;
    public int maxLevel;
    public float increment;

    public class Baker : Baker<AbilityHasteAuthoring>
    {
        public override void Bake(AbilityHasteAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new AbilityHasteComponent
            {
                abilityHasteValue = authoring.abilityHasteValue,

                currentLevel = authoring.currentLevel,
                maxLevel = authoring.maxLevel,
                increment = authoring.increment,
            });
        }
    }
}

public struct AbilityHasteComponent : IComponentData
{
    public float abilityHasteValue;

    public int currentLevel;
    public int maxLevel;
    public float increment;
}
