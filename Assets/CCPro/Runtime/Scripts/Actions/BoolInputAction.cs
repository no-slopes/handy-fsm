using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace IndieGabo.HandyFSM.CCPro
{

    /// <summary>
    /// This struct contains all the button states, which are updated frame by frame.
    /// </summary>
    [System.Serializable]
    public class BoolInputAction
    {
        /// <summary>
        /// The action current value.
        /// </summary>
        public bool value;

        /// <summary>
        /// Returns true if the value transitioned from false to true (e.g. a button press).
        /// </summary>
        public bool Started { get; private set; }

        /// <summary>
        /// Returns true if the value transitioned from true to false (e.g. a button release).
        /// </summary>
        public bool Canceled { get; private set; }

        /// <summary>
        /// Elapsed time since the last "Started" flag.
        /// </summary>
        public float StartedElapsedTime => Time.time - _performedAt;

        /// <summary>
        /// Elapsed time since the last "Canceled" flag.
        /// </summary>
        public float CanceledElapsedTime => Time.time - _canceledAt;

        InputAction _inputAction;

        float _performedAt;
        float _canceledAt;

        /// <summary>
        /// Initialize the values.
        /// </summary>
        public void Initialize(InputAction inputAction)
        {
            _inputAction = inputAction;
            value = false;

            _inputAction.performed += OnPerformed;
            _inputAction.canceled += OnCanceled;
        }

        public void Dismiss()
        {
            _inputAction.performed -= OnPerformed;
            _inputAction.canceled -= OnCanceled;
        }

        /// <summary>
        /// Resets the action.
        /// </summary>
        public void Reset()
        {
            Started = false;
            Canceled = false;
        }

        private void OnPerformed(InputAction.CallbackContext context)
        {
            value = true;

            _performedAt = Time.time;
            _canceledAt = Mathf.Infinity;
            Started = true;
            Canceled = false;
        }

        private void OnCanceled(InputAction.CallbackContext context)
        {
            value = false;

            _performedAt = Mathf.Infinity;
            _canceledAt = Time.time;
            Canceled = true;
            Started = false;
        }
    }
}