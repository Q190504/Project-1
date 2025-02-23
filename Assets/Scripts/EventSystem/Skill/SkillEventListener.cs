using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SkillEventListener : MonoBehaviour
{
    [SerializeField] private UnityEvent<Skill, int> EventResponse;
    [SerializeField] private SkillPublisherSO publisher;

    private void OnEnable()
    {
        publisher.OnEventRaised += Respond;
    }

    private void OnDisable()
    {
        publisher.OnEventRaised -= Respond;
    }

    private void Respond(Skill obj, int num=-1)
    {
        EventResponse?.Invoke(obj, num);
    }
}