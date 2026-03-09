using UnityEngine;

namespace Scripts
{
    [CreateAssetMenu(fileName = "BlockData", menuName = "Block/Block Data")]
    public class BlockData : ScriptableObject
    {
        public int colorId;
        public Sprite sprite;
    }
}