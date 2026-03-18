//------------------------------------------------------------
// Game Framework
// AVG Game Project
//------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;
using UnityGameFramework.Runtime;
using GameMain.Scripts.UI.Base;

namespace AVGGame
{
    /// <summary>
    /// 游戏设置面板
    /// 可从主菜单或游戏内菜单打开
    /// </summary>
    public class GameSettingPanel : UIFormBase
    {
        #region 序列化字段 - 音量设置

        [Header("音量设置")]
        [SerializeField] private Slider m_SliderMasterVolume;
        [SerializeField] private Slider m_SliderBGMVolume;
        [SerializeField] private Slider m_SliderSFXVolume;
        [SerializeField] private TextMeshProUGUI m_TextMasterVolume;
        [SerializeField] private TextMeshProUGUI m_TextBGMVolume;
        [SerializeField] private TextMeshProUGUI m_TextSFXVolume;

        #endregion

        #region 序列化字段 - 画面设置

        [Header("画面设置")]
        [SerializeField] private Toggle m_ToggleFullScreen;
        [SerializeField] private TMP_Dropdown m_DropdownResolution;
        [SerializeField] private TMP_Dropdown m_DropdownQuality;

        #endregion

        #region 序列化字段 - 其他设置

        [Header("其他设置")]
        [SerializeField] private Slider m_SliderTextSpeed;
        [SerializeField] private TextMeshProUGUI m_TextTextSpeed;
        [SerializeField] private Toggle m_ToggleAutoPlay;

        #endregion

        #region 序列化字段 - 按钮

        [Header("按钮")]
        [SerializeField] private Button m_ButtonClose;
        [SerializeField] private Button m_ButtonApply;
        [SerializeField] private Button m_ButtonReset;

        #endregion

        #region 序列化字段 - 音频混音器

        [Header("音频混音器")]
        [SerializeField] private AudioMixer m_AudioMixer;

        #endregion

        #region 私有常量

        private const string PREF_MASTER_VOLUME = "MasterVolume";
        private const string PREF_BGM_VOLUME = "BGMVolume";
        private const string PREF_SFX_VOLUME = "SFXVolume";
        private const string PREF_FULLSCREEN = "Fullscreen";
        private const string PREF_QUALITY = "Quality";
        private const string PREF_TEXT_SPEED = "TextSpeed";
        private const string PREF_AUTO_PLAY = "AutoPlay";

        #endregion

        #region 属性

        public override int SortingOrder => 250;

        #endregion

        #region 生命周期

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);

            if (m_ButtonClose != null)
                m_ButtonClose.onClick.AddListener(OnCloseClick);

            if (m_ButtonApply != null)
                m_ButtonApply.onClick.AddListener(OnApplyClick);

            if (m_ButtonReset != null)
                m_ButtonReset.onClick.AddListener(OnResetClick);

            if (m_SliderMasterVolume != null)
                m_SliderMasterVolume.onValueChanged.AddListener(OnMasterVolumeChanged);

            if (m_SliderBGMVolume != null)
                m_SliderBGMVolume.onValueChanged.AddListener(OnBGMVolumeChanged);

            if (m_SliderSFXVolume != null)
                m_SliderSFXVolume.onValueChanged.AddListener(OnSFXVolumeChanged);

            if (m_ToggleFullScreen != null)
                m_ToggleFullScreen.onValueChanged.AddListener(OnFullScreenChanged);

            if (m_DropdownResolution != null)
                m_DropdownResolution.onValueChanged.AddListener(OnResolutionChanged);

            if (m_DropdownQuality != null)
                m_DropdownQuality.onValueChanged.AddListener(OnQualityChanged);

            if (m_SliderTextSpeed != null)
                m_SliderTextSpeed.onValueChanged.AddListener(OnTextSpeedChanged);

            if (m_ToggleAutoPlay != null)
                m_ToggleAutoPlay.onValueChanged.AddListener(OnAutoPlayChanged);
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            LoadSettings();
        }

        #endregion

        #region 按钮事件

        private void OnCloseClick()
        {
            Log.Info("[GameSettingPanel] Close clicked");
            CloseSelf();
        }

        private void OnApplyClick()
        {
            Log.Info("[GameSettingPanel] Apply clicked");
            SaveSettings();
            ApplySettings();
        }

        private void OnResetClick()
        {
            Log.Info("[GameSettingPanel] Reset clicked");
            ResetToDefault();
        }

        #endregion

        #region 音量设置事件

        private void OnMasterVolumeChanged(float value)
        {
            UpdateVolumeText(m_TextMasterVolume, value);
            SetMixerVolume("MasterVolume", value);
        }

        private void OnBGMVolumeChanged(float value)
        {
            UpdateVolumeText(m_TextBGMVolume, value);
            SetMixerVolume("BGMVolume", value);
        }

        private void OnSFXVolumeChanged(float value)
        {
            UpdateVolumeText(m_TextSFXVolume, value);
            SetMixerVolume("SFXVolume", value);
        }

        private void UpdateVolumeText(TextMeshProUGUI text, float value)
        {
            if (text != null)
            {
                text.text = Mathf.RoundToInt(value * 100) + "%";
            }
        }

        private void SetMixerVolume(string parameterName, float value)
        {
            if (m_AudioMixer != null)
            {
                float dB = value > 0.001f ? Mathf.Log10(value) * 20f : -80f;
                m_AudioMixer.SetFloat(parameterName, dB);
            }
        }

        #endregion

        #region 画面设置事件

        private void OnFullScreenChanged(bool isFullScreen)
        {
            Screen.fullScreen = isFullScreen;
        }

        private void OnResolutionChanged(int index)
        {
            Log.Info($"[GameSettingPanel] Resolution changed to index: {index}");
        }

        private void OnQualityChanged(int index)
        {
            QualitySettings.SetQualityLevel(index, true);
            Log.Info($"[GameSettingPanel] Quality changed to: {QualitySettings.names[index]}");
        }

        #endregion

        #region 其他设置事件

        private void OnTextSpeedChanged(float value)
        {
            if (m_TextTextSpeed != null)
            {
                m_TextTextSpeed.text = value.ToString("F1");
            }
        }

        private void OnAutoPlayChanged(bool isAutoPlay)
        {
            Log.Info($"[GameSettingPanel] AutoPlay: {isAutoPlay}");
        }

        #endregion

        #region 设置保存/加载

        private void LoadSettings()
        {
            float masterVolume = PlayerPrefs.GetFloat(PREF_MASTER_VOLUME, 1f);
            float bgmVolume = PlayerPrefs.GetFloat(PREF_BGM_VOLUME, 1f);
            float sfxVolume = PlayerPrefs.GetFloat(PREF_SFX_VOLUME, 1f);

            if (m_SliderMasterVolume != null)
                m_SliderMasterVolume.value = masterVolume;

            if (m_SliderBGMVolume != null)
                m_SliderBGMVolume.value = bgmVolume;

            if (m_SliderSFXVolume != null)
                m_SliderSFXVolume.value = sfxVolume;

            bool fullScreen = PlayerPrefs.GetInt(PREF_FULLSCREEN, 1) == 1;
            int quality = PlayerPrefs.GetInt(PREF_QUALITY, QualitySettings.GetQualityLevel());

            if (m_ToggleFullScreen != null)
                m_ToggleFullScreen.isOn = fullScreen;

            if (m_DropdownQuality != null)
                m_DropdownQuality.value = quality;

            float textSpeed = PlayerPrefs.GetFloat(PREF_TEXT_SPEED, 1f);
            bool autoPlay = PlayerPrefs.GetInt(PREF_AUTO_PLAY, 0) == 1;

            if (m_SliderTextSpeed != null)
                m_SliderTextSpeed.value = textSpeed;

            if (m_ToggleAutoPlay != null)
                m_ToggleAutoPlay.isOn = autoPlay;
        }

        private void SaveSettings()
        {
            if (m_SliderMasterVolume != null)
                PlayerPrefs.SetFloat(PREF_MASTER_VOLUME, m_SliderMasterVolume.value);

            if (m_SliderBGMVolume != null)
                PlayerPrefs.SetFloat(PREF_BGM_VOLUME, m_SliderBGMVolume.value);

            if (m_SliderSFXVolume != null)
                PlayerPrefs.SetFloat(PREF_SFX_VOLUME, m_SliderSFXVolume.value);

            if (m_ToggleFullScreen != null)
                PlayerPrefs.SetInt(PREF_FULLSCREEN, m_ToggleFullScreen.isOn ? 1 : 0);

            if (m_DropdownQuality != null)
                PlayerPrefs.SetInt(PREF_QUALITY, m_DropdownQuality.value);

            if (m_SliderTextSpeed != null)
                PlayerPrefs.SetFloat(PREF_TEXT_SPEED, m_SliderTextSpeed.value);

            if (m_ToggleAutoPlay != null)
                PlayerPrefs.SetInt(PREF_AUTO_PLAY, m_ToggleAutoPlay.isOn ? 1 : 0);

            PlayerPrefs.Save();
            Log.Info("[GameSettingPanel] Settings saved");
        }

        private void ApplySettings()
        {
            if (m_SliderMasterVolume != null)
                SetMixerVolume("MasterVolume", m_SliderMasterVolume.value);

            if (m_SliderBGMVolume != null)
                SetMixerVolume("BGMVolume", m_SliderBGMVolume.value);

            if (m_SliderSFXVolume != null)
                SetMixerVolume("SFXVolume", m_SliderSFXVolume.value);

            if (m_ToggleFullScreen != null)
                Screen.fullScreen = m_ToggleFullScreen.isOn;

            if (m_DropdownQuality != null)
                QualitySettings.SetQualityLevel(m_DropdownQuality.value, true);

            Log.Info("[GameSettingPanel] Settings applied");
        }

        private void ResetToDefault()
        {
            if (m_SliderMasterVolume != null)
                m_SliderMasterVolume.value = 1f;

            if (m_SliderBGMVolume != null)
                m_SliderBGMVolume.value = 1f;

            if (m_SliderSFXVolume != null)
                m_SliderSFXVolume.value = 1f;

            if (m_ToggleFullScreen != null)
                m_ToggleFullScreen.isOn = true;

            if (m_DropdownQuality != null)
                m_DropdownQuality.value = QualitySettings.names.Length - 1;

            if (m_SliderTextSpeed != null)
                m_SliderTextSpeed.value = 1f;

            if (m_ToggleAutoPlay != null)
                m_ToggleAutoPlay.isOn = false;

            Log.Info("[GameSettingPanel] Settings reset to default");
        }

        #endregion
    }
}