using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class PlayerMovementSpeedAuthoring : MonoBehaviour
{
    public float baseSpeed;
    public float smoothTime;

    public int currentLevel;
    public int maxLevel;
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

                currentLevel = authoring.currentLevel,
                maxLevel = authoring.maxLevel,
                increment = authoring.increment,
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

    public int currentLevel;
    public int maxLevel;
    public float increment;
}
