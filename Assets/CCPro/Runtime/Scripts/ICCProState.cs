using UnityEngine;

namespace IndieGabo.HandyFSM.CCPro
{
    public interface ICCProState : IState
    {
        bool OverrideAnimatorController { get; }
        RuntimeAnimatorController RuntimeAnimatorController { get; }

        void PreCharacterSimulation(float dt);
        void PostCharacterSimulation(float dt);

        void PreFixedTick();
        void PostFixedTick();
    }
}