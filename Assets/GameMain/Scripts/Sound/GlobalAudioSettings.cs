//------------------------------------------------------------
// Game Framework
// AVG Game Project
// 全局音频设置 — 在设置面板和运行时对话之间共享
//------------------------------------------------------------

using UnityEngine;
using UnityGameFramework.Runtime;

namespace AVGGame
{
    /// <summary>
    /// 全局音频设置，负责音量值的集中存储和应用。
    /// GameSettingPanel 写入，DialoguePanel 读取。
    /// </summary>
    public static class GlobalAudioSettings
    {
        // ================================================================
        // PlayerPrefs 键名（与 GameSettingPanel 一致）
        // ================================================================
        public const string PREF_ALL_AUDIO      = "AllAudioVolume";
        public const string PREF_BGM            = "BGMVolume";
        public const string PREF_SE             = "SEVolume";
        public const string PREF_VOICE          = "VoiceVolume";
        public const string PREF_BGM_WITH_SOUND = "BGMWithSound";

        // ================================================================
        // 当前设置值（0~1）
        // ================================================================
        public static float AllAudioVolume  = 1f;
        public static float BGMVolume       = 1f;
        public static float SEVolume        = 1f;
        public static float VoiceVolume     = 1f;

        /// <summary>语音播放时是否减弱BGM</summary>
        public static bool BGMWithSound = true;

        /// <summary>BGM被外部面板（如设置面板）终止，需要DialoguePanel恢复</summary>
        public static bool BgmInvalidated = false;

        // ================================================================
        // 是否已初始化
        // ================================================================
        private static bool s_Initialized = false;

        /// <summary>
        /// 从 PlayerPrefs 加载，若没有存档则读取 GF SoundGroup 初始音量
        /// </summary>
        public static void Load()
        {
            // 先从 GF SoundGroup 读取初始音量作为默认值
            float defaultBGM   = GetGroupVolume("BGM");
            float defaultSE    = GetGroupVolume("Sound");
            float defaultVoice = GetGroupVolume("Voice");

            AllAudioVolume = PlayerPrefs.GetFloat(PREF_ALL_AUDIO, 1f);
            BGMVolume      = PlayerPrefs.GetFloat(PREF_BGM,       defaultBGM);
            SEVolume       = PlayerPrefs.GetFloat(PREF_SE,        defaultSE);
            VoiceVolume    = PlayerPrefs.GetFloat(PREF_VOICE,     defaultVoice);
            BGMWithSound   = PlayerPrefs.GetInt(PREF_BGM_WITH_SOUND, 1) == 1;

            s_Initialized = true;
            ApplyToSoundGroups();
        }

        /// <summary>
        /// 将音量应用到 GF SoundGroup（最终音量 = 分组音量 * 总音量）
        /// </summary>
        public static void ApplyToSoundGroups()
        {
            SetGroupVolume("BGM",   BGMVolume    * AllAudioVolume);
            SetGroupVolume("Sound", SEVolume     * AllAudioVolume);
            SetGroupVolume("Voice", VoiceVolume  * AllAudioVolume);
        }

        /// <summary>
        /// 保存到 PlayerPrefs
        /// </summary>
        public static void Save()
        {
            PlayerPrefs.SetFloat(PREF_ALL_AUDIO, AllAudioVolume);
            PlayerPrefs.SetFloat(PREF_BGM,        BGMVolume);
            PlayerPrefs.SetFloat(PREF_SE,         SEVolume);
            PlayerPrefs.SetFloat(PREF_VOICE,      VoiceVolume);
            PlayerPrefs.SetInt(PREF_BGM_WITH_SOUND, BGMWithSound ? 1 : 0);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 获取当前是否需要减弱BGM（语音播放时调用）
        /// </summary>
        public static float GetEffectiveBGMVolume()
        {
            return BGMWithSound ? BGMVolume * AllAudioVolume * 0.3f : BGMVolume * AllAudioVolume;
        }

        // ================================================================
        // GF SoundGroup 辅助
        // ================================================================
        private static float GetGroupVolume(string groupName)
        {
            var group = GameEntry.Sound?.GetSoundGroup(groupName);
            return group != null ? group.Volume : 1f;
        }

        private static void SetGroupVolume(string groupName, float volume)
        {
            var group = GameEntry.Sound?.GetSoundGroup(groupName);
            if (group != null)
            {
                group.Volume = Mathf.Clamp01(volume);
            }
        }
    }
}
