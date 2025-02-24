using UnityEngine;

namespace GameCore
{
    public interface IGridCalculations
    {
        public (int x, int y) MapSize { get; }

        public Vector2Int GetTileIndex(Vector3 position);
    }
}