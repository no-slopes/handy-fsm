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

        void OnCollisionEnter2D(Collision2D collision);
        void OnCollisionStay2D(Collision2D collision);
        void OnCollisionExit2D(Collision2D collision);
        void OnTriggerEnter2D(Collider2D other);
        void OnTriggerStay2D(Collider2D other);
        void OnTriggerExit2D(Collider2D other);
    }
}