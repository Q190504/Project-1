using Unity.Entities;
using UnityEngine;

public class GenericDamageModifierAuthoring : MonoBehaviour
{
    public float genericDamageModifierValue;

    public int currentLevel;
    public int maxLevel;
    public float increment;

    public class Baker : Baker<GenericDamageModifierAuthoring>
    {
        public override void Bake(GenericDamageModifierAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new GenericDamageModifierComponent
            {
                genericDamageModifierValue = authoring.genericDamageModifierValue,

                currentLevel = authoring.currentLevel,
                maxLevel = authoring.maxLevel,
                increment = authoring.increment,
            });
        }
    }
}

public struct GenericDamageModifierComponent : IComponentData
{
    public float genericDamageModifierValue;

    public int currentLevel;
    public int maxLevel;
    public float increment;
}
