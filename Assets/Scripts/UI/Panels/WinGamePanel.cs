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

        private void OnEnable()
        {
            GameManager.OnUpdateCoins += UpdateCoinText;
            UpdateCoinText(GameManager.CurrentCoins);
        }

        private void OnDisable()
        {
            GameManager.OnUpdateCoins -= UpdateCoinText;
        }

        public void GetCoinAndNextLevel()
        {
            OnClaimAndNextRequested?.Invoke();
        }

        private void UpdateCoinText(int coin)
        {
            if (_coinText != null)
            {
                _coinText.text = coin.ToString();
            }
        }
    }
}