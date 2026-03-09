using System;
using UI.Panels;
using UnityEngine;

namespace Scripts
{
    public class GameManager : MonoBehaviour
    {
        public static event Action<int> OnUpdateCoins;
        public static event Action<int> OnUpdateMoves;
        public static event Action<int> OnUpdateLevel;
        public static event Action OnMoveUsed;
        public static event Action OnGameOverTriggered;
        public static event Action OnWinTriggered;

        [SerializeField] private BlockSpawner _blockSpawnerPrefab;
        [SerializeField] private LevelData _levelData;
        [SerializeField, Min(0)] private int _startLevel;
        [SerializeField] private Camera _targetCamera;
        [SerializeField, Min(0)] private int _winRewardCoins = 10;

        private BlockSpawner _blockSpawner;
        private Board _board;
        private int _currentLevel;
        private int _remainingMoves;
        private int _coins;
        private bool _isLevelEnded;

        public static int CurrentCoins { get; private set; }

        private void Start()
        {
            _blockSpawner = Instantiate(_blockSpawnerPrefab, transform);
            _blockSpawner.Init();
            _board = _blockSpawner.Board;
            _coins = GameConfig.COINS;
            CurrentCoins = _coins;

            SubscribeBoardEvents();
            SubscribeUiEvents();

            LoadLevel(_startLevel);
        }

        private void OnDestroy()
        {
            UnsubscribeBoardEvents();
            UnsubscribeUiEvents();
        }

        public void LoadLevel(int levelNumber)
        {
            if (_board == null || _levelData == null || _levelData.Count == 0)
            {
                return;
            }

            int clampedLevel = Mathf.Clamp(levelNumber, 0, _levelData.Count - 1);
            if (!_levelData.TryGetLevel(clampedLevel, out LevelConfig level))
            {
                return;
            }

            Time.timeScale = 1f;
            _isLevelEnded = false;

            ClearBoard();

            float padding = level.screenPadding;
            float slotSize = ComputeCellSizeForCamera(level.width, level.height, padding);
            float blockVisualSize = slotSize * Mathf.Clamp(level.blockFillRatio, 0.5f, 1f);

            _blockSpawner.ConfigureBoard(level.width, level.height, blockVisualSize, slotSize);
            foreach (var spawn in level.blocks)
            {
                _blockSpawner.SpawnBlock(spawn.colorId, spawn.direction, spawn.boardPos);
            }

            _remainingMoves = Mathf.Max(1, level.moveLimit);
            _board.SetInputEnabled(true);

            _currentLevel = clampedLevel;
            CenterCameraOnBoard();
            OpenGameplayPanel();
            PushHudState();
        }

        [ContextMenu("Next Level")]
        public void NextLevel()
        {
            if (_levelData == null || _levelData.Count == 0)
            {
                return;
            }

            int next = (_currentLevel + 1) % _levelData.Count;
            LoadLevel(next);
        }

        public void RetryLevel()
        {
            LoadLevel(_currentLevel);
        }

        private void HandleMoveCommitted()
        {
            if (_isLevelEnded)
            {
                return;
            }

            _remainingMoves = Mathf.Max(0, _remainingMoves - 1);
            OnUpdateMoves?.Invoke(_remainingMoves);
            OnMoveUsed?.Invoke();

            if (_remainingMoves == 0 && _board.HasAnyBlock())
            {
                TriggerGameOver();
            }
        }

        private void HandleBoardCleared()
        {
            if (_isLevelEnded)
            {
                return;
            }

            TriggerWin();
        }

        private void TriggerGameOver()
        {
            _isLevelEnded = true;
            _board.SetInputEnabled(false);
            Time.timeScale = 0f;

            PanelManager.Instance.ClosePanel(GameConfig.SETTING_PANEL);
            PanelManager.Instance.ClosePanel(GameConfig.WIN_GAME_PANEL);
            PanelManager.Instance.OpenPanel(GameConfig.GAMEOVER_PANEL);
            OnGameOverTriggered?.Invoke();
        }

        private void TriggerWin()
        {
            _isLevelEnded = true;
            _board.SetInputEnabled(false);
            Time.timeScale = 0f;

            PanelManager.Instance.ClosePanel(GameConfig.SETTING_PANEL);
            PanelManager.Instance.ClosePanel(GameConfig.GAMEOVER_PANEL);
            PanelManager.Instance.OpenPanel(GameConfig.WIN_GAME_PANEL);
            OnWinTriggered?.Invoke();
        }

        private void HandleClaimAndNextLevel()
        {
            AddCoins(_winRewardCoins);
            NextLevel();
        }

        private void AddCoins(int bonusCoins)
        {
            _coins += bonusCoins;
            CurrentCoins = _coins;
            PlayerPrefs.SetInt(GameConfig.COIN_ITEM, _coins);
            OnUpdateCoins?.Invoke(_coins);
        }

        private void OpenGameplayPanel()
        {
            PanelManager.Instance.ClosePanel(GameConfig.SETTING_PANEL);
            PanelManager.Instance.ClosePanel(GameConfig.GAMEOVER_PANEL);
            PanelManager.Instance.ClosePanel(GameConfig.WIN_GAME_PANEL);
            PanelManager.Instance.OpenPanel(GameConfig.GAMEPLAY_PANEL);
        }

        private void PushHudState()
        {
            CurrentCoins = _coins;
            OnUpdateCoins?.Invoke(_coins);
            OnUpdateMoves?.Invoke(_remainingMoves);
            OnUpdateLevel?.Invoke(_currentLevel + 1);
        }

        private void SubscribeBoardEvents()
        {
            _board.OnMoveCommitted += HandleMoveCommitted;
            _board.OnBoardCleared += HandleBoardCleared;
        }

        private void UnsubscribeBoardEvents()
        {
            _board.OnMoveCommitted -= HandleMoveCommitted;
            _board.OnBoardCleared -= HandleBoardCleared;
        }

        private void SubscribeUiEvents()
        {
            GameplayPanel.OnRestartRequested += RetryLevel;
            GameplayPanel.OnOpenSettingRequested += OpenSettingPanel;
            GameplayPanel.OnUseBonusTouchRequested += HandleUseBonusTouch;
            GameOverPanel.OnRetryRequested += RetryLevel;
            SettingPanel.OnResumeRequested += ResumeFromSetting;
            // SettingPanel.OnRestartRequested += RetryLevel;
            WinGamePanel.OnClaimAndNextRequested += HandleClaimAndNextLevel;
        }

        private void UnsubscribeUiEvents()
        {
            GameplayPanel.OnRestartRequested -= RetryLevel;
            GameplayPanel.OnOpenSettingRequested -= OpenSettingPanel;
            GameplayPanel.OnUseBonusTouchRequested -= HandleUseBonusTouch;
            GameOverPanel.OnRetryRequested -= RetryLevel;
            SettingPanel.OnResumeRequested -= ResumeFromSetting;
            // SettingPanel.OnRestartRequested -= RetryLevel;
            WinGamePanel.OnClaimAndNextRequested -= HandleClaimAndNextLevel;
        }

        private void HandleUseBonusTouch(int addMoves)
        {
            if (_isLevelEnded)
            {
                return;
            }

            _remainingMoves += Mathf.Max(1, addMoves);
            OnUpdateMoves?.Invoke(_remainingMoves);
        }

        private void OpenSettingPanel()
        {
            if (_isLevelEnded)
            {
                return;
            }

            _board?.SetInputEnabled(false);
            Time.timeScale = 0f;
            PanelManager.Instance.OpenPanel(GameConfig.SETTING_PANEL);
        }

        private void ResumeFromSetting()
        {
            if (_isLevelEnded)
            {
                return;
            }

            _board?.SetInputEnabled(true);
            Time.timeScale = 1f;
            PanelManager.Instance.ClosePanel(GameConfig.SETTING_PANEL);
        }

        private void ClearBoard()
        {
            _board?.Clear();
        }

        private float ComputeCellSizeForCamera(int width, int height, float padding)
        {
            Camera cam = _targetCamera != null ? _targetCamera : Camera.main;
            if (cam == null)
            {
                return 1f;
            }

            float visibleHalfHeight = cam.orthographicSize - padding;
            float visibleHalfWidth = cam.orthographicSize * cam.aspect - padding;

            float fitByWidth = (visibleHalfWidth * 2f) / Mathf.Max(1, width);
            float fitByHeight = (visibleHalfHeight * 2f) / Mathf.Max(1, height);
            float fitted = Mathf.Min(fitByWidth, fitByHeight);

            return Mathf.Max(0.01f, fitted);
        }

        private void CenterCameraOnBoard()
        {
            Camera cam = _targetCamera != null ? _targetCamera : Camera.main;
            if (cam == null || _board == null)
            {
                return;
            }

            cam.transform.position = new Vector3(_board.transform.position.x, _board.transform.position.y, cam.transform.position.z);
        }
    }
}

