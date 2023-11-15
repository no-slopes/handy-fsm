using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildStateAttacking : ChildState
{
    protected void OnInit()
    {
        SetInterruptible(true);
        SetName("Attacking");
    }

    public void OnEnter()
    {
        Machine.StartCoroutine(WaitAndIdle());
    }

    public void OnExit()
    {
    }

    private IEnumerator WaitAndIdle()
    {
        yield return new WaitForSeconds(0.25f);
        Machine.EndState(Machine.GetState<ChildStateIdle>());
    }
}
