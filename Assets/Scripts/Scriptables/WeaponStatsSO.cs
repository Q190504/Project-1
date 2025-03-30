using UnityEngine;

[CreateAssetMenu(fileName = "WeaponStats", menuName = "Stats/Weapon Stats")]
public class WeaponStatsSO : ScriptableObject
{
    public WeaponType type;
    public float damage;
    public float attackSpeed;
    public float range;
    public float cooldownTime;
}
