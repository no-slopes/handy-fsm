using System;
using System.Collections.Generic;
using Lightbug.CharacterControllerPro.Implementation;
using UnityEngine;
using UnityEngine.InputSystem;

namespace IndieGabo.HandyFSM.CCPro
{
    /// <summary>
    /// Resolves Character Controller Pro actions through explicit
    /// InputActionReference fields so each binding remains stable even when
    /// different maps expose actions with identical names.
    /// </summary>
    public sealed class GaboInputHandler : InputHandler
    {
        #region Constants

        private const string MovementActionName = "Movement";
        private const string JumpActionName = "Jump";
        private const string CrouchActionName = "Crouch";
        private const string RunActionName = "Run";
        private const string DashActionName = "Dash";
        private const string InteractActionName = "Interact";
        private const string JetPackActionName = "Jet Pack";
        private const string PitchActionName = "Pitch";
        private const string RollActionName = "Roll";

        #endregion

        #region Inspector

        [SerializeField]
        private PlayerInput _playerInput;

        [SerializeField]
        private InputActionReference _movementActionRef;

        [SerializeField]
        private InputActionReference _jumpActionRef;

        [SerializeField]
        private InputActionReference _crouchActionRef;

        [SerializeField]
        private InputActionReference _runActionRef;

        [SerializeField]
        private InputActionReference _dashActionRef;

        [SerializeField]
        private InputActionReference _interactActionRef;

        [SerializeField]
        private InputActionReference _jetPackActionRef;

        [SerializeField]
        private InputActionReference _pitchActionRef;

        [SerializeField]
        private InputActionReference _rollActionRef;

        #endregion

        #region Fields

        private readonly Dictionary<string, InputAction> _inputActions =
            new(9, StringComparer.Ordinal);

        private readonly HashSet<string> _missingActions =
            new(StringComparer.Ordinal);

        #endregion

        #region Unity Messages

        /// <summary>
        /// Resolves the PlayerInput source and caches every action required by
        /// the CharacterBrain.
        /// </summary>
        private void Awake()
        {
            ResolvePlayerInput();
            CacheActions();
        }

        #endregion

        #region InputHandler

        /// <summary>
        /// Reads the current pressed state for a button action.
        /// </summary>
        /// <param name="actionName">The Character Controller Pro action name.</param>
        /// <returns>True while the mapped action is pressed.</returns>
        public override bool GetBool(string actionName)
        {
            return TryGetAction(actionName, out InputAction action)
                && action.IsPressed();
        }

        /// <summary>
        /// Reads the scalar value associated with an action.
        /// </summary>
        /// <param name="actionName">The Character Controller Pro action name.</param>
        /// <returns>The current scalar value.</returns>
        public override float GetFloat(string actionName)
        {
            return TryGetAction(actionName, out InputAction action)
                ? action.ReadValue<float>()
                : default;
        }

        /// <summary>
        /// Reads the vector value associated with an action.
        /// </summary>
        /// <param name="actionName">The Character Controller Pro action name.</param>
        /// <returns>The current vector value.</returns>
        public override Vector2 GetVector2(string actionName)
        {
            return TryGetAction(actionName, out InputAction action)
                ? action.ReadValue<Vector2>()
                : default;
        }

        #endregion

        #region Resolution

        /// <summary>
        /// Finds the PlayerInput instance used by the sample scene.
        /// </summary>
        private void ResolvePlayerInput()
        {
            if (_playerInput != null)
            {
                return;
            }

            _playerInput = GetComponent<PlayerInput>();

            if (_playerInput == null)
            {
                _playerInput = GetComponentInParent<PlayerInput>();
            }

            if (_playerInput == null)
            {
                _playerInput = FindAnyObjectByType<PlayerInput>();
            }

            if (_playerInput == null)
            {
                Debug.LogError(
                    "GaboInputHandler requires a PlayerInput component in the scene.",
                    this);
                enabled = false;
            }
        }

        /// <summary>
        /// Caches all actions required by the character brain.
        /// </summary>
        private void CacheActions()
        {
            _inputActions.Clear();

            if (_playerInput == null || _playerInput.actions == null)
            {
                return;
            }

            CacheAction(MovementActionName, _movementActionRef);
            CacheAction(JumpActionName, _jumpActionRef);
            CacheAction(CrouchActionName, _crouchActionRef);
            CacheAction(RunActionName, _runActionRef);
            CacheAction(DashActionName, _dashActionRef);
            CacheAction(InteractActionName, _interactActionRef);
            CacheAction(JetPackActionName, _jetPackActionRef);
            CacheAction(PitchActionName, _pitchActionRef);
            CacheAction(RollActionName, _rollActionRef);
        }

        /// <summary>
        /// Resolves a single action through a serialized reference and stores it
        /// in the local cache.
        /// </summary>
        /// <param name="actionName">The Character Controller Pro action name.</param>
        /// <param name="actionReference">The explicit action reference.</param>
        private void CacheAction(
            string actionName,
            InputActionReference actionReference)
        {
            if (actionReference == null || actionReference.action == null)
            {
                PrintMissingReferenceWarning(actionName);
                return;
            }

            InputAction action = _playerInput.actions.FindAction(
                actionReference.action.id.ToString(),
                false);

            if (action == null)
            {
                PrintMissingResolvedActionWarning(actionName);
                return;
            }

            _inputActions[actionName] = action;
        }

        /// <summary>
        /// Tries to retrieve a cached action.
        /// </summary>
        /// <param name="actionName">The action name to retrieve.</param>
        /// <param name="action">The resolved action.</param>
        /// <returns>True when the action exists in the cache.</returns>
        private bool TryGetAction(string actionName, out InputAction action)
        {
            bool found = _inputActions.TryGetValue(actionName, out action);

            if (!found)
            {
                PrintMissingResolvedActionWarning(actionName);
            }

            return found;
        }

        /// <summary>
        /// Logs a missing action reference warning only once per action name.
        /// </summary>
        /// <param name="actionName">The unresolved action name.</param>
        private void PrintMissingReferenceWarning(string actionName)
        {
            if (!_missingActions.Add($"ref:{actionName}"))
            {
                return;
            }

            Debug.LogWarning(
                $"Input action reference for '{actionName}' is not assigned.",
                this);
        }

        /// <summary>
        /// Logs a missing cached action warning only once per action name.
        /// </summary>
        /// <param name="actionName">The unresolved action name.</param>
        private void PrintMissingResolvedActionWarning(string actionName)
        {
            if (!_missingActions.Add($"action:{actionName}"))
            {
                return;
            }

            Debug.LogWarning(
                $"Input action '{actionName}' could not be resolved from the assigned PlayerInput asset.",
                this);
        }

        #endregion
    }
}