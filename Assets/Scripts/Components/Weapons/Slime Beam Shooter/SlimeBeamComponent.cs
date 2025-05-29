using Unity.Entities;

public struct SlimeBeamComponent : IComponentData
{
    public int damage;
    public bool hasDealDamageToEnemies;
    public float timer;
}
