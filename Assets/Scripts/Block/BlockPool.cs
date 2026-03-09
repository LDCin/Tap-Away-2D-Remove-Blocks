using System.Collections.Generic;
using UnityEngine;

namespace Scripts
{
    public class BlockPool : MonoBehaviour
    {
        private List<BlockData> _blockDatas;
        private List<Block> _blocks;
        [SerializeField] private Block _blockPrefab;
        private bool _isInitialized = false;
        public void Init()
        {
            if (_isInitialized) return;
            
            _blockDatas = new List<BlockData>(Resources.LoadAll<BlockData>(GameConfig.BLOCK_DATA_PATH));
            Debug.Log("BlockPool: Loaded BlockData count: " + _blockDatas.Count);
            
            if (_blockDatas.Count == 0)
            {
                return;
            }
            
            _blocks = new List<Block>();
            InitBlockList();
            _isInitialized = true;
            Debug.Log("BlockPool: Initialized with " + _blocks.Count + " blocks");
        }
        private void InitBlockList()
        {
            _blocks = new List<Block>();

            foreach (BlockData blockData in _blockDatas)
            {
                for (int quantity = 0; quantity < 36; quantity++)
                {
                    Block block = Instantiate(_blockPrefab, transform);
                    block.Init(blockData.sprite, blockData.colorId);
                    _blocks.Add(block);
                    block.gameObject.SetActive(false);
                }
            }
        }

        private BlockData GetBlockData(int colorId)
        {
            foreach (var blockData in _blockDatas)
            {
                if (blockData.colorId == colorId)
                {
                    return blockData;
                }
            }

            return null;
        }

        [ContextMenu("Get Blocks")]
        public Block GetBlock(int colorId)
        {
            foreach (var block in _blocks)
            {
                if (!block.gameObject.activeInHierarchy)
                {
                    if (block.ColorID == colorId)
                    {
                        return block;
                    }
                }
            }

            BlockData blockData = GetBlockData(colorId);
            if (blockData == null)
            {
                return null;
            }

            Block newBlock = Instantiate(_blockPrefab, transform);
            newBlock.Init(blockData.sprite, blockData.colorId);
            _blocks.Add(newBlock);

            return newBlock;
        }
    }
}