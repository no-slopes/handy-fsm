using System.Collections.Generic;
using UnityEngine;

namespace IndieGabo.HandyFSM
{
    public interface IState
    {
        string Key { get; }
        string DisplayName { get; }
        FSMBrain Brain { get; }

        bool WantsToTransition(out List<IState> targets);
        bool CanEnter(IState from);
        void Initialize(FSMBrain machine);
        void Enter();
        void Exit();
        void Tick();
        void FixedTick();
        void LateTick();
    }
}