using UnityEngine;
using UnityEngine.Events;

public class UpgradeEventListener : MonoBehaviour
{
    [SerializeField] private UnityEvent<UpgradeEventArgs> EventResponse;
    [SerializeField] private UpgradePublisherSO publisher;

    private void OnEnable()
    {
        publisher.OnEventRaised += Respond;
    }

    private void OnDisable()
    {
        publisher.OnEventRaised -= Respond;
    }

    private void Respond(UpgradeEventArgs upgradeEventArgs)
    {
        EventResponse?.Invoke(upgradeEventArgs);
    }
}
