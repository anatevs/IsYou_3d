using UnityEngine;

namespace GameCore
{
    public class TileGridCalculations : MonoBehaviour,
        IGridCalculations
    {
        public (int x, int y) MapSize => _mapSize;

        private (int x, int y) _mapSize;

        private readonly int[] _mapWH = { 24, 20 };

        private readonly int _cellSize = 1;

        private void Awake()
        {
            _mapSize = (_mapWH[0], _mapWH[1]);
        }

        public Vector2Int GetTileIndex(Vector3 position)
        {
            var x = GetCoordinateInt(position.x);
            var y = GetCoordinateInt(position.z);

            return new Vector2Int(x, y);
        }

        private int GetCoordinateInt(float coordinate)
        {
            var res = coordinate / _cellSize;

            return Mathf.FloorToInt(res);
        }
    }
}