using IndieGabo.HandyFSM;
using UnityEngine;

[CreateAssetMenu(fileName = "TriggerStateTwo", menuName = "FSM/Triggers/StateTwo")]
public class TriggerStateTwo : ScriptableState
{
    public void OnInit()
    {

    }

    public void OnEnter()
    {
        Brain.Triggers.RegisterCallback("tap", GoToOne);
    }

    public void OnExit()
    {
        Brain.Triggers.UnregisterCallback("tap", GoToOne);
    }

    private void GoToOne(TriggerData data)
    {
        if (data is FloatTriggerData floatData)
        {
            // Debug.Log($"GoToOne - {floatData.Value}");
        }
        Brain.EndState(Brain.GetState<TriggerStateOne>());
    }
}