using Unity.Entities;
using UnityEngine;

public class MaxHealthAuthoring : MonoBehaviour
{
    public int ID;
    public int baseMaxHealth;

    public int currentLevel;
    public int increment;

    public class Baker : Baker<MaxHealthAuthoring>
    {
        public override void Bake(MaxHealthAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new MaxHealthComponent
            {
                increment = authoring.increment,
            });

            AddComponent(entity, new PassiveComponent
            {
                PassiveType = PassiveType.MaxHealth,
                ID = authoring.ID,
                Level = authoring.currentLevel,
                MaxLevel = 5,
                DisplayName = "Max Health",
                Description = "Increases the maximum health of the player.",
            });
        }
    }
}

public struct MaxHealthComponent : IComponentData
{
    public int increment;
}
