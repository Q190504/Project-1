using UnityEngine;
using UnityEngine.Events;

public class UpgradeEventListener : MonoBehaviour
{
    [SerializeField] private UnityEvent<UpgradeType, WeaponType, PassiveType, int> EventResponse;
    [SerializeField] private UpgradePublisherSO publisher;

    private void OnEnable()
    {
        publisher.OnEventRaised += Respond;
    }

    private void OnDisable()
    {
        publisher.OnEventRaised -= Respond;
    }

    private void Respond(UpgradeType type, WeaponType weaponType, PassiveType passiveType, int ID)
    {
        EventResponse?.Invoke(type, weaponType, passiveType, ID);
    }
}
