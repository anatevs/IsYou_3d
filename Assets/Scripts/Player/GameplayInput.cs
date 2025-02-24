using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameCore
{
    [RequireComponent(typeof(PlayerInput))]
    public sealed class GameplayInput : MonoBehaviour
    {
        public event Action<Vector3> OnMoveDirection;

        public event Action<Vector3> OnMovePosition;

        private Camera _camera;

        private Plane _groundPlane;

        //a field of input and isPlaying listener to switch input action maps

        private PlayerInput _playerInput;

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
            _camera = Camera.main;
            _groundPlane = new Plane(Vector3.up, 0);
        }

        private void OnEnable()
        {
            _playerInput.onActionTriggered += OnClickPosition;
            _playerInput.onActionTriggered += OnMoveUp;
            _playerInput.onActionTriggered += OnMoveDown;
            _playerInput.onActionTriggered += OnMoveLeft;
            _playerInput.onActionTriggered += OnMoveRight;
        }

        private void OnDisable()
        {
            _playerInput.onActionTriggered -= OnClickPosition;
            _playerInput.onActionTriggered -= OnMoveUp;
            _playerInput.onActionTriggered -= OnMoveDown;
            _playerInput.onActionTriggered -= OnMoveLeft;
            _playerInput.onActionTriggered -= OnMoveRight;
        }


        private void OnClickPosition(InputAction.CallbackContext context)
        {
            if (context.action.name == "PositionMove" && context.performed)
            {
                var clickPositionScreen = context.ReadValue<Vector2>();

                var ray = _camera.ScreenPointToRay(clickPositionScreen);

                if (_groundPlane.Raycast(ray, out var distance))
                {
                    var clickedPos = ray.GetPoint(distance);

                    OnMovePosition?.Invoke(clickedPos);
                }
            }
        }

        private void OnMoveUp(InputAction.CallbackContext context)
        {
            if (context.action.name == "MoveUp" && context.performed)
            {
                OnMoveDirection?.Invoke(Vector3.forward);
            }
        }

        private void OnMoveDown(InputAction.CallbackContext context)
        {
            if (context.action.name == "MoveDown" && context.performed)
            {
                OnMoveDirection?.Invoke(Vector3.back);
            }
        }

        private void OnMoveLeft(InputAction.CallbackContext context)
        {
            if (context.action.name == "MoveLeft" && context.performed)
            {
                OnMoveDirection?.Invoke(Vector3.left);
            }
        }

        private void OnMoveRight(InputAction.CallbackContext context)
        {
            if (context.action.name == "MoveRight" && context.performed)
            {
                OnMoveDirection?.Invoke(Vector3.right);
            }
        }
    }
}