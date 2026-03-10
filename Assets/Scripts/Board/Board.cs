using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts
{
    public class Board : MonoBehaviour
    {
        [Min(1)] public int width = 6;
        [Min(1)] public int height = 6;

        public Block[,] board;
        [Min(0.01f)] public float cellSize = 1f;
        [Min(0.01f)] public float gridSpacing = 1f;
        [SerializeField, Min(1f)] private float _exitDistanceInCells = 20f;

        public event Action OnMoveCommitted;
        public event Action OnBoardCleared;

        private bool _inputEnabled = true;
        private int _pendingExitReturns;
        private int _stateVersion;

        private void Awake()
        {
            Init();
        }

        public void ConfigureSize(int newWidth, int newHeight)
        {
            width = newWidth;
            height = newHeight;
        }

        public void SetCellLayout(float newCellSize, float newGridSpacing)
        {
            cellSize = newCellSize;
            gridSpacing = newGridSpacing;
        }

        public void SetInputEnabled(bool enabled)
        {
            _inputEnabled = enabled;
        }

        public void Init()
        {
            board = new Block[width, height];
            _pendingExitReturns = 0;
            _stateVersion++;
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
            {
                return null;
            }

            return board[x, y];
        }

        public bool TryPlaceBlock(Block block, Vector2Int pos)
        {
            if (!IsEmpty(pos))
            {
                return false;
            }

            board[pos.x, pos.y] = block;
            block.SetBoardPos(pos);
            block.SetBoard(this);
            return true;
        }

        public List<Vector2Int> GetEmptyPositions()
        {
            var result = new List<Vector2Int>();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (board[x, y] == null)
                    {
                        result.Add(new Vector2Int(x, y));
                    }
                }
            }

            return result;
        }

        public void Clear()
        {
            if (board == null)
            {
                return;
            }

            _stateVersion++;
            _pendingExitReturns = 0;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var block = board[x, y];
                    if (block == null)
                    {
                        continue;
                    }

                    board[x, y] = null;
                    block.ReturnToPool();
                }
            }
        }

        public Vector3 BoardToWorld(Vector2Int boardPos, float z = 0f)
        {
            float halfWidth = (width - 1) * 0.5f;
            float halfHeight = (height - 1) * 0.5f;

            float worldX = transform.position.x + (boardPos.x - halfWidth) * gridSpacing;
            float worldY = transform.position.y + (boardPos.y - halfHeight) * gridSpacing;
            return new Vector3(worldX, worldY, z);
        }

        public bool HasAnyBlock()
        {
            if (board == null)
            {
                return false;
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (board[x, y] != null)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void NotifyMoveCommitted()
        {
            OnMoveCommitted?.Invoke();
        }

        private void TryNotifyBoardCleared()
        {
            if (_pendingExitReturns == 0 && !HasAnyBlock())
            {
                OnBoardCleared?.Invoke();
            }
        }

        private void HandleExitReturn(Block block, int callbackStateVersion)
        {
            if (callbackStateVersion != _stateVersion)
            {
                return;
            }

            block.ReturnToPool();
            _pendingExitReturns = Mathf.Max(0, _pendingExitReturns - 1);
            TryNotifyBoardCleared();
        }

        public bool TrySlideBlock(Block block)
        {
            if (!_inputEnabled || block == null || block.IsMoving)
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

            if (IsInside(next) && !IsEmpty(next))
            {
                NotifyMoveCommitted();
                Block blocker = board[next.x, next.y];
                blocker?.PlayBlockedFeedback();
                return false;
            }

            Vector2Int to = from;

            while (IsInside(next) && IsEmpty(next))
            {
                to = next;
                next += step;
            }

            if (!IsInside(next))
            {
                float farExitDistance = gridSpacing * Mathf.Max(1f, _exitDistanceInCells);
                Vector3 exitTarget = BoardToWorld(to, block.transform.position.z) + new Vector3(step.x, step.y, 0f) * farExitDistance;

                board[from.x, from.y] = null;
                block.SetBoardPos(new Vector2Int(-1, -1));

                _pendingExitReturns++;
                int callbackStateVersion = _stateVersion;
                if (!block.PlaySlideTo(exitTarget, () => HandleExitReturn(block, callbackStateVersion)))
                {
                    _pendingExitReturns = Mathf.Max(0, _pendingExitReturns - 1);
                    board[from.x, from.y] = block;
                    block.SetBoardPos(from);
                    return false;
                }

                NotifyMoveCommitted();
                return true;
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

            NotifyMoveCommitted();
            TryNotifyBoardCleared();
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