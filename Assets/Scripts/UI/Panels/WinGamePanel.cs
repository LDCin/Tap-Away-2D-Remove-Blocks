using System;
using Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Panels
{
    public class WinGamePanel : Panel
    {
        public static event Action OnClaimAndNextRequested;

        [SerializeField] private TextMeshProUGUI _coinText;
        [SerializeField] private Image _awardProgressImage;
        [SerializeField, Min(1)] private int _levelsPerFullBar = 4;

        private const string AwardProgressStepKey = "win_award_progress_step";
        private int _completedLevelSteps;

        private void OnEnable()
        {
            GameManager.OnUpdateCoins += UpdateCoinText;
            UpdateCoinText(GameManager.CurrentCoins);
            LoadAwardProgress();
            RefreshAwardProgressUI();
        }

        private void OnDisable()
        {
            GameManager.OnUpdateCoins -= UpdateCoinText;
        }

        public void GetCoinAndNextLevel()
        {
            AdvanceAwardProgress();
            OnClaimAndNextRequested?.Invoke();
        }

        private void UpdateCoinText(int coin)
        {
            if (_coinText != null)
            {
                _coinText.text = coin.ToString();
            }
        }

        private void LoadAwardProgress()
        {
            int safeLevelsPerFullBar = Mathf.Max(1, _levelsPerFullBar);
            _completedLevelSteps = PlayerPrefs.GetInt(AwardProgressStepKey, 0);
            _completedLevelSteps %= safeLevelsPerFullBar;
        }

        private void SaveAwardProgress()
        {
            PlayerPrefs.SetInt(AwardProgressStepKey, _completedLevelSteps);
            PlayerPrefs.Save();
        }

        private void AdvanceAwardProgress()
        {
            int safeLevelsPerFullBar = Mathf.Max(1, _levelsPerFullBar);
            _completedLevelSteps = (_completedLevelSteps + 1) % safeLevelsPerFullBar;
            SaveAwardProgress();
            RefreshAwardProgressUI();
        }

        private void RefreshAwardProgressUI()
        {
            int safeLevelsPerFullBar = Mathf.Max(1, _levelsPerFullBar);
            _awardProgressImage.type = Image.Type.Filled;
            _awardProgressImage.fillMethod = Image.FillMethod.Horizontal;
            _awardProgressImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            _awardProgressImage.fillAmount = _completedLevelSteps / (float)safeLevelsPerFullBar;
        }
    }
}