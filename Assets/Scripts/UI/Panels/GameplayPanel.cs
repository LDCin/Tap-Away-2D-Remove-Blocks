using System;
using Scripts;
using TMPro;
using UnityEngine;

namespace UI.Panels
{
    public class GameplayPanel : Panel
    {
        public static event Action OnRestartRequested;
        public static event Action OnOpenSettingRequested;
        public static event Action OnUseBombClicked;
        public static event Action OnUseBonusTouchClicked;
        public static event Action<int> OnUseBonusTouchRequested;

        [SerializeField] private TextMeshProUGUI _coinText;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private TextMeshProUGUI _moveText;
        [SerializeField] private TextMeshProUGUI _bombquantityText;
        [SerializeField] private TextMeshProUGUI _bonusTouchquantityText;

        private int _bombQuantity;
        private int _bonusTouchQuantity;
        [SerializeField, Min(1)] private int _bonusTouchAddMoves = 3;

        private void OnEnable()
        {
            GameManager.OnUpdateCoins += UpdateCoinText;
            GameManager.OnUpdateLevel += UpdateLevelText;
            GameManager.OnUpdateMoves += UpdateMoveText;
        }

        private void OnDisable()
        {
            GameManager.OnUpdateCoins -= UpdateCoinText;
            GameManager.OnUpdateLevel -= UpdateLevelText;
            GameManager.OnUpdateMoves -= UpdateMoveText;
        }

        private void Start()
        {
            _bombQuantity = GameConfig.BOMB_QUANTITY;
            _bonusTouchQuantity = GameConfig.BONUS_TOUCH_QUANTITY;
            UpdateBombQuantityText(_bombQuantity);
            UpdateBonusTouchQuantityText(_bonusTouchQuantity);
        }

        public void UpdateCoinText(int coin)
        {
            if (_coinText != null)
            {
                _coinText.text = coin.ToString();
            }
        }

        public void UpdateLevelText(int level)
        {
            if (_levelText != null)
            {
                _levelText.text = "Level " + level.ToString();
            }
        }

        public void UpdateMoveText(int move)
        {
            if (_moveText != null)
            {
                _moveText.text = move.ToString() + " Moves";
            }
        }

        public void UpdateBombQuantityText(int bombQuantity)
        {
            if (_bombquantityText != null)
            {
                _bombquantityText.text = bombQuantity.ToString();
            }
        }

        public void UpdateBonusTouchQuantityText(int bonusTouchQuantity)
        {
            if (_bonusTouchquantityText != null)
            {
                _bonusTouchquantityText.text = bonusTouchQuantity.ToString();
            }
        }

        public void UseBomb()
        {
            OnUseBombClicked?.Invoke();

            _bombQuantity = Mathf.Max(0, _bombQuantity - 1);
            UpdateBombQuantityText(_bombQuantity);
            PlayerPrefs.SetInt(GameConfig.BOMB_ITEM, _bombQuantity);
        }

        public void UseBonusTouch()
        {
            OnUseBonusTouchClicked?.Invoke();

            if (_bonusTouchQuantity <= 0)
            {
                return;
            }

            _bonusTouchQuantity = Mathf.Max(0, _bonusTouchQuantity - 1);
            UpdateBonusTouchQuantityText(_bonusTouchQuantity);
            PlayerPrefs.SetInt(GameConfig.BONUS_TOUCH_ITEM, _bonusTouchQuantity);
            OnUseBonusTouchRequested?.Invoke(Mathf.Max(1, _bonusTouchAddMoves));
        }

        public void Setting()
        {
            OnOpenSettingRequested?.Invoke();
        }

        public void Restart()
        {
            OnRestartRequested?.Invoke();
        }
    }
}