using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Upgrade Pulisher", menuName = "Scriptable Objects/Events/Upgrade Publisher")]
public class UpgradePublisherSO : ScriptableObject
{
    public UnityAction<UpgradeType, WeaponType, PassiveType, int> OnEventRaised;

    public void RaiseEvent(UpgradeType type, WeaponType weaponType, PassiveType passiveType, int ID)
    {
        OnEventRaised?.Invoke(type, weaponType, passiveType, ID);
    }
}
