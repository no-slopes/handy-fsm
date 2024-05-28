using System;
using IndieGabo.HandyFSM.Implementations;
using Sirenix.OdinInspector;
using UnityEngine;

public class Child : GenericHandyFSMBrain<ChildState, ChildStateIdle>
{
    [BoxGroup("Test")]
    [SerializeField]
    private float _testFloat;
}
