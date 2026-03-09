using UnityEngine;

namespace Scripts
{
    public class BlockSpawner : MonoBehaviour
    {
        [SerializeField] private BlockPool _blockPoolPrefab;
        [SerializeField] private Board boardPrefab;
        private BlockPool _blockPool;
        private Board _board;
        public Board Board => _board;
        public void Init()
        {
            _blockPool = Instantiate(_blockPoolPrefab, transform);
            _blockPool.Init();
            
            _board = Instantiate(boardPrefab, transform);
            _board.Init();
        }

        public bool SpawnBlock(int colorId, Vector2Int boardPos)
        {
            return SpawnBlock(colorId, Direction.Right, boardPos);
        }

        public bool SpawnBlock(int colorId, Direction dir, Vector2Int boardPos)
        {
            if (_board == null)
            {
                return false;
            }

            if (!_board.IsEmpty(boardPos))
            {
                return false;
            }

            Block block = _blockPool.GetBlock(colorId);
            if (block == null)
            {
                return false;
            }

            block.SetBoardManager(_board);
            block.SetDirection(dir);

            Vector3 worldPos = _board.BoardToWorld(boardPos, 0f);
            block.transform.position = worldPos;
            block.gameObject.SetActive(true);
            
            if (!_board.TryPlaceBlock(block, boardPos))
            {
                block.ReturnToPool();
                return false;
            }

            return true;
        }
    }
}