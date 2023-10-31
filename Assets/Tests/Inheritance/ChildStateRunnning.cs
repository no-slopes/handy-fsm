using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildStateRunnning : ChildStateGrounded
{
    protected override void OnInit()
    {
        base.OnInit();
        SetInterruptible(true);
        SetName("Running");
    }

    public override void OnEnter()
    {
        base.OnEnter();

        Machine.StartCoroutine(WaitAndAttack());
    }

    public override void OnExit()
    {
        base.OnExit();
    }

    private IEnumerator WaitAndAttack()
    {
        yield return new WaitForSeconds(0.25f);
        Machine.EndState(Machine.GetState<ChildStateAttacking>());
    }
}
