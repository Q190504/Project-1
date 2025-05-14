using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class PlayerMovementAuthoring : MonoBehaviour
{
    public float speed;
    public float smoothTime;

    class PlayerMovementBaker : Baker<PlayerMovementAuthoring>
    {
        public override void Bake(PlayerMovementAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new PlayerMovementComponent
            {
                baseSpeed = authoring.speed,
                currentSpeed = authoring.speed,
                smoothTime = authoring.smoothTime,
            });
        }
    }
}

public struct PlayerMovementComponent : IComponentData
{
    public float baseSpeed;
    public float currentSpeed;
    public float smoothTime;
}
