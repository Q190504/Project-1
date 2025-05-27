using Unity.Entities;
using UnityEngine;

public class ArmorAuthoring : MonoBehaviour
{
    public int ID;
    public int armorVaule;

    public int currentLevel;
    public int increment;

    public class Baker : Baker<ArmorAuthoring>
    {
        public override void Bake(ArmorAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new ArmorComponent
            {
                armorValue = authoring.armorVaule,

                increment = authoring.increment,
            });

            AddComponent(entity, new PassiveComponent
            {
                ID = authoring.ID,

                Level = authoring.currentLevel,
                MaxLevel = 10,
                DisplayName = "Armor",
                Description = "Reduces incoming damage.",
            });
        }
    }
}

public struct ArmorComponent : IComponentData
{
    public int armorValue;

    public int increment;
}