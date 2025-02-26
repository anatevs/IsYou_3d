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

        public Vector3 MoveDirection => _moveDirection;

        public Vector3 MovePosition => _movePosition;

        public bool IsMoving => _isMoving;

        public bool IsMovingPos => _isMovingPos;

        private Camera _camera;

        private Plane _groundPlane;

        //a field of input and isPlaying listener to switch input action maps

        private PlayerInput _playerInput;

        private Vector3 _moveDirection;

        private Vector3 _movePosition;

        private bool _isMoving;

        private bool _isMovingPos;

        private static Vector3 _zeroVector = Vector3.zero;

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
            _camera = Camera.main;
            _groundPlane = new Plane(Vector3.up, 0);
            _moveDirection = _zeroVector;
        }

        private void OnEnable()
        {
            _playerInput.onActionTriggered += OnMoveUpContinued;
            _playerInput.onActionTriggered += OnMoveDownContinued;
            _playerInput.onActionTriggered += OnMoveLeftContinued;
            _playerInput.onActionTriggered += OnMoveRightContinued;
            _playerInput.onActionTriggered += OnClickPositionContinued;

            _playerInput.onActionTriggered += OnClickPosition;
            _playerInput.onActionTriggered += OnMoveUp;
            _playerInput.onActionTriggered += OnMoveDown;
            _playerInput.onActionTriggered += OnMoveLeft;
            _playerInput.onActionTriggered += OnMoveRight;
        }

        private void OnDisable()
        {
            _playerInput.onActionTriggered -= OnMoveUpContinued;
            _playerInput.onActionTriggered -= OnMoveDownContinued;
            _playerInput.onActionTriggered -= OnMoveLeftContinued;
            _playerInput.onActionTriggered -= OnMoveRightContinued;
            _playerInput.onActionTriggered -= OnClickPositionContinued;

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

        private void OnClickPositionContinued(InputAction.CallbackContext context)
        {
            if (context.action.name == "PositionMove")
            {
                if (context.started)
                {
                    Debug.Log("pos move started");

                    var clickPositionScreen = context.ReadValue<Vector2>();

                    var ray = _camera.ScreenPointToRay(clickPositionScreen);

                    if (_groundPlane.Raycast(ray, out var distance))
                    {
                        _isMovingPos = true;
                        _movePosition = ray.GetPoint(distance);

                        Debug.Log($"is ray to {_movePosition}");
                    }
                }

                else if (context.canceled)
                {
                    Debug.Log("pos move ended");

                    _isMovingPos = false;
                    _movePosition = _zeroVector;
                }
            }
        }


        private void OnMoveUpContinued(InputAction.CallbackContext context)
        {
            if (context.action.name == "MoveUpValue")
            {
                if (context.started)
                {
                    _isMoving = true;
                    _moveDirection = Vector3.forward;
                }
                else if (context.canceled)
                {
                    _isMoving = false;
                    _moveDirection = _zeroVector;
                }
            }
        }

        private void OnMoveDownContinued(InputAction.CallbackContext context)
        {
            if (context.action.name == "MoveDownValue")
            {
                if (context.started)
                {
                    _isMoving = true;
                    _moveDirection = Vector3.back;
                }
                else if (context.canceled)
                {
                    _isMoving = false;
                    _moveDirection = _zeroVector;
                }
            }
        }

        private void OnMoveLeftContinued(InputAction.CallbackContext context)
        {
            if (context.action.name == "MoveLeftValue")
            {
                if (context.started)
                {
                    _isMoving = true;
                    _moveDirection = Vector3.left;
                }
                else if (context.canceled)
                {
                    _isMoving = false;
                    _moveDirection = _zeroVector;
                }
            }
        }

        private void OnMoveRightContinued(InputAction.CallbackContext context)
        {
            if (context.action.name == "MoveRightValue")
            {
                if (context.started)
                {
                    _isMoving = true;
                    _moveDirection = Vector3.right;
                }
                else if (context.canceled)
                {
                    _isMoving = false;
                    _moveDirection = _zeroVector;
                }
            }
        }
    }
}