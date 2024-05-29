using UnityEngine;

namespace IndieGabo.HandyFSM
{
    public interface IState
    {
        bool Interruptible { get; }
        string Name { get; }
        FSMBrain Brain { get; }

        bool ShouldTransition(out IState target);
        void Initialize(FSMBrain machine);
        void Enter();
        void Exit();
        void Tick();
        void FixedTick();
        void LateTick();
        void TickIK(int layerIndex);
    }
}