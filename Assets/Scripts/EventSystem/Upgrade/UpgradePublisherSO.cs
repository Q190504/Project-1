using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Upgrade Pulisher", menuName = "Scriptable Objects/Events/Upgrade Publisher")]
public class UpgradePublisherSO : ScriptableObject
{
    public UnityAction<UpgradeEventArgs> OnEventRaised;

    public void RaiseEvent(UpgradeEventArgs upgradeEventArgs)
    {
        OnEventRaised?.Invoke(upgradeEventArgs);
    }
}
