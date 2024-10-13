using System.Collections.Generic;
using Lightbug.CharacterControllerPro.Implementation;
using UnityEngine;
using UnityEngine.InputSystem;

namespace IndieGabo.HandyFSM.CCPro
{
    public class GaboInputHandler : InputHandler
    {
        [SerializeField] PlayerInput _playerInput;

        [SerializeField] InputActionReference _movementActionRef;
        [SerializeField] InputActionReference _jumpActionRef;
        [SerializeField] InputActionReference _crouchActionRef;
        [SerializeField] InputActionReference _runActionRef;
        [SerializeField] InputActionReference _dashActionRef;
        [SerializeField] InputActionReference _interactActionRef;
        [SerializeField] InputActionReference _jetPackActionRef;
        [SerializeField] InputActionReference _pitchActionRef;
        [SerializeField] InputActionReference _rollActionRef;

        private Dictionary<string, InputAction> _inputActions;

        private void Awake()
        {
            _inputActions = new Dictionary<string, InputAction>();

            InputAction movementAction = _playerInput.actions.FindAction(_movementActionRef.action.id);
            _inputActions.Add(movementAction.name, movementAction);

            InputAction jumpAction = _playerInput.actions.FindAction(_jumpActionRef.action.id);
            _inputActions.Add(jumpAction.name, jumpAction);

            InputAction crouchAction = _playerInput.actions.FindAction(_crouchActionRef.action.id);
            _inputActions.Add(crouchAction.name, crouchAction);

            InputAction runAction = _playerInput.actions.FindAction(_runActionRef.action.id);
            _inputActions.Add(runAction.name, runAction);

            InputAction dashAction = _playerInput.actions.FindAction(_dashActionRef.action.id);
            _inputActions.Add(dashAction.name, dashAction);

            InputAction interactAction = _playerInput.actions.FindAction(_interactActionRef.action.id);
            _inputActions.Add(interactAction.name, interactAction);

            InputAction jetPackAction = _playerInput.actions.FindAction(_jetPackActionRef.action.id);
            _inputActions.Add(jetPackAction.name, jetPackAction);

            InputAction pitchAction = _playerInput.actions.FindAction(_pitchActionRef.action.id);
            _inputActions.Add(pitchAction.name, pitchAction);

            InputAction rollAction = _playerInput.actions.FindAction(_rollActionRef.action.id);
            _inputActions.Add(rollAction.name, rollAction);
        }


        public override bool GetBool(string actionName)
        {
            if (!_inputActions.ContainsKey(actionName))
            {
                Debug.LogError("Input Action not found: " + actionName, this);
                return false;
            }

            return _inputActions[actionName].WasPerformedThisFrame();
        }

        public override float GetFloat(string actionName)
        {
            if (!_inputActions.ContainsKey(actionName))
            {
                Debug.LogError("Input Action not found: " + actionName, this);
                return default;
            }

            return _inputActions[actionName].ReadValue<float>();
        }

        public override Vector2 GetVector2(string actionName)
        {
            if (!_inputActions.ContainsKey(actionName))
            {
                Debug.LogError("Input Action not found: " + actionName, this);
                return default;
            }

            return _inputActions[actionName].ReadValue<Vector2>();
        }
    }
}