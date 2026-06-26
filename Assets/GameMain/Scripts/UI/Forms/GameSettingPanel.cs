//------------------------------------------------------------
// Game Framework
// AVG Game Project
//------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;
using UnityGameFramework.Runtime;
using GameFramework.Sound;

namespace AVGGame
{
    /// <summary>
    /// 游戏设置面板
    /// 三个子面板通过按钮切换: SoundSettingPanel / OperationSettingPanel / SceneSettingPanel
    /// 设置值通过 PlayerPrefs 持久化，DialoguePanel 运行时读取相同键名
    /// </summary>
    public class GameSettingPanel : UIFormBase
    {
        // ================================================================
        // PlayerPrefs 键名（DialoguePanel 也通过这些键读取运行时设置）
        // ================================================================
        public const string PREF_ALL_AUDIO      = "AllAudioVolume";
        public const string PREF_BGM            = "BGMVolume";
        public const string PREF_SE             = "SEVolume";
        public const string PREF_VOICE          = "VoiceVolume";
        public const string PREF_BGM_WITH_SOUND = "BGMWithSound";
        public const string PREF_WORDS_SPEED    = "WordsSpeed";
        public const string PREF_ACT_SPEED      = "ActSpeed";
        public const string PREF_SKIP_ALL       = "SkipAll";
        public const string PREF_SKIP_STOP      = "SkipStop";
        public const string PREF_FULLSCREEN     = "Fullscreen";
        public const string PREF_WINDOW_MODE    = "WindowMode";
        public const string PREF_BORDERLESS_FS  = "BorderlessFullscreen";
        public const string PREF_BRIGHTNESS     = "Brightness";
        public const string PREF_RESOLUTION     = "Resolution";
        public const string PREF_FRAME_RATE     = "FrameRate";
        public const string PREF_QUALITY        = "Quality";

        // ================================================================
        // 序列化字段
        // ================================================================
        #region 音量设置

        [Header("音量 - 总音量")]
        [SerializeField] private Slider m_SliderAllAudio;
        [SerializeField] private Text m_TextAllAudio;

        [Header("音量 - BGM")]
        [SerializeField] private Slider m_SliderBGM;
        [SerializeField] private Text m_TextBGM;

        [Header("音量 - 音效SE")]
        [SerializeField] private Slider m_SliderSE;
        [SerializeField] private Text m_TextSE;

        [Header("音量 - 语音Voice")]
        [SerializeField] private Slider m_SliderVoice;
        [SerializeField] private Text m_TextVoice;

        [Header("音量 - 语音播放时减弱BGM")]
        [SerializeField] private Toggle m_ToggleBGMWithSound;

        #endregion

        #region 操作设置

        [Header("操作 - 演示文本")]
        [SerializeField] private Text m_TextAct;

        [Header("操作 - 文字速度")]
        [SerializeField] private Slider m_SliderWordsSpeed;
        [SerializeField] private Text m_TextWordsSpeed;

        [Header("操作 - 跳过时间间隔")]
        [SerializeField] private Slider m_SliderActSpeed;
        [SerializeField] private Text m_TextActSpeed;

        [Header("操作 - 跳过设置")]
        [SerializeField] private Toggle m_ToggleSkipAll;
        [SerializeField] private Toggle m_ToggleSkipStop;

        #endregion

        #region 画面设置

        [Header("画面 - 全屏")]
        [SerializeField] private Toggle m_ToggleFullScreen;

        [Header("画面 - 窗口模式")]
        [SerializeField] private Toggle m_ToggleWindowMode;

        [Header("画面 - 无边框全屏")]
        [SerializeField] private Toggle m_ToggleBorderlessFullScreen;

        [Header("画面 - 亮度")]
        [SerializeField] private Slider m_SliderBrightness;
        [SerializeField] private Text m_TextBrightness;

        [Header("画面 - 分辨率")]
        [SerializeField] private TMP_Dropdown m_DropdownResolution;

        [Header("画面 - 帧率")]
        [SerializeField] private TMP_Dropdown m_DropdownFrameRate;

        [Header("画面 - 画质")]
        [SerializeField] private TMP_Dropdown m_DropdownQuality;

        #endregion

        #region 面板切换按钮

        [Header("面板切换按钮")]
        [SerializeField] private Button m_ButtonSoundSettingPanel;
        [SerializeField] private Button m_ButtonOperationSettingPanel;
        [SerializeField] private Button m_ButtonSceneSettingPanel;

        #endregion

        #region 其他

        [Header("关闭按钮")]
        [SerializeField] private Button m_ButtonClose;

        [Header("音频混音器 (可选)")]
        [SerializeField] private AudioMixer m_AudioMixer;

        #endregion

        // ================================================================
        // 私有字段
        // ================================================================
        private Transform m_SoundPanel;
        private Transform m_OperationPanel;
        private Transform m_ScenePanel;

        // 调试音频路径
        private const string c_TestBgmPath   = "Assets/GameMain/Music/优雅.mp3";
        private const string c_TestVoicePath = "Assets/GameMain/Art/Voice/配音/周杉/4.2老板别急.mp3";

        // 调试音频状态
        private bool m_TestBgmPlaying = false;
        private bool m_IsLoading = false;  // 加载设置时抑制事件

        // 演示文本打字机
        private Coroutine m_PreviewCoroutine;
        private const string c_PreviewText = "演示文本演示文本演示文本演示文本演示文本演示文本演示文本";

        // ================================================================
        // 属性
        // ================================================================
        public override int SortingOrder => 250;

        // ================================================================
        // 生命周期
        // ================================================================
        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            BindComponents();
            InitDropdowns();
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            // 从全局设置加载（首次会从 GF SoundGroup / PlayerPrefs 读取）
            GlobalAudioSettings.Load();
            GlobalGameSettings.Load();

            LoadSettings();
            UpdateAllTexts();

            // 默认显示操作设置面板
            SwitchPanel(1);

            // 打开设置即播放BGM（不播放语音）
            PlayTestBgm();

            // 开始演示文本预览
            StartTextPreview();
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);
        }

        // ================================================================
        // 组件绑定
        // ================================================================
        private void BindComponents()
        {
            string settingBase = "Canvas/Background/SettingPlate";
            string soundBase   = $"{settingBase}/SoundSettingPanel";
            string opBase      = $"{settingBase}/OperationSettingPanel";
            string sceneBase   = $"{settingBase}/SceneSettingPanel";

            // 面板引用
            m_SoundPanel     = this.GetComponentByPath<Transform>(soundBase);
            m_OperationPanel = this.GetComponentByPath<Transform>(opBase);
            m_ScenePanel     = this.GetComponentByPath<Transform>(sceneBase);

            // --- 音量设置 ---
            m_SliderAllAudio     = this.GetComponentByPath<Slider>($"{soundBase}/AllAudioPlate/Slider");
            m_TextAllAudio       = this.GetComponentByPath<Text>($"{soundBase}/AllAudioPlate/TextNumber");
            m_SliderBGM          = this.GetComponentByPath<Slider>($"{soundBase}/BGMPlate/Slider");
            m_TextBGM            = this.GetComponentByPath<Text>($"{soundBase}/BGMPlate/TextNumber");
            m_SliderSE           = this.GetComponentByPath<Slider>($"{soundBase}/SEPlate/Slider");
            m_TextSE             = this.GetComponentByPath<Text>($"{soundBase}/SEPlate/TextNumber");
            m_SliderVoice        = this.GetComponentByPath<Slider>($"{soundBase}/SoundPlate/Slider");
            m_TextVoice          = this.GetComponentByPath<Text>($"{soundBase}/SoundPlate/TextNumber");
            m_ToggleBGMWithSound = this.GetComponentByPath<Toggle>($"{soundBase}/BGMWithSoundPlate/Toggle");

            // --- 操作设置 ---
            m_TextAct            = this.GetComponentByPath<Text>($"{opBase}/WordsSpeedPlate/ActPlate/TextAct");
            m_SliderWordsSpeed   = this.GetComponentByPath<Slider>($"{opBase}/WordsSpeedPlate/Slider");
            m_TextWordsSpeed     = this.GetComponentByPath<Text>($"{opBase}/WordsSpeedPlate/TextActSpeed");
            m_SliderActSpeed     = this.GetComponentByPath<Slider>($"{opBase}/WordsSpeedPlate/Slider2");
            m_TextActSpeed       = this.GetComponentByPath<Text>($"{opBase}/WordsSpeedPlate/TextSkip");
            m_ToggleSkipAll      = this.GetComponentByPath<Toggle>($"{opBase}/SkipPlate/ToggleSkipAll");
            m_ToggleSkipStop     = this.GetComponentByPath<Toggle>($"{opBase}/SkipPlate/ToggleSkipStop");

            // --- 画面设置 ---
            m_ToggleFullScreen       = this.GetComponentByPath<Toggle>($"{sceneBase}/FullScene/ToggleFullScene");
            m_ToggleWindowMode       = this.GetComponentByPath<Toggle>($"{sceneBase}/FullScene/ToggleBoilderScene");
            m_ToggleBorderlessFullScreen = this.GetComponentByPath<Toggle>($"{sceneBase}/FullScene/ToggleBorderlessFullScreen");
            m_SliderBrightness       = this.GetComponentByPath<Slider>($"{sceneBase}/LightPlate/Slider");
            m_TextBrightness         = this.GetComponentByPath<Text>($"{sceneBase}/LightPlate/TextNumber");
            m_DropdownResolution     = this.GetComponentByPath<TMP_Dropdown>($"{sceneBase}/SceneModePlate/Dropdown");
            m_DropdownFrameRate      = this.GetComponentByPath<TMP_Dropdown>($"{sceneBase}/FrameRatePlate/Dropdown");
            m_DropdownQuality        = this.GetComponentByPath<TMP_Dropdown>($"{sceneBase}/LightPlate/Dropdown");

            // --- 面板切换按钮 ---
            m_ButtonSoundSettingPanel      = this.GetComponentByPath<Button>($"{settingBase}/SettingLinePanel/ButtonSound");
            m_ButtonOperationSettingPanel  = this.GetComponentByPath<Button>($"{settingBase}/SettingLinePanel/ButtonOperation");
            m_ButtonSceneSettingPanel      = this.GetComponentByPath<Button>($"{settingBase}/SettingLinePanel/ButtonScene");

            // --- 关闭按钮 ---
            m_ButtonClose = this.GetComponentByPath<Button>("Canvas/Background/ButtonClose");

            // === 绑定事件 ===

            // 面板切换
            if (m_ButtonSoundSettingPanel != null)
                m_ButtonSoundSettingPanel.onClick.AddListener(() => SwitchPanel(0));
            if (m_ButtonOperationSettingPanel != null)
                m_ButtonOperationSettingPanel.onClick.AddListener(() => SwitchPanel(1));
            if (m_ButtonSceneSettingPanel != null)
                m_ButtonSceneSettingPanel.onClick.AddListener(() => SwitchPanel(2));

            // 关闭
            if (m_ButtonClose != null)
                m_ButtonClose.onClick.AddListener(OnCloseClick);

            // 音量 Slider
            if (m_SliderAllAudio != null)
                m_SliderAllAudio.onValueChanged.AddListener(OnAllAudioChanged);
            if (m_SliderBGM != null)
                m_SliderBGM.onValueChanged.AddListener(OnBGMChanged);
            if (m_SliderSE != null)
                m_SliderSE.onValueChanged.AddListener(OnSEChanged);
            if (m_SliderVoice != null)
                m_SliderVoice.onValueChanged.AddListener(OnVoiceChanged);

            // 音量 Toggle
            if (m_ToggleBGMWithSound != null)
                m_ToggleBGMWithSound.onValueChanged.AddListener(OnBGMWithSoundChanged);

            // 操作 Slider
            if (m_SliderWordsSpeed != null)
                m_SliderWordsSpeed.onValueChanged.AddListener(OnWordsSpeedChanged);
            if (m_SliderActSpeed != null)
                m_SliderActSpeed.onValueChanged.AddListener(OnActSpeedChanged);

            // 操作 Toggle
            if (m_ToggleSkipAll != null)
                m_ToggleSkipAll.onValueChanged.AddListener(OnSkipAllChanged);
            if (m_ToggleSkipStop != null)
                m_ToggleSkipStop.onValueChanged.AddListener(OnSkipStopChanged);

            // 画面 Toggle
            if (m_ToggleFullScreen != null)
                m_ToggleFullScreen.onValueChanged.AddListener(OnFullScreenChanged);
            if (m_ToggleWindowMode != null)
                m_ToggleWindowMode.onValueChanged.AddListener(OnWindowModeChanged);
            if (m_ToggleBorderlessFullScreen != null)
                m_ToggleBorderlessFullScreen.onValueChanged.AddListener(OnBorderlessChanged);

            // 画面 Slider
            if (m_SliderBrightness != null)
                m_SliderBrightness.onValueChanged.AddListener(OnBrightnessChanged);

            // 画面 Dropdown
            if (m_DropdownResolution != null)
                m_DropdownResolution.onValueChanged.AddListener(OnResolutionChanged);
            if (m_DropdownFrameRate != null)
                m_DropdownFrameRate.onValueChanged.AddListener(OnFrameRateChanged);
            if (m_DropdownQuality != null)
                m_DropdownQuality.onValueChanged.AddListener(OnQualityChanged);
        }

        // ================================================================
        // 面板切换
        // ================================================================
        /// <param name="index">0=音量, 1=操作, 2=画面</param>
        private void SwitchPanel(int index)
        {
            if (m_SoundPanel != null) m_SoundPanel.gameObject.SetActive(index == 0);
            if (m_OperationPanel != null) m_OperationPanel.gameObject.SetActive(index == 1);
            if (m_ScenePanel != null) m_ScenePanel.gameObject.SetActive(index == 2);

            // 切换到音量面板时播放一次语音测试
            if (index == 0)
            {
                PlayTestVoice();
            }
        }

        // ================================================================
        // Dropdown 初始化
        // ================================================================
        private void InitDropdowns()
        {
            // 分辨率
            if (m_DropdownResolution != null)
            {
                m_DropdownResolution.ClearOptions();
                var options = new List<TMP_Dropdown.OptionData>();
                var resolutions = Screen.resolutions;
                var seen = new HashSet<string>();
                int currentIndex = 0;

                for (int i = 0; i < resolutions.Length; i++)
                {
                    string label = $"{resolutions[i].width} x {resolutions[i].height} @{resolutions[i].refreshRate}Hz";
                    if (seen.Add(label))
                    {
                        options.Add(new TMP_Dropdown.OptionData(label));
                        if (resolutions[i].width == Screen.currentResolution.width &&
                            resolutions[i].height == Screen.currentResolution.height)
                            currentIndex = options.Count - 1;
                    }
                }
                m_DropdownResolution.AddOptions(options);
                m_DropdownResolution.value = currentIndex;
                m_DropdownResolution.RefreshShownValue();
            }

            // 帧率
            if (m_DropdownFrameRate != null)
            {
                m_DropdownFrameRate.ClearOptions();
                m_DropdownFrameRate.AddOptions(new List<string> { "30 FPS", "60 FPS", "120 FPS", "不限制" });
            }

            // 画质
            if (m_DropdownQuality != null)
            {
                m_DropdownQuality.ClearOptions();
                var qualityOptions = new List<TMP_Dropdown.OptionData>();
                foreach (var name in QualitySettings.names)
                    qualityOptions.Add(new TMP_Dropdown.OptionData(name));
                m_DropdownQuality.AddOptions(qualityOptions);
            }
        }

        // ================================================================
        // 音量事件
        // ================================================================
        private void OnAllAudioChanged(float value)
        {
            if (m_IsLoading) return;
            UpdatePercentText(m_TextAllAudio, value);
            GlobalAudioSettings.AllAudioVolume = value;
            GlobalAudioSettings.ApplyToSoundGroups();
            GlobalAudioSettings.Save();

            // 总音量调整：同时播放BGM和语音测试
            PlayTestBgm();
            PlayTestVoice();
        }

        private void OnBGMChanged(float value)
        {
            if (m_IsLoading) return;
            UpdatePercentText(m_TextBGM, value);
            GlobalAudioSettings.BGMVolume = value;
            GlobalAudioSettings.ApplyToSoundGroups();
            GlobalAudioSettings.Save();

            PlayTestBgm();
        }

        private void OnSEChanged(float value)
        {
            if (m_IsLoading) return;
            UpdatePercentText(m_TextSE, value);
            GlobalAudioSettings.SEVolume = value;
            GlobalAudioSettings.ApplyToSoundGroups();
            GlobalAudioSettings.Save();
        }

        private void OnVoiceChanged(float value)
        {
            if (m_IsLoading) return;
            UpdatePercentText(m_TextVoice, value);
            GlobalAudioSettings.VoiceVolume = value;
            GlobalAudioSettings.ApplyToSoundGroups();
            GlobalAudioSettings.Save();

            PlayTestVoice();
        }

        private void OnBGMWithSoundChanged(bool isOn)
        {
            if (m_IsLoading) return;
            GlobalAudioSettings.BGMWithSound = isOn;
            GlobalAudioSettings.Save();
        }

        private void UpdatePercentText(Text text, float value)
        {
            if (text != null)
                text.text = Mathf.RoundToInt(value * 100) + "%";
        }

        // ================================================================
        // 操作设置事件
        // ================================================================
        private void OnWordsSpeedChanged(float value)
        {
            if (m_IsLoading) return;
            GlobalGameSettings.WordsSpeed = value;
            GlobalGameSettings.Save();
            if (m_TextWordsSpeed != null)
                m_TextWordsSpeed.text = value.ToString("F1");

            StartTextPreview();
        }

        private void OnActSpeedChanged(float value)
        {
            if (m_IsLoading) return;
            GlobalGameSettings.SkipInterval = value;
            GlobalGameSettings.Save();
            if (m_TextActSpeed != null)
                m_TextActSpeed.text = value.ToString("F1");

            StartTextPreview();
        }

        private void OnSkipAllChanged(bool isOn)
        {
            if (m_IsLoading) return;
            GlobalGameSettings.SkipAll = isOn;
            GlobalGameSettings.Save();
        }

        private void OnSkipStopChanged(bool isOn)
        {
            if (m_IsLoading) return;
            GlobalGameSettings.SkipStop = isOn;
            GlobalGameSettings.Save();
        }

        // ================================================================
        // 画面设置事件
        // ================================================================
        private void OnFullScreenChanged(bool isOn)
        {
            if (m_IsLoading) return;
            if (isOn)
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
            PlayerPrefs.SetInt(PREF_FULLSCREEN, isOn ? 1 : 0);
        }

        private void OnWindowModeChanged(bool isOn)
        {
            if (m_IsLoading) return;
            if (isOn)
                Screen.fullScreenMode = FullScreenMode.Windowed;
            PlayerPrefs.SetInt(PREF_WINDOW_MODE, isOn ? 1 : 0);
        }

        private void OnBorderlessChanged(bool isOn)
        {
            if (m_IsLoading) return;
            if (isOn)
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
            PlayerPrefs.SetInt(PREF_BORDERLESS_FS, isOn ? 1 : 0);
        }

        private void OnBrightnessChanged(float value)
        {
            if (m_IsLoading) return;
            Screen.brightness = value;
            if (m_TextBrightness != null)
                m_TextBrightness.text = Mathf.RoundToInt(value * 100) + "%";
            PlayerPrefs.SetFloat(PREF_BRIGHTNESS, value);
        }

        private void OnResolutionChanged(int index)
        {
            if (m_IsLoading) return;
            var resolutions = Screen.resolutions;
            if (index >= 0 && index < resolutions.Length)
            {
                var res = resolutions[index];
                Screen.SetResolution(res.width, res.height, Screen.fullScreenMode);
            }
            PlayerPrefs.SetInt(PREF_RESOLUTION, index);
        }

        private void OnFrameRateChanged(int index)
        {
            if (m_IsLoading) return;
            int targetFps = index switch
            {
                0 => 30,
                1 => 60,
                2 => 120,
                _ => -1 // 不限制
            };
            Application.targetFrameRate = targetFps;
            PlayerPrefs.SetInt(PREF_FRAME_RATE, index);
        }

        private void OnQualityChanged(int index)
        {
            if (m_IsLoading) return;
            QualitySettings.SetQualityLevel(index, true);
            PlayerPrefs.SetInt(PREF_QUALITY, index);
        }

        // ================================================================
        // 调试音频播放
        // ================================================================
        private void PlayTestBgm()
        {
            if (m_TestBgmPlaying) return;

            var bgmParams = new PlaySoundParams
            {
                Loop = true,
                Priority = 0,
                VolumeInSoundGroup = 1f,
                FadeInSeconds = 0.3f,
                Pitch = 1f,
                PanStereo = 0f,
                SpatialBlend = 0f,
                MaxDistance = 100f,
                DopplerLevel = 0f
            };
            GameEntry.Sound.PlaySound(c_TestBgmPath, "BGM", bgmParams);
            m_TestBgmPlaying = true;
        }

        private void PlayTestVoice()
        {
            var voiceParams = new PlaySoundParams
            {
                Loop = false,
                Priority = 0,
                VolumeInSoundGroup = 1f,
                FadeInSeconds = 0f,
                Pitch = 1f,
                PanStereo = 0f,
                SpatialBlend = 0f,
                MaxDistance = 100f,
                DopplerLevel = 0f
            };
            GameEntry.Sound.PlaySound(c_TestVoicePath, "Voice", voiceParams);
        }

        private void StopTestAudio()
        {
            if (m_TestBgmPlaying)
            {
                GameEntry.Sound.GetSoundGroup("BGM")?.StopAllLoadedSounds();
                m_TestBgmPlaying = false;
            }
            GameEntry.Sound.GetSoundGroup("Voice")?.StopAllLoadedSounds();
        }

        // ================================================================
        // 演示文本打字机预览
        // ================================================================
        private void StartTextPreview()
        {
            if (m_TextAct == null) return;

            if (m_PreviewCoroutine != null)
                StopCoroutine(m_PreviewCoroutine);

            m_PreviewCoroutine = StartCoroutine(TextPreviewRoutine());
        }

        private IEnumerator TextPreviewRoutine()
        {
            while (true)
            {
                m_TextAct.text = "";

                // 实时读取全局设置中的打字间隔
                float typeInterval = GlobalGameSettings.TypeInterval;

                // 如果速度极快，直接显示全文
                if (typeInterval <= 0.01f)
                {
                    m_TextAct.text = c_PreviewText;
                    yield return new WaitForSeconds(1f);
                    continue;
                }

                // 逐字打出
                for (int i = 0; i < c_PreviewText.Length; i++)
                {
                    m_TextAct.text += c_PreviewText[i];
                    yield return new WaitForSeconds(typeInterval);
                }

                // 打完后按跳过间隔等待，再循环
                yield return new WaitForSeconds(GlobalGameSettings.SkipDelay);
            }
        }

        // ================================================================
        // 关闭
        // ================================================================
        private void OnCloseClick()
        {
            StopTestAudio();

            if (m_PreviewCoroutine != null)
            {
                StopCoroutine(m_PreviewCoroutine);
                m_PreviewCoroutine = null;
            }

            GlobalAudioSettings.Save();
            GlobalGameSettings.Save();
            PlayerPrefs.Save();
            CloseSelf();
        }

        // ================================================================
        // 保存 / 加载
        // ================================================================
        private void LoadSettings()
        {
            m_IsLoading = true;

            // 音量 — 从全局设置同步到UI
            if (m_SliderAllAudio != null) m_SliderAllAudio.value = GlobalAudioSettings.AllAudioVolume;
            if (m_SliderBGM != null) m_SliderBGM.value = GlobalAudioSettings.BGMVolume;
            if (m_SliderSE != null) m_SliderSE.value = GlobalAudioSettings.SEVolume;
            if (m_SliderVoice != null) m_SliderVoice.value = GlobalAudioSettings.VoiceVolume;
            if (m_ToggleBGMWithSound != null) m_ToggleBGMWithSound.isOn = GlobalAudioSettings.BGMWithSound;

            // 操作 — 从全局设置同步到UI
            if (m_SliderWordsSpeed != null) m_SliderWordsSpeed.value = GlobalGameSettings.WordsSpeed;
            if (m_SliderActSpeed != null) m_SliderActSpeed.value = GlobalGameSettings.SkipInterval;
            if (m_ToggleSkipAll != null) m_ToggleSkipAll.isOn = GlobalGameSettings.SkipAll;
            if (m_ToggleSkipStop != null) m_ToggleSkipStop.isOn = GlobalGameSettings.SkipStop;

            // 画面
            bool fullScreen  = PlayerPrefs.GetInt(PREF_FULLSCREEN, 1) == 1;
            bool windowMode  = PlayerPrefs.GetInt(PREF_WINDOW_MODE, 0) == 1;
            bool borderless  = PlayerPrefs.GetInt(PREF_BORDERLESS_FS, 0) == 1;
            float brightness = PlayerPrefs.GetFloat(PREF_BRIGHTNESS, 1f);
            int resolution   = PlayerPrefs.GetInt(PREF_RESOLUTION, 0);
            int frameRate    = PlayerPrefs.GetInt(PREF_FRAME_RATE, 1);
            int quality      = PlayerPrefs.GetInt(PREF_QUALITY, QualitySettings.GetQualityLevel());

            if (m_ToggleFullScreen != null) m_ToggleFullScreen.isOn = fullScreen;
            if (m_ToggleWindowMode != null) m_ToggleWindowMode.isOn = windowMode;
            if (m_ToggleBorderlessFullScreen != null) m_ToggleBorderlessFullScreen.isOn = borderless;
            if (m_SliderBrightness != null) m_SliderBrightness.value = brightness;
            if (m_DropdownResolution != null) m_DropdownResolution.value = resolution;
            if (m_DropdownFrameRate != null) m_DropdownFrameRate.value = frameRate;
            if (m_DropdownQuality != null) m_DropdownQuality.value = quality;

            m_IsLoading = false;
        }

        /// <summary>
        /// OnOpen时更新所有Text显示，使其与加载的Slider值同步
        /// </summary>
        private void UpdateAllTexts()
        {
            UpdatePercentText(m_TextAllAudio,  GlobalAudioSettings.AllAudioVolume);
            UpdatePercentText(m_TextBGM,       GlobalAudioSettings.BGMVolume);
            UpdatePercentText(m_TextSE,        GlobalAudioSettings.SEVolume);
            UpdatePercentText(m_TextVoice,     GlobalAudioSettings.VoiceVolume);

            if (m_TextWordsSpeed != null)
                m_TextWordsSpeed.text = GlobalGameSettings.WordsSpeed.ToString("F1");
            if (m_TextActSpeed != null)
                m_TextActSpeed.text = GlobalGameSettings.SkipInterval.ToString("F1");

            float brightness = PlayerPrefs.GetFloat(PREF_BRIGHTNESS, 1f);
            UpdatePercentText(m_TextBrightness, brightness);
        }
    }
}
