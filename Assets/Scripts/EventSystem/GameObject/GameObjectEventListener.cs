using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameObjectEventListener : MonoBehaviour
{
    [SerializeField] private UnityEvent<GameObject, string, int> EventResponse;
    [SerializeField] private GameObjectPublisherSO publisher;

    private void OnEnable()
    {
        publisher.OnEventRaised += Respond;
    }

    private void OnDisable()
    {
        publisher.OnEventRaised -= Respond;
    }

    private void Respond(GameObject obj, string tag,int name)
    {
        EventResponse?.Invoke(obj, tag, name);
    }
}