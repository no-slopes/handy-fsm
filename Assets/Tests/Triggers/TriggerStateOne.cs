using System.Collections;
using System.Collections.Generic;
using HandyFSM;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "TriggerStateOne", menuName = "FSM/Triggers/StateOne")]
public class TriggerStateOne : ScriptableState
{
    public void OnInit()
    {

    }

    public void OnEnter()
    {
        Brain.RegisterOnTrigger("tap", GoToTwo);
    }

    public void OnExit()
    {
        Brain.UnregisterOnTrigger("tap", GoToTwo);
    }

    private void GoToTwo()
    {
        Debug.Log($"GoToTwo - {Random.Range(0, 100)}");
        Brain.EndState(Brain.GetState<TriggerStateTwo>());
    }
}