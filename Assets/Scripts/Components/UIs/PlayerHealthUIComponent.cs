using Unity.Entities;
using UnityEngine;

public struct PlayerHealthUIComponent : IComponentData
{
    public float currentHealth;
    public float maxHealth;
}

