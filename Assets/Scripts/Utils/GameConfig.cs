using UnityEngine;

namespace Scripts
{
    public class GameConfig : MonoBehaviour
    {
        // RESOURCES
        public static string BLOCK_DATA_PATH = "SO/BlockDatas";
        
        // PANELS
        public static string PANEL_PATH = "UI/Panels/";
        public static string GAMEPLAY_PANEL = "Panel - Gameplay";
        public static string SETTING_PANEL = "Panel - Setting";
        public static string GAMEOVER_PANEL = "Panel - GameOver";
        public static string WIN_GAME_PANEL = "Panel - WinGame";
        
        // ITEMS
        public static string COIN_ITEM = "Coin";
        public static int COINS => PlayerPrefs.GetInt(COIN_ITEM, 0);
        public static string BOMB_ITEM = "Bomb";
        // public static int BOMB_QUANTITY => PlayerPrefs.GetInt(BOMB_ITEM, 3);
        public static int BOMB_QUANTITY = 3;
        public static string BONUS_TOUCH_ITEM = "BonusTouch";
        // public static int BONUS_TOUCH_QUANTITY => PlayerPrefs.GetInt(BONUS_TOUCH_ITEM, 3);
        public static int BONUS_TOUCH_QUANTITY = 3;
        
        // AUDIO
        public const string BGM_STATE_KEY = "BGMState";
        public const string SFX_STATE_KEY = "SFXState";
        public static int BGM_STATE => PlayerPrefs.GetInt(BGM_STATE_KEY, 1);
        public static int SFX_STATE => PlayerPrefs.GetInt(SFX_STATE_KEY, 1);
        public static string VIBRATION_STATE_KEY = "VibrationState";
        public static int VIBRATION_STATE => PlayerPrefs.GetInt(VIBRATION_STATE_KEY, 1);

        // INFOR
        public static string GITHUB_LINK = "https://github.com/LDCin";
    }
}