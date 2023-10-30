using System;
using HandyFSM;
using Sirenix.OdinInspector;
using UnityEngine;

public class Child : StateMachine
{
    [BoxGroup("Test")]
    [SerializeField]
    private float _testFloat;

    public Type LoadableStateType => typeof(ChildState);
    public Type DefaultStateType => typeof(ChildStateIdle);
}
