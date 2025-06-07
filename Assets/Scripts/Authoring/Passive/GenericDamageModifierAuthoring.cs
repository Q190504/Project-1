using Unity.Entities;
using UnityEngine;

public class GenericDamageModifierAuthoring : MonoBehaviour
{
    public int ID;
    public float baseGenericDamageModifierValue;

    public int currentLevel;
    public float increment;

    public class Baker : Baker<GenericDamageModifierAuthoring>
    {
        public override void Bake(GenericDamageModifierAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new GenericDamageModifierComponent
            {
                baseGenericDamageModifierValue = authoring.baseGenericDamageModifierValue,
                genericDamageModifierValue = authoring.baseGenericDamageModifierValue,

                increment = authoring.increment,
            });

            AddComponent(entity, new PassiveComponent
            {
                PassiveType = PassiveType.Damage,
                ID = authoring.ID,
                Level = authoring.currentLevel,
                MaxLevel = 5,
                DisplayName = "Damage",
                Description = "Increases the damage dealt by abilities and attacks.",
            });
        }
    }
}

public struct GenericDamageModifierComponent : IComponentData
{
    public float genericDamageModifierValue;
    public float baseGenericDamageModifierValue;
    public float increment;
}
