using Unity.Entities;
using UnityEngine;

public class PlayerLevelAuthoring : MonoBehaviour
{
    public int currentLevel;
    public int maxLevel;
    public int baseExperienceToNextLevel;

    public class Baker : Baker<PlayerLevelAuthoring>
    {
        public override void Bake(PlayerLevelAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new PlayerLevelComponent
            {
                currentLevel = authoring.currentLevel,
                maxLevel = authoring.maxLevel,
                baseExperienceToNextLevel = authoring.baseExperienceToNextLevel,
                experience = 0,
                experienceToNextLevel = authoring.baseExperienceToNextLevel
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
    public int baseExperienceToNextLevel;
}
