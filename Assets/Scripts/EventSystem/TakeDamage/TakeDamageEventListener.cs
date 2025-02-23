using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TakeDamageEventListener : MonoBehaviour
{
    [SerializeField] private UnityEvent<float, string, int> EventResponse;
    [SerializeField] private TakeDamagePublisherSO publisher;

    private void OnEnable()
    {
        publisher.OnEventRaised += Respond;
    }

    private void OnDisable()
    {
        publisher.OnEventRaised -= Respond;
    }

    private void Respond(float value, string tag, int myName)
    {
        EventResponse?.Invoke(value, tag, myName);
    }
}