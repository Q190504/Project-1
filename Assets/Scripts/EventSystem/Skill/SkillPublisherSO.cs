using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Skill Pulisher", menuName = "Scriptable Objects/Events/Skill Publisher")]
public class SkillPublisherSO : ScriptableObject
{
    public UnityAction<Skill, int> OnEventRaised;

    public void RaiseEvent(Skill obj, int num = -1)
    {
        OnEventRaised?.Invoke(obj, num);
    }
}