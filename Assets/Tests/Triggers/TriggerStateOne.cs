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
        Brain.UnregisterFromTrigger("tap", GoToTwo);
    }

    private void GoToTwo(TriggerData data)
    {
        if (data is FloatTriggerData floatData)
        {
            Debug.Log($"GoToTwo - {floatData.Value}");
        }
        Brain.EndState(Brain.GetState<TriggerStateTwo>());
    }
}