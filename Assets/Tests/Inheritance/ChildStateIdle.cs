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

        Debug.Log("ChildStateIdle OnEnter");
    }
}
