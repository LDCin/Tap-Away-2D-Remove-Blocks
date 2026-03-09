using System;
using UnityEngine;

namespace Scripts
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private BlockSpawner _blockSpawnerPrefab;

        private BlockSpawner _blockSpawner;
        private Board _board;

        private void Start()
        {
            _blockSpawner = Instantiate(_blockSpawnerPrefab, transform);
            _blockSpawner.Init();
            _board = _blockSpawner.Board;

            ClearBoard();
        }

        public void LoadLevel(int levelNumber)
        {
            ClearBoard();
        }

        public void NextLevel()
        {
        }

        public void RetryLevel()
        {
            ClearBoard();
        }

        private void ClearBoard()
        {
            if (_board == null || _board.board == null)
            {
                return;
            }

            for (int x = 0; x < _board.width; x++)
            {
                for (int y = 0; y < _board.height; y++)
                {
                    Block block = _board.GetBlock(x, y);
                    if (block != null)
                    {
                        block.ReturnToPool();
                        _board.board[x, y] = null;
                    }
                }
            }
        }
    }
}