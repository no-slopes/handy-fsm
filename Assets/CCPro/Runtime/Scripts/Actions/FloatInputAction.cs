using UnityEngine;
using UnityEngine.InputSystem;

namespace IndieGabo.HandyFSM.CCPro
{

    /// <summary>
    /// This struct contains all the button states, which are updated frame by frame.
    /// </summary>
    [System.Serializable]
    public class FloatInputAction
    {
        /// <summary>
        /// The action current value.
        /// </summary>
        public float value;


        InputAction _inputAction;

        /// <summary>
        /// Resets the action.
        /// </summary>
        public void Reset()
        {
            value = 0f;
        }

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
            value = context.ReadValue<float>();
        }

        private void OnCanceled(InputAction.CallbackContext context)
        {
            value = 0f;
        }
    }
}