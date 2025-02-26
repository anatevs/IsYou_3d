using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using DG.Tweening;

namespace GameCore
{
    public class PlaiyingElement : MonoBehaviour
    {
        public ElementType Type
        {
            get => _type; set => _type = value;
        }

        public string Id { get => _id; }

        public bool IsInteractable
        {
            get => _isInteractable; set => _isInteractable = value;
        }

        public bool IsMovable
        {
            get => _isMovable; set => _isMovable = value;
        }

        [SerializeField]
        private PlayingGrid _playingGrid;

        [SerializeField]
        private ElementType _type;

        [SerializeField]
        private string _id;

        [SerializeField]
        private GameObject _viewPrefab;

        [SerializeField]
        private bool _isInteractable;

        [SerializeField]
        private bool _isMovable;

        private float _moveDuration = 1f;

        private Vector2Int _currentIndex;

        private bool _isAtMoveTween;

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
                var moveSequence = DOTween.Sequence().Pause();

                foreach (var movable in movableElements)
                {
                    movable.Element.MoveIndex(movable.Index);

                    moveSequence.Join(movable.Element.MoveView(movable.Position, _moveDuration));
                }

                moveSequence.Play();
            }
        }

        public void MakeMoveProcess(bool isMove, Vector3 direction)//async: 1st make move with anim, 2nd: check if win
        {
            if (isMove)
            {
                if (!_isAtMoveTween)
                {
                    Debug.Log($"to next cell");

                    _isAtMoveTween = true;

                    List<MovableInfo> movableElements = new();

                    if (!CanMove(direction, ref movableElements))
                    {
                        Debug.Log("cannot move");
                        return;
                    }

                    else
                    {
                        Debug.Log("can move");

                        var moveSequence = DOTween
                            .Sequence()
                            .Pause()
                            .OnComplete(SetEndMoveTween);

                        foreach (var movable in movableElements)
                        {
                            movable.Element.MoveIndex(movable.Index);

                            moveSequence.Join(movable.Element.MoveView(movable.Position, _moveDuration));
                        }

                        moveSequence.Play();
                    }
                }
            }
        }

        private void SetEndMoveTween()
        {
            _isAtMoveTween = false;
            Debug.Log("end tween");
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

        private void MoveIndex(Vector2Int newIndex)
        {
            _playingGrid.RemoveElement(_currentIndex, this);
            _playingGrid.AddElement(newIndex, this);

            _currentIndex = newIndex;

            CheckWinInSet();
            UpdateNeighbors();
        }

        private Tween MoveView(Vector3 newPos, float duration)
        {
            return transform.DOMove(newPos, duration).Pause();
        }

        private Vector2Int GetNewIndex(Vector3 newPosition)
        {
            return _playingGrid.FromPositionToIndex(newPosition);
        }

        private Vector3 GetNewPosition(Vector3 direction)
        {
            Debug.Log(direction);
            Debug.Log(transform.position);
            Debug.Log(direction + transform.position);
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