using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class PlayerMovementSpeedAuthoring : MonoBehaviour
{
    public int ID;
    public float baseSpeed;
    public float smoothTime;

    public int currentLevel;
    public float increment;

    class PlayerMovementBaker : Baker<PlayerMovementSpeedAuthoring>
    {
        public override void Bake(PlayerMovementSpeedAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new PlayerMovementSpeedComponent
            {
                baseSpeed = authoring.baseSpeed,
                currentSpeed = authoring.baseSpeed,
                totalSpeed = authoring.baseSpeed,
                smoothTime = authoring.smoothTime,

                increment = authoring.increment,
            });

            AddComponent(entity, new PassiveComponent
            {
                 ID = authoring.ID,
                 Level = authoring.currentLevel,
                 MaxLevel = 5,
                 DisplayName = "Movement Speed",
                 Description = "Increase movement speed.",
            });
        }
    }
}

public struct PlayerMovementSpeedComponent : IComponentData
{
    public float baseSpeed;
    public float currentSpeed;
    public float totalSpeed;
    public float smoothTime;

    public float increment;
}
