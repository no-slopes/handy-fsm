using UnityEngine;

namespace IndieGabo.HandyFSM.CCPro
{
    public class CharacterActions : MonoBehaviour
    {
        #region Inspector

        #endregion

        #region Fields

        private Vector2 _movementInput;

        private bool _jumpBeingPerformed;
        private float _jumpPerformedAt;
        private float _jumpCanceledAt;

        #endregion

        #region Getters

        public bool WantsToMove => _movementInput.x != 0;
        public bool WantsToCrouch => _movementInput.y < 0;

        public Vector2 MovementInput => _movementInput;

        public bool WantsToJump => _jumpBeingPerformed;
        public float JumpElapsedTime => Time.time - _jumpPerformedAt;
        public float JumpCancelElapsedTime => Time.time - _jumpCanceledAt;

        #endregion
    }
}