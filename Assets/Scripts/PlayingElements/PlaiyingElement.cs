using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

namespace GameCore
{
    public class PlaiyingElement : MonoBehaviour
    {
        [SerializeField]
        private PlayingGrid _playingGrid;


        public ElementType Type;

        public string Id;

        public Sprite Sprite;

        public bool IsInteractable;

        public bool IsMovable;

        private Vector2Int _currentIndex;

        protected Dictionary<(int x, int y), HashSet<PlaiyingElement>> _udlrNeighbors = new()
        {
            { (0, 1), new() },
            { (0, -1), new() },
            { (-1, 0), new() },
            { (1, 0), new() }
        };

        public void SetCurrentIndex(Vector2Int index)
        {
            _currentIndex = index;
        }

        public void MakeMove(Vector3 direction)//async: 1st make move with anim, 2nd: check if win
        {
            List<MovableInfo> movableElements = new();

            if (!CanMove(direction, ref movableElements))
            {
                return;
            }

            else
            {
                foreach (var movable in movableElements)
                {
                    movable.Element.Move(movable.Index, movable.Position);
                }
            }


        }

        private bool CanMove(Vector3 direction, ref List<MovableInfo> obstacleElements)
        {
            var newPosition = GetNewPosition(direction);
            var newIndex = GetNewIndex(newPosition);

            if (!IsMovable)
            {
                return false;
            }

            if (newIndex.x < 0 || newIndex.x >= _playingGrid.MaxX ||
                newIndex.y < 0 || newIndex.y >= _playingGrid.MaxY)
            {
                return false;
            }

            if (_playingGrid.TryGetElement_Index(newIndex, out var obstaclesSet))
            {
                foreach (var obstacleElement in obstaclesSet)
                {
                    if (obstacleElement.IsInteractable &&
                        !obstacleElement.CanMove(direction, ref obstacleElements))
                    {
                        return false;
                    }
                }
            }

            obstacleElements.Add(new MovableInfo
            {
                Element = this,
                Index = newIndex,
                Position = newPosition,
            });

            return true;
        }

        private void Move(Vector2Int newIndex, Vector3 newPos)
        {
            _playingGrid.RemoveElement(_currentIndex, this);
            _playingGrid.AddElement(newIndex, this);

            _currentIndex = newIndex;

            transform.position = newPos;

            CheckWinInSet();
            UpdateNeighbors();
        }

        private Vector2Int GetNewIndex(Vector3 newPosition)
        {
            return _playingGrid.FromPositionToIndex(newPosition);
        }

        private Vector3 GetNewPosition(Vector3 direction)
        {
            return transform.position + direction;
        }

        protected virtual void UpdateNeighbors()
        {
            foreach (var direction in _udlrNeighbors.Keys.ToList())
            {
                var newNeighborSet = GetNeighbor(direction);

                CnangeNeighborSet(direction, newNeighborSet);
            }
        }

        protected HashSet<PlaiyingElement> GetNeighbor((int x, int y) direction)
        {
            var indexX = _currentIndex.x + direction.x;
            var indexY = _currentIndex.y + direction.y;

            var neighborIndex = new Vector2Int(indexX, indexY);

            return _playingGrid.GetElementsSet(neighborIndex);
        }

        public void CnangeNeighborSet((int x, int y) direction, HashSet<PlaiyingElement> neighborSet)
        {
            var oldNeighborSet = _udlrNeighbors[direction];

            if (oldNeighborSet != neighborSet)
            {
                var oppositeDirection = (-direction.x, -direction.y);

                if (oldNeighborSet != null)
                {
                    foreach (var oldNeighbor in oldNeighborSet)
                    {
                        CancelRule(direction, oldNeighbor);

                        oldNeighbor.CancelRule(oppositeDirection, this);
                    }
                }
                else
                {
                    Debug.Log($"{this}'s old neighbor at {direction} is null");
                }

                _udlrNeighbors[direction] = neighborSet;

                if (neighborSet != null)
                {
                    foreach (var neighbor in neighborSet)
                    {
                        SetRule(direction, neighbor);

                        neighbor.SetRule(oppositeDirection, this);
                    }
                }
                else
                {
                    Debug.Log($"{this}'s new neighbor at {direction} is null");
                }
            }
        }

        protected bool TryGetNeighbor((int x, int y) direction, out HashSet<PlaiyingElement> neighborSet)
        {
            var indexX = _currentIndex.x + direction.x;
            var indexY = _currentIndex.y + direction.y;

            var neighborIndex = new Vector2Int(indexX, indexY);

            return _playingGrid.TryGetElement_Index(neighborIndex, out neighborSet);
        }

        public void InitNeighbors()
        {
            foreach (var direction in _udlrNeighbors.Keys.ToList())
            {
                if (TryGetNeighbor(direction, out var neighborElement))
                {
                    CnangeNeighborSet(direction, neighborElement);
                }
            }
        }

        public event Action<PlaiyingElement> OnCheckPlayerAndTarget;

        public void MakeOnCheckWin(PlaiyingElement element)
        {
            OnCheckPlayerAndTarget?.Invoke(element);
        }

        public void CheckWinInSet()
        {
            if (_playingGrid.TryGetElement_Index(_currentIndex, out var elementsSet))
            {
                if (elementsSet.Count > 1)
                {
                    foreach (var element in elementsSet)
                    {
                        if (element != this)
                        {
                            OnCheckPlayerAndTarget?.Invoke(element);
                            element.MakeOnCheckWin(this);
                        }
                    }
                }
            }
        }

        public virtual void SetRule((int x, int y) direction, PlaiyingElement neighbor) { }

        public virtual void CancelRule((int x, int y) direction, PlaiyingElement neighbor) { }

        public virtual void ApplyProperty(PlaiyingElement[] subjectElement) { }

        public virtual void CancelProperty(PlaiyingElement[] subjectElement) { }

        public virtual PlaiyingElement[] GetPointer() { return null; }
    }

    public struct MovableInfo
    {
        public PlaiyingElement Element;
        public Vector2Int Index;
        public Vector3 Position;
    }
}