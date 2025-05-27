using Unity.Entities;
using UnityEngine;

public class MaxHealthAuthoring : MonoBehaviour
{
    public int ID;
    public int baseMaxHealth;

    public int currentLevel;
    public int maxLevel;
    public int increment;

    public class Baker : Baker<MaxHealthAuthoring>
    {
        public override void Bake(MaxHealthAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new PassiveComponent
            {
                ID = authoring.ID,
                Level = authoring.currentLevel,
                MaxLevel = authoring.maxLevel,
                DisplayName = "Max Health",
                Description = "Increases the maximum health of the player.",
            });
        }
    }
}

public struct MaxHealthComponent : IComponentData
{
    public int level;
    public int increment;
}
