using System;
using IndieGabo.HandyFSM.Implementations;
using UnityEngine;

public class Child : GenericHandyFSMBrain<ChildState, ChildStateIdle>
{
    [SerializeField]
    private float _testFloat;
}
