using System.Collections.Generic;
using UnityEngine;

namespace Scripts
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "Game/Level Data")]
    public class LevelData : ScriptableObject
    {
        [SerializeField] private List<LevelConfig> _levels = new List<LevelConfig>();

        public int Count => _levels.Count;
        public List<LevelConfig> Levels => _levels;

        public bool TryGetLevel(int levelNumber, out LevelConfig level)
        {
            if (levelNumber >= 0 && levelNumber < _levels.Count)
            {
                level = _levels[levelNumber];
                return true;
            }

            level = null;
            return false;
        }

        public void AddLevel()
        {
            _levels.Add(new LevelConfig());
        }

        public void RemoveLevelAt(int index)
        {
            if (index >= 0 && index < _levels.Count)
            {
                _levels.RemoveAt(index);
            }
        }
    }
}
