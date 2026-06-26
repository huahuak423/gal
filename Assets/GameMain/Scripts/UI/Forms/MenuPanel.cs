//------------------------------------------------------------
// Game Framework
// AVG Game Project
//------------------------------------------------------------

using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;
using AVGGame;

namespace AVGGame
{
    /// <summary>
    /// 游戏内菜单 - 作为弹窗覆盖层
    /// 注意：Menu打开时不会关闭当前页面，用于对话等场景
    /// </summary>
    public class MenuPanel : UIFormBase
    {
        #region 序列化字段 - 按钮

        [Header("菜单按钮")]
        [SerializeField] private Button ButtonContinue;
        [SerializeField] private Button m_ButtonSave;
        [SerializeField] private Button m_ButtonLoad;        
        [SerializeField] private Button ButtonTimeLine;
        [SerializeField] private Button ButtonHistory;
        [SerializeField] private Button ButtonMainMenu;        
        [SerializeField] private Button m_ButtonSettings;
        [SerializeField] private Button m_ButtonExit;
        [SerializeField] private Button ButtonNPC;
        [SerializeField] private Button ButtonGallery;
        [SerializeField] private Button ButtonManual;

        [Header("透明背景 - 用于隔绝用户其他操作")]
        [SerializeField] private Button m_TransparentBgButton;

        #endregion

        #region 私有字段

        private ProcedureGame m_ProcedureGame = null;
        private bool m_IsPaused = false;

        #endregion

        #region 属性

        /// <summary>
        /// 高层级显示，确保覆盖在其他UI上方
        /// </summary>
        public override int SortingOrder => 200;

        #endregion

        #region 生命周期

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);

            // 挂载组件引用
            ButtonContinue = this.GetComponentByPath<Button>("Canvas/Background/MenuPlate/ButtonContinue");
            m_ButtonSave = this.GetComponentByPath<Button>("Canvas/Background/MenuPlate/ButtonSave");
            m_ButtonLoad = this.GetComponentByPath<Button>("Canvas/Background/MenuPlate/ButtonLoad");
            m_ButtonSettings = this.GetComponentByPath<Button>("Canvas/Background/MenuPlate/ButtonSetting");
            ButtonMainMenu = this.GetComponentByPath<Button>("Canvas/Background/MenuPlate/ButtonMainMenu");
            m_ButtonExit = this.GetComponentByPath<Button>("Canvas/Background/MenuPlate/ButtonExitGame");
            m_TransparentBgButton = this.GetComponentByPath<Button>("Canvas/Background");

            // 绑定按钮事件
            if (ButtonContinue != null)
                ButtonContinue.onClick.AddListener(OnResumeClick);

            if (m_ButtonSave != null)
                m_ButtonSave.onClick.AddListener(OnSaveClick);

            if (m_ButtonLoad != null)
                m_ButtonLoad.onClick.AddListener(OnLoadClick);

            if (m_ButtonSettings != null)
                m_ButtonSettings.onClick.AddListener(OnSettingClick);

            if (ButtonMainMenu != null)
                ButtonMainMenu.onClick.AddListener(OnBackClick);

            if (m_ButtonExit != null)
                m_ButtonExit.onClick.AddListener(OnExitClick);

            // 透明背景点击也关闭菜单
            if (m_TransparentBgButton != null)
                m_TransparentBgButton.onClick.AddListener(OnResumeClick);
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            if (userData is ProcedureGame procedureGame)
            {
                m_ProcedureGame = procedureGame;
            }

            // 暂停游戏
            PauseGame();
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);

            // 恢复游戏
            ResumeGame();
        }

        #endregion

        #region 按钮事件

        /// <summary>
        /// 继续游戏 - 关闭菜单
        /// </summary>
        private void OnResumeClick()
        {
            Log.Info("[MenuPanel] Resume clicked");
            CloseSelf();
        }

        /// <summary>
        /// 保存游戏 - 截图后打开存档选择界面（保存模式）
        /// </summary>
        private void OnSaveClick()
        {
            Log.Info("[MenuPanel] Save game clicked");
            StartCoroutine(CaptureScreenThenOpenArchive(ArchivePanel.ArchiveMode.Save));
        }

        /// <summary>
        /// 读档 - 打开存档选择界面（加载模式）
        /// </summary>
        private void OnLoadClick()
        {
            Log.Info("[MenuPanel] Load game clicked");
            CloseSelf();
            GameEntry.UI.OpenUIForm(
                AssetUtility.GetUIFormAsset(UIFormId.Archive),
                UIGroupDefinition.Popup,
                Constant.AssetPriority.UIAsset,
                ArchivePanel.ArchiveMode.Load
            );
        }

        /// <summary>
        /// 打开设置界面
        /// </summary>
        private void OnSettingClick()
        {
            Log.Info("[MenuPanel] Settings clicked");
            GameEntry.UI.OpenUIForm(
                AssetUtility.GetUIFormAsset(UIFormId.Settings),
                UIGroupDefinition.Popup,
                Constant.AssetPriority.UIAsset,
                null
            );
        }

        /// <summary>
        /// 返回主菜单 - 自动留档到第一个空位（无空位则跳过），然后直接返回
        /// </summary>
        private void OnBackClick()
        {
            Log.Info("[MenuPanel] Back to main menu clicked");
            StartCoroutine(CaptureAndAutoSaveThenReturnToMain());
        }

        /// <summary>
        /// 退出游戏
        /// </summary>
        private void OnExitClick()
        {
            if (m_ProcedureGame != null)
            {
                m_ProcedureGame.QuitGame();
            }
        }
        #endregion

        #region 截图与自动留档

        /// <summary>
        /// 截取当前游戏画面（隐藏菜单UI），然后打开存档面板
        /// </summary>
        private IEnumerator CaptureScreenThenOpenArchive(ArchivePanel.ArchiveMode mode)
        {
            // 临时隐藏菜单Canvas，确保截图是纯净的游戏画面
            Canvas menuCanvas = GetMenuCanvas();
            bool wasEnabled = menuCanvas?.enabled ?? true;
            if (menuCanvas != null) menuCanvas.enabled = false;

            yield return new WaitForEndOfFrame();

            // 截图 + 缩小为缩略图
            Texture2D thumbnail = CaptureThumbnail();

            // 恢复菜单Canvas
            if (menuCanvas != null) menuCanvas.enabled = wasEnabled;

            // 存入上下文
            if (thumbnail != null)
            {
                SaveLoadContext.PendingScreenshot = thumbnail;
            }

            // 关闭菜单并打开存档面板
            CloseSelf();
            GameEntry.UI.OpenUIForm(
                AssetUtility.GetUIFormAsset(UIFormId.Archive),
                UIGroupDefinition.Popup,
                Constant.AssetPriority.UIAsset,
                mode
            );
        }

        /// <summary>
        /// 截图 → 自动留档到第一个空位 → 返回主菜单
        /// </summary>
        private IEnumerator CaptureAndAutoSaveThenReturnToMain()
        {
            // 临时隐藏菜单Canvas
            Canvas menuCanvas = GetMenuCanvas();
            bool wasEnabled = menuCanvas?.enabled ?? true;
            if (menuCanvas != null) menuCanvas.enabled = false;

            yield return new WaitForEndOfFrame();

            // 截图
            Texture2D thumbnail = CaptureThumbnail();

            // 恢复菜单Canvas（虽然马上要关闭，但防止其他逻辑需要）
            if (menuCanvas != null) menuCanvas.enabled = wasEnabled;

            // 自动留档到第一个空位
            int emptySlot = FindFirstEmptySlot();
            if (emptySlot > 0)
            {
                // 保存截图文件
                if (thumbnail != null)
                {
                    try
                    {
                        string dir = Path.Combine(UnityEngine.Application.persistentDataPath, "Saves");
                        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                        string screenshotPath = Path.Combine(dir, $"thumb_{emptySlot}.png");
                        File.WriteAllBytes(screenshotPath, thumbnail.EncodeToPNG());
                    }
                    catch (System.Exception e)
                    {
                        Log.Warning($"[MenuPanel] 自动留档截图保存失败: {e.Message}");
                    }
                }

                // 保存游戏数据
                CustomEntry.PlayerData?.SaveGame(emptySlot);
                Log.Info($"[MenuPanel] 自动留档到槽位 {emptySlot}");
            }
            else
            {
                Log.Info("[MenuPanel] 无空余槽位，跳过自动留档");
            }

            // 清理截图纹理
            if (thumbnail != null)
            {
                Destroy(thumbnail);
            }

            // 返回主菜单
            if (m_ProcedureGame != null)
            {
                m_ProcedureGame.ReturnToMainMenu();
            }
            else
            {
                Log.Error("[MenuPanel] m_ProcedureGame is null, cannot return to main menu");
            }
        }

        /// <summary>
        /// 截取全屏并缩小为480px宽的缩略图
        /// </summary>
        private Texture2D CaptureThumbnail()
        {
            int captureWidth = Screen.width;
            int captureHeight = Screen.height;

            Texture2D fullScreen = new Texture2D(captureWidth, captureHeight, TextureFormat.RGB24, false);
            fullScreen.ReadPixels(new Rect(0, 0, captureWidth, captureHeight), 0, 0);
            fullScreen.Apply();

            // 缩小
            int thumbWidth = 480;
            float ratio = (float)captureHeight / captureWidth;
            int thumbHeight = Mathf.RoundToInt(thumbWidth * ratio);

            RenderTexture rt = RenderTexture.GetTemporary(thumbWidth, thumbHeight, 0);
            rt.filterMode = FilterMode.Bilinear;
            RenderTexture.active = rt;
            Graphics.Blit(fullScreen, rt);

            Texture2D thumbnail = new Texture2D(thumbWidth, thumbHeight, TextureFormat.RGB24, false);
            thumbnail.ReadPixels(new Rect(0, 0, thumbWidth, thumbHeight), 0, 0);
            thumbnail.Apply();

            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);
            Destroy(fullScreen);

            return thumbnail;
        }

        /// <summary>
        /// 获取菜单上的Canvas组件
        /// </summary>
        private Canvas GetMenuCanvas()
        {
            Transform canvasTrans = transform.Find("Canvas");
            return canvasTrans?.GetComponent<Canvas>();
        }

        /// <summary>
        /// 找到第一个空存档槽位（1~12），没有返回-1
        /// </summary>
        private int FindFirstEmptySlot()
        {
            var saveSystem = CustomEntry.SaveSystem;
            if (saveSystem == null) return -1;

            for (int i = 1; i <= 12; i++)
            {
                if (!saveSystem.HasSave(i))
                    return i;
            }
            return -1;
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 暂停游戏
        /// </summary>
        public void PauseGame()
        {
            m_IsPaused = true;
            Time.timeScale = 0f;
            Log.Info("[MenuPanel] Game paused");
        }

        /// <summary>
        /// 恢复游戏
        /// </summary>
        public void ResumeGame()
        {
            m_IsPaused = false;
            Time.timeScale = 1f;
            Log.Info("[MenuPanel] Game resumed");
        }

        /// <summary>
        /// 退出游戏
        /// </summary>
        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        #endregion
    }
}