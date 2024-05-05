using UnityEngine;

namespace HandyFSM
{
    public interface IState
    {
        bool Interruptible { get; }
        string Name { get; }
        HandyFSMBrain Brain { get; }

        bool ShouldTransition(out IState target);

        void Initialize(HandyFSMBrain machine);

        void Enter();
        void Exit();
        void Tick();
        void FixedTick();
        void LateTick();
    }
}