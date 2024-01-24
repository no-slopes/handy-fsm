using UnityEngine;

namespace HandyFSM
{
    public interface IState
    {
        bool Interruptible { get; }
        string Name { get; }
        StateMachineBehaviour Machine { get; }

        bool ShouldTransition(out IState target);

        void Initialize(StateMachineBehaviour machine);

        void Enter();
        void Exit();
        void Tick();
        void FixedTick();
        void LateTick();
    }
}