using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public abstract class Skill : ScriptableObject
{
    public new string name;
    public int id;
    public Sprite skillImage;
    public InputActionReference action;
    public InputActionReference startAction;
    public InputActionReference endAction;

    public bool isCasted = false;
    public int dmg;
    public float castTime;

    [SerializeField] private FloatPublisherSO disableSkill;
    [SerializeField] private FloatPublisherSO enableSkill;
    public enum SkillState
    {
        disabled, ready, casting, active
    }

    public SkillState state = SkillState.ready;
    /*  __________________________________
        |           |                    |
        v           V                    |
     disabled <-> ready <-> casting -> active
         ^                     |
         |_____________________| 
    */
                                                        
    public void SetReady()
    {
        state = SkillState.ready;
        enableSkill.RaiseEvent(id);
    }

    public void SetDisabled()
    {
        state = SkillState.disabled;
        disableSkill.RaiseEvent(id);
    }

    public void SetCasting()
    {
        if (state == SkillState.ready)
            state = SkillState.casting;
    }

    public void SetActive()
    {
        if (state == SkillState.casting)
            state = SkillState.active;
    }

    public SkillState GetState()
    {
        return state;
    }

    public abstract void Cast(GameObject player = null);
    public virtual void CancelCast(GameObject player = null)
    {

    }    
    public abstract void Activate(GameObject player = null);

    public abstract void PerformCurrentAction(GameObject player = null);
    public abstract void NextStage(string actionName, string actionState);
}
