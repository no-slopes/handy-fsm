using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildStateRunnning : ChildStateGrounded
{
    protected void OnInit()
    {
        SetInterruptible(true);
        SetName("Running");
    }

    public void OnEnter()
    {
        Brain.StartCoroutine(WaitAndAttack());
    }

    public void OnExit()
    {
    }

    private IEnumerator WaitAndAttack()
    {
        yield return new WaitForSeconds(0.25f);
        Brain.EndState(Brain.GetState<ChildStateAttacking>());
    }
}
