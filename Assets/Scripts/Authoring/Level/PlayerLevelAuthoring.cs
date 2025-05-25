using Unity.Entities;
using UnityEngine;

public class PlayerLevelAuthoring : MonoBehaviour
{
    public class Baker : Baker<PlayerLevelAuthoring>
    {
        public override void Bake(PlayerLevelAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new PlayerLevelComponent
            {
                currentLevel = 1,
                maxLevel = 99,
                experience = 0,
                experienceToNextLevel = 100 // Example value, adjust as needed
            });
        }
    }
}

public struct PlayerLevelComponent : IComponentData
{
    public int currentLevel;
    public int maxLevel;
    public int experience;
    public int experienceToNextLevel;
}
