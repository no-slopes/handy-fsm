using UnityEngine;

namespace IndieGabo.HandyFSM.CCPro
{
    public interface ICCProState : IState
    {
        void PreCharacterSimulation(float dt);
        void PostCharacterSimulation(float dt);

        void PreFixedTick();
        void PostFixedTick();
        void TickIK(int layerIndex);
    }
}