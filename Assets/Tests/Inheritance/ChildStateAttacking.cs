using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildStateAttacking : ChildState
{
    protected override void OnInit()
    {
        base.OnInit();
        SetInterruptible(true);
        SetName("Attacking");
    }

    public override void OnEnter()
    {
        base.OnEnter();

        Machine.StartCoroutine(WaitAndIdle());
    }

    public override void OnExit()
    {
        base.OnExit();
    }

    private IEnumerator WaitAndIdle()
    {
        yield return new WaitForSeconds(0.25f);
        Machine.EndState(Machine.GetState<ChildStateIdle>());
    }
}
