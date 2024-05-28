using System.Collections;
using System.Collections.Generic;
using IndieGabo.HandyFSM;
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
        Brain.Triggers.RegisterCallback("tap", GoToTwo);
        Brain.Signals.Set("test", true);
        Brain.Signals.Set("testInt", Random.Range(0, 100));
    }

    public void OnExit()
    {
        Debug.Log($"{Brain.Signals.ReadInt("testInt")}");
        Brain.Triggers.UnregisterCallback("tap", GoToTwo);
    }

    private void GoToTwo(TriggerData data)
    {
        if (data is FloatTriggerData floatData)
        {
            // Debug.Log($"GoToTwo - {floatData.Value}");
        }
        Brain.EndState(Brain.GetState<TriggerStateTwo>());
    }
}