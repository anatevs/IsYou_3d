﻿using System;
using UnityEngine;

namespace GameCore
{
    public sealed class Player : MonoBehaviour
    {
        public PlaiyingElement CurrentPlayer => _currentPlayer;

        [SerializeField]
        private GameplayInput _input;

        [SerializeField]
        private PlaiyingElement _currentPlayer;

        [SerializeField]
        private GridCalculations _gridCalculations;

        private void OnEnable()
        {
            _input.OnMoveDirection += MakeMove;
            _input.OnMovePosition += MakeMovePosition;
        }

        private void OnDisable()
        {
            _input.OnMoveDirection -= MakeMove;
            _input.OnMovePosition -= MakeMovePosition;
        }

        public void SetPlayer(PlaiyingElement player)
        {
            _currentPlayer = player;
        }

        private void MakeMovePosition(Vector3 clickedPos)
        {
            if (IsPlayerNull())
            {
                return;
            }

            var clickedTileIndex = _gridCalculations.GetTileIndex(clickedPos);

            var playerTileIndex = _gridCalculations.GetTileIndex(_currentPlayer.transform.position);

            var directionInt = clickedTileIndex - playerTileIndex;

            if (directionInt.x != 0 && directionInt.y != 0)
            {
                return;
            }

            var signX = Math.Sign(directionInt.x);
            var signY = Math.Sign(directionInt.y);

            var direction = new Vector3(signX, 0, signY);

            MakeMove(direction);
        }

        private void MakeMove(Vector3 direction)
        {
            if (IsPlayerNull())
            {
                return;
            }

            _currentPlayer.MakeMove(direction);
        }

        private bool IsPlayerNull()
        {
            if (_currentPlayer == null)
            {
                Debug.Log("<color=yellow>the current player element is null</color>");
                return true;
            }

            return false;
        }
    }
}