using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class PlayerInputAuthoring : MonoBehaviour
{
    class PlayerInputBaker : Baker<PlayerInputAuthoring>
    {
        public override void Bake(PlayerInputAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new PlayerInputComponent());
        }
    }
}

public struct PlayerInputComponent : IComponentData
{
    public float2 moveInput;
    public bool isShootingPressed;
    public bool isEPressed;
    public bool isRPressed;
    public bool isEscPressed;
}
