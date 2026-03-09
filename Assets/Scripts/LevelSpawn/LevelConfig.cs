using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts
{
    public enum BlockShape
    {
        Single,
        Line2Horizontal,
        Line2Vertical,
        Square2
    }

    [Serializable]
    public class LevelBlockSpawn
    {
        public int colorId;
        public Direction direction = Direction.Right;
        public Vector2Int boardPos;
        public BlockShape shape = BlockShape.Single;
    }

    [Serializable]
    public class LevelConfig
    {
        [Min(1)] public int width = 6;
        [Min(1)] public int height = 6;
        [Min(0f)] public float screenPadding = 1.2f;
        [Range(0.5f, 1f)] public float blockFillRatio = 0.95f;
        [Min(1)] public int moveLimit = 20;
        public List<LevelBlockSpawn> blocks = new List<LevelBlockSpawn>();

        public bool IsInside(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
        }

        public bool TryGetBlockAt(Vector2Int pos, out LevelBlockSpawn block)
        {
            if (blocks != null)
            {
                for (int i = 0; i < blocks.Count; i++)
                {
                    if (blocks[i].boardPos == pos)
                    {
                        block = blocks[i];
                        return true;
                    }
                }
            }

            block = null;
            return false;
        }

        public void SetBlockAt(Vector2Int pos, int colorId, Direction direction, BlockShape shape)
        {
            if (!IsInside(pos))
            {
                return;
            }

            if (blocks == null)
            {
                blocks = new List<LevelBlockSpawn>();
            }

            for (int i = 0; i < blocks.Count; i++)
            {
                if (blocks[i].boardPos == pos)
                {
                    blocks[i].colorId = colorId;
                    blocks[i].direction = direction;
                    blocks[i].shape = shape;
                    return;
                }
            }

            blocks.Add(new LevelBlockSpawn
            {
                boardPos = pos,
                colorId = colorId,
                direction = direction,
                shape = shape
            });
        }

        public bool RemoveBlockAt(Vector2Int pos)
        {
            if (blocks == null)
            {
                return false;
            }

            for (int i = blocks.Count - 1; i >= 0; i--)
            {
                if (blocks[i].boardPos == pos)
                {
                    blocks.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public void ClampAndDeduplicate()
        {
            if (blocks == null)
            {
                blocks = new List<LevelBlockSpawn>();
                return;
            }

            var occupied = new HashSet<Vector2Int>();
            for (int i = blocks.Count - 1; i >= 0; i--)
            {
                var pos = blocks[i].boardPos;
                if (!IsInside(pos) || occupied.Contains(pos))
                {
                    blocks.RemoveAt(i);
                    continue;
                }

                occupied.Add(pos);
            }
        }

        public static Vector2Int[] GetShapeOffsets(BlockShape shape)
        {
            switch (shape)
            {
                case BlockShape.Line2Horizontal:
                    return new[] { new Vector2Int(0, 0), new Vector2Int(1, 0) };
                case BlockShape.Line2Vertical:
                    return new[] { new Vector2Int(0, 0), new Vector2Int(0, 1) };
                case BlockShape.Square2:
                    return new[]
                    {
                        new Vector2Int(0, 0),
                        new Vector2Int(1, 0),
                        new Vector2Int(0, 1),
                        new Vector2Int(1, 1)
                    };
                default:
                    return new[] { Vector2Int.zero };
            }
        }
    }
}
