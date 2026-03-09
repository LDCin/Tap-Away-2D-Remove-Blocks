using UI.Panels;
using UnityEngine;

namespace Scripts
{
    public class AudioManager : Singleton<AudioManager>
    {
        public static event System.Action<bool> OnBgmStateChanged;
        public static event System.Action<bool> OnSfxStateChanged;

        [SerializeField] private AudioSource _BGM;
        [SerializeField] private AudioSource _SFX;

        [SerializeField] private AudioClip _defaultSFX;
        [SerializeField] private AudioClip _clickSound;
        [SerializeField] private AudioClip _gameOverSound;
        [SerializeField] private AudioClip _winSound;

        public override void Awake()
        {
            base.Awake();
            ApplyBgmState(GameConfig.BGM_STATE == 1, true);
            ApplySfxState(GameConfig.SFX_STATE == 1, true);
        }

        private void OnEnable()
        {
            SettingPanel.OnChangeBGMState += ChangeBGMState;
            SettingPanel.OnChangeSFXState += ChangeSFXState;

            GameplayPanel.OnRestartRequested += PlayClickSound;
            GameplayPanel.OnOpenSettingRequested += PlayClickSound;
            GameOverPanel.OnRetryRequested += PlayClickSound;
            SettingPanel.OnResumeRequested += PlayClickSound;
            WinGamePanel.OnClaimAndNextRequested += PlayClickSound;

            GameManager.OnMoveUsed += PlayDefaultSfx;
            GameManager.OnGameOverTriggered += PlayGameOverSound;
            GameManager.OnWinTriggered += PlayWinSound;
        }

        private void OnDisable()
        {
            SettingPanel.OnChangeBGMState -= ChangeBGMState;
            SettingPanel.OnChangeSFXState -= ChangeSFXState;

            GameplayPanel.OnRestartRequested -= PlayClickSound;
            GameplayPanel.OnOpenSettingRequested -= PlayClickSound;
            GameOverPanel.OnRetryRequested -= PlayClickSound;
            SettingPanel.OnResumeRequested -= PlayClickSound;
            WinGamePanel.OnClaimAndNextRequested -= PlayClickSound;

            GameManager.OnMoveUsed -= PlayDefaultSfx;
            GameManager.OnGameOverTriggered -= PlayGameOverSound;
            GameManager.OnWinTriggered -= PlayWinSound;
        }

        public void PlayBGM()
        {
            ApplyBgmState(true, false);
        }

        public void StopBGM()
        {
            ApplyBgmState(false, false);
        }

        public void PlaySFX()
        {
            ApplySfxState(true, false);
        }

        public void StopSFX()
        {
            ApplySfxState(false, false);
        }

        public void ChangeSFXState()
        {
            ApplySfxState(GameConfig.SFX_STATE != 1, false);
        }

        public void ChangeBGMState()
        {
            ApplyBgmState(GameConfig.BGM_STATE != 1, false);
        }

        public void PlayClickSound()
        {
            PlayOneShot(_clickSound);
        }

        public void PlayDefaultSfx()
        {
            PlayOneShot(_defaultSFX);
        }

        public void PlayGameOverSound()
        {
            PlayOneShot(_gameOverSound);
        }

        public void PlayWinSound()
        {
            PlayOneShot(_winSound != null ? _winSound : _defaultSFX);
        }

        private void ApplyBgmState(bool isOn, bool silent)
        {
            if (_BGM == null)
            {
                return;
            }

            _BGM.loop = true;
            _BGM.mute = !isOn;
            if (isOn && !_BGM.isPlaying)
            {
                _BGM.Play();
            }

            PlayerPrefs.SetInt(GameConfig.BGM_STATE_KEY, isOn ? 1 : 0);
            PlayerPrefs.Save();

            if (!silent)
            {
                OnBgmStateChanged?.Invoke(isOn);
            }
            else
            {
                OnBgmStateChanged?.Invoke(GameConfig.BGM_STATE == 1);
            }
        }

        private void ApplySfxState(bool isOn, bool silent)
        {
            if (_SFX == null)
            {
                return;
            }

            _SFX.mute = !isOn;
            PlayerPrefs.SetInt(GameConfig.SFX_STATE_KEY, isOn ? 1 : 0);
            PlayerPrefs.Save();

            if (!silent)
            {
                OnSfxStateChanged?.Invoke(isOn);
            }
            else
            {
                OnSfxStateChanged?.Invoke(GameConfig.SFX_STATE == 1);
            }
        }

        private void PlayOneShot(AudioClip clip)
        {
            if (_SFX == null || clip == null || GameConfig.SFX_STATE != 1)
            {
                return;
            }

            _SFX.PlayOneShot(clip);
        }
    }
}