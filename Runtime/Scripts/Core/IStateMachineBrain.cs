using System;
using UnityEngine;

namespace IndieGabo.HandyFSM
{
    public interface IStateMachineBrain
    {
        void Resume();
        void Pause();
        void Stop();

        void RequestStateChange(State state);
        void RequestStateChange<T>() where T : State;

        void EndState(State target = null);
        void EndState<T>() where T : State;

        T GetState<T>() where T : State;
        bool TryGetState<T>(out State state) where T : State;

        T As<T>() where T : MonoBehaviour, IStateMachineBrain;
    }
}