using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildStateIdle : ChildState
{
    protected override void OnInit()
    {
        base.OnInit();
        SetInterruptible(true);
        SetName("Idle");
    }

    public override void OnEnter()
    {
        base.OnEnter();

        Machine.StartCoroutine(WaitAndRun());
    }

    public override void OnExit()
    {
        base.OnExit();
        Debug.Log($"Leaving Idle state");
    }

    private IEnumerator WaitAndRun()
    {
        yield return new WaitForSeconds(0.25f);
        Machine.EndState(Machine.GetState<ChildStateRunnning>());
    }
}