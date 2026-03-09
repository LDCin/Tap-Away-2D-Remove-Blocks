using System;
using Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Panels
{
    public class SettingPanel : Panel
    {
        public static event Action OnResumeRequested;
        public static event Action OnChangeBGMState;
        public static event Action OnChangeSFXState;
        public static event Action OnChangeVibrationState;

        [SerializeField] private Sprite _onBGMIcon;
        [SerializeField] private Sprite _offBGMIcon;
        [SerializeField] private Sprite _onSFXIcon;
        [SerializeField] private Sprite _offSFXIcon;
        [SerializeField] private Sprite _onVibrationIcon;
        [SerializeField] private Sprite _offVibrationIcon;
        [SerializeField] private Image _bgmIconImage;
        [SerializeField] private Image _sfxIconImage;
        [SerializeField] private Image _vibrationIconImage;

        private void OnEnable()
        {
            AudioManager.OnBgmStateChanged += UpdateBgmIcon;
            AudioManager.OnSfxStateChanged += UpdateSfxIcon;
            RefreshIcons();
        }

        private void OnDisable()
        {
            AudioManager.OnBgmStateChanged -= UpdateBgmIcon;
            AudioManager.OnSfxStateChanged -= UpdateSfxIcon;
        }

        public void Rate()
        {
            Application.OpenURL(GameConfig.GITHUB_LINK);
        }

        public void ChangeVibrationState()
        {
            bool isOn = GameConfig.VIBRATION_STATE != 1;
            PlayerPrefs.SetInt(GameConfig.VIBRATION_STATE_KEY, isOn ? 1 : 0);
            PlayerPrefs.Save();
            UpdateVibrationIcon(isOn);
            OnChangeVibrationState?.Invoke();
        }

        public void Resume()
        {
            OnResumeRequested?.Invoke();
        }

        public void ChangeBGMState()
        {
            OnChangeBGMState?.Invoke();
        }

        public void ChangeSFXState()
        {
            OnChangeSFXState?.Invoke();
        }

        private void RefreshIcons()
        {
            UpdateBgmIcon(GameConfig.BGM_STATE == 1);
            UpdateSfxIcon(GameConfig.SFX_STATE == 1);
            UpdateVibrationIcon(GameConfig.VIBRATION_STATE == 1);
        }

        private void UpdateBgmIcon(bool isOn)
        {
            if (_bgmIconImage != null)
            {
                _bgmIconImage.sprite = isOn ? _onBGMIcon : _offBGMIcon;
            }
        }

        private void UpdateSfxIcon(bool isOn)
        {
            if (_sfxIconImage != null)
            {
                _sfxIconImage.sprite = isOn ? _onSFXIcon : _offSFXIcon;
            }
        }

        private void UpdateVibrationIcon(bool isOn)
        {
            if (_vibrationIconImage != null)
            {
                _vibrationIconImage.sprite = isOn ? _onVibrationIcon : _offVibrationIcon;
            }
        }
    }
}