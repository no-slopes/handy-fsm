using UnityEngine;
using UnityEngine.InputSystem;

namespace IndieGabo.HandyFSM.CCPro
{

    [System.Serializable]
    public struct Vector2InputAction
    {
        /// <summary>
        /// The action current value.
        /// </summary>
        public Vector2 value;

        private InputAction _inputAction;

        /// <summary>
        /// Resets the action
        /// </summary>
        public void Reset()
        {
            value = Vector2.zero;
        }


        /// <summary>
        /// Returns true if the value is not equal to zero (e.g. When pressing a D-pad)
        /// </summary>
        public readonly bool Detected => value != Vector2.zero;

        /// <summary>
        /// Returns true if the x component is positive.
        /// </summary>
        public readonly bool Right => value.x > 0;

        /// <summary>
        /// Returns true if the x component is negative.
        /// </summary>
        public readonly bool Left => value.x < 0;

        /// <summary>
        /// Returns true if the y component is positive.
        /// </summary>
        public readonly bool Up => value.y > 0;

        /// <summary>
        /// Returns true if the y component is negative.
        /// </summary>
        public readonly bool Down => value.y < 0;

        public void Initialize(InputAction inputAction)
        {
            _inputAction = inputAction;
            _inputAction.performed += OnPerformed;
            _inputAction.canceled += OnCanceled;
        }

        public void Dismiss()
        {
            _inputAction.performed -= OnPerformed;
            _inputAction.canceled -= OnCanceled;
        }

        private void OnPerformed(InputAction.CallbackContext context)
        {
            value = context.ReadValue<Vector2>().normalized;
        }

        private void OnCanceled(InputAction.CallbackContext context)
        {
            value = Vector2.zero;
        }
    }
}