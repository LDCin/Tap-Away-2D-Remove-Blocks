using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts
{
    public class Board : MonoBehaviour
    {
        public int width = 6;
        public int height = 6;

        public Block[,] board;
        [Min(0.01f)] public float cellSize = 1f;
        [Min(0.01f)] public float gridSpacing = 1f;

        public void Init()
        {
            board = new Block[width, height];
        }

        public bool IsInside(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
        }

        public bool IsEmpty(Vector2Int pos)
        {
            return IsInside(pos) && board[pos.x, pos.y] == null;
        }

        public Block GetBlock(int x, int y)
        {
            if (!IsInside(new Vector2Int(x, y)))
                return null;
            return board[x, y];
        }

        public Block GetBlock(Vector2Int pos)
        {
            if (!IsInside(pos))
                return null;
            return board[pos.x, pos.y];
        }

        public bool TryPlaceBlock(Block block, Vector2Int pos)
        {
            if (block == null || !IsEmpty(pos)) return false;
            board[pos.x, pos.y] = block;
            block.SetBoardPos(pos);
            return true;
        }

        public void PlaceBlock(Block block, Vector2Int pos)
        {
            TryPlaceBlock(block, pos);
        }

        public List<Vector2Int> GetEmptyPositions()
        {
            var result = new List<Vector2Int>();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (board[x, y] == null)
                        result.Add(new Vector2Int(x, y));
                }
            }
            return result;
        }

        public void TryRemoveBlock(Block block)
        {
            if (CanExit(block))
            {
                RemoveBlock(block);
            }
        }

        void RemoveBlock(Block block)
        {
            Vector2Int pos = block.BoardPos;
            if (IsInside(pos) && board[pos.x, pos.y] == block)
            {
                board[pos.x, pos.y] = null;
            }
            block.ReturnToPool();
        }

        bool CanExit(Block block)
        {
            int x = block.BoardPos.x;
            int y = block.BoardPos.y;

            switch (block.Direction)
            {
                case Direction.Right:

                    for (int i = x + 1; i < width; i++)
                    {
                        if (board[i, y] != null)
                            return false;
                    }

                    return true;

                case Direction.Left:

                    for (int i = x - 1; i >= 0; i--)
                    {
                        if (board[i, y] != null)
                            return false;
                    }

                    return true;

                case Direction.Up:

                    for (int j = y + 1; j < height; j++)
                    {
                        if (board[x, j] != null)
                            return false;
                    }

                    return true;

                case Direction.Down:

                    for (int j = y - 1; j >= 0; j--)
                    {
                        if (board[x, j] != null)
                            return false;
                    }

                    return true;
            }

            return false;
        }

        public Vector3 BoardToWorld(Vector2Int boardPos, float z = 0f)
        {
            float spacing = gridSpacing > 0f ? gridSpacing : cellSize;
            float halfWidth = (width - 1) * 0.5f;
            float halfHeight = (height - 1) * 0.5f;

            float worldX = transform.position.x + (boardPos.x - halfWidth) * spacing;
            float worldY = transform.position.y + (boardPos.y - halfHeight) * spacing;
            return new Vector3(worldX, worldY, z);
        }

        private void Awake()
        {
            if (board == null || board.GetLength(0) != width || board.GetLength(1) != height)
            {
                Init();
            }
        }

        public bool TrySlideBlock(Block block)
        {
            if (board == null)
            {
                return false;
            }

            if (block == null || block.IsMoving)
            {
                return false;
            }

            Vector2Int from = block.BoardPos;
            if (!IsInside(from) || board[from.x, from.y] != block)
            {
                return false;
            }

            Vector2Int step = DirectionToStep(block.Direction);
            Vector2Int next = from + step;
            if (!IsInside(next))
            {
                if (!CanExit(block))
                {
                    return false;
                }

                float spacing = gridSpacing > 0f ? gridSpacing : cellSize;
                Vector3 exitTarget = block.transform.position + new Vector3(step.x, step.y, 0f) * spacing;

                board[from.x, from.y] = null;
                if (!block.PlaySlideTo(exitTarget, () => block.ReturnToPool()))
                {
                    board[from.x, from.y] = block;
                    return false;
                }

                return true;
            }

            if (!IsEmpty(next))
            {
                return false;
            }

            Vector2Int to = from;
            while (IsInside(next) && IsEmpty(next))
            {
                to = next;
                next += step;
            }

            if (to == from)
            {
                return false;
            }

            board[from.x, from.y] = null;
            board[to.x, to.y] = block;
            block.SetBoardPos(to);

            Vector3 worldTarget = BoardToWorld(to, block.transform.position.z);
            if (!block.PlaySlideTo(worldTarget))
            {
                board[to.x, to.y] = null;
                board[from.x, from.y] = block;
                block.SetBoardPos(from);
                return false;
            }

            return true;
        }

        private static Vector2Int DirectionToStep(Direction direction)
        {
            switch (direction)
            {
                case Direction.Right:
                    return Vector2Int.right;
                case Direction.Left:
                    return Vector2Int.left;
                case Direction.Up:
                    return Vector2Int.up;
                case Direction.Down:
                    return Vector2Int.down;
                default:
                    return Vector2Int.zero;
            }
        }
    }
}