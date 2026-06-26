//------------------------------------------------------------
// Game Framework
// AVG Game Project
// 全局游戏设置 — 文字速度、跳过间隔等
//------------------------------------------------------------

using UnityEngine;

namespace AVGGame
{
    /// <summary>
    /// 全局游戏行为设置。
    /// GameSettingPanel 写入，DialoguePanel/ProcedureGame 读取。
    /// </summary>
    public static class GlobalGameSettings
    {
        // ================================================================
        // PlayerPrefs 键名
        // ================================================================
        public const string PREF_WORDS_SPEED = "WordsSpeed";
        public const string PREF_ACT_SPEED  = "ActSpeed";
        public const string PREF_SKIP_ALL   = "SkipAll";
        public const string PREF_SKIP_STOP  = "SkipStop";

        // ================================================================
        // 当前设置值
        // ================================================================

        /// <summary>文字速度 0~1（0=慢, 1=快）</summary>
        public static float WordsSpeed = 0.5f;

        /// <summary>跳过时间间隔 0~1（0=慢, 1=快）</summary>
        public static float SkipInterval = 0.5f;

        /// <summary>跳过已读对话</summary>
        public static bool SkipAll = false;

        /// <summary>跳过未读对话</summary>
        public static bool SkipStop = false;

        // ================================================================
        // 计算属性（DialoguePanel 直接使用）
        // ================================================================

        /// <summary>
        /// 每个字符的打字间隔（秒）。
        /// WordsSpeed=0 → 0.1s/字, WordsSpeed=1 → 0.005s/字
        /// </summary>
        public static float TypeInterval => Mathf.Lerp(0.1f, 0.005f, WordsSpeed);

        /// <summary>
        /// 自动播放/跳过时，每句对话的停留时间（秒）。
        /// SkipInterval=0 → 2s, SkipInterval=1 → 0.2s
        /// </summary>
        public static float SkipDelay => Mathf.Lerp(2f, 0.2f, SkipInterval);

        // ================================================================
        // 加载 / 保存
        // ================================================================

        public static void Load()
        {
            WordsSpeed  = PlayerPrefs.GetFloat(PREF_WORDS_SPEED, 0.5f);
            SkipInterval = PlayerPrefs.GetFloat(PREF_ACT_SPEED,  0.5f);
            SkipAll     = PlayerPrefs.GetInt(PREF_SKIP_ALL, 0) == 1;
            SkipStop    = PlayerPrefs.GetInt(PREF_SKIP_STOP, 0) == 1;
        }

        public static void Save()
        {
            PlayerPrefs.SetFloat(PREF_WORDS_SPEED, WordsSpeed);
            PlayerPrefs.SetFloat(PREF_ACT_SPEED,   SkipInterval);
            PlayerPrefs.SetInt(PREF_SKIP_ALL,      SkipAll ? 1 : 0);
            PlayerPrefs.SetInt(PREF_SKIP_STOP,     SkipStop ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
