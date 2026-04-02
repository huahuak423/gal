using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections.Generic;
using AVGGame;

namespace AVGGame.Editor
{
    /// <summary>
    /// 预览沙盒引擎：在主场景中创建隔离的 UI 预览
    ///
    /// 技术说明：
    /// 曾尝试使用 EditorSceneManager.NewPreviewScene() 方案，
    /// 但 Unity 的 Preview Scene 对 UGUI Canvas 渲染支持不完整，
    /// 导致 Canvas 内容无法正常显示（无论 WorldSpace 还是 ScreenSpaceCamera 模式）。
    ///
    /// 当前方案：在主场景实例化 UI，使用 HideAndDontSave 标记，
    /// 对象不会保存到场景，但会在 Scene 视图中可见。
    /// </summary>
    public class PreviewSandboxEngine
    {
        private GameObject m_UIInstance;
        private Canvas m_Canvas;
        private Camera m_RenderCamera;
        private RenderTexture m_RenderTexture;

        private const float k_CanvasWidth = 1920f;
        private const float k_CanvasHeight = 1080f;

        // 固定高分辨率渲染，避免模糊
        private const int k_RenderWidth = 1920;
        private const int k_RenderHeight = 1080;

        private float m_Zoom = 1f;
        private Vector2 m_PanOffset = Vector2.zero;

        // UI 组件缓存
        private Image m_BackgroundImage;
        private Text m_DialogueText;
        private Text m_CharacterNameText;
        private List<Image> m_CharacterImages = new List<Image>();

        public void Initialize(GameObject uiPrefab)
        {
            Cleanup();

            if (uiPrefab == null) return;

            // 实例化 UI 预制体
            m_UIInstance = Object.Instantiate(uiPrefab);
            m_UIInstance.SetActive(true);

            // 查找 Canvas
            m_Canvas = m_UIInstance.GetComponentInChildren<Canvas>();
            if (m_Canvas == null)
            {
                Debug.LogError("[预览] 没有找到 Canvas 组件！");
                return;
            }

            // 设置 Canvas 为 World Space 模式
            m_Canvas.renderMode = RenderMode.WorldSpace;
            RectTransform canvasRect = m_Canvas.GetComponent<RectTransform>();

            // 关键修复：重置 Canvas 的 scale（从 ScreenSpace-Overlay 转换时 scale 会变成 0,0,0）
            canvasRect.localScale = Vector3.one;
            canvasRect.localPosition = Vector3.zero;
            canvasRect.sizeDelta = new Vector2(k_CanvasWidth, k_CanvasHeight);

            // 创建渲染摄像机
            GameObject cameraObj = new GameObject("PreviewCamera");
            cameraObj.hideFlags = HideFlags.HideAndDontSave;
            m_RenderCamera = cameraObj.AddComponent<Camera>();
            m_RenderCamera.orthographic = true;
            m_RenderCamera.orthographicSize = k_CanvasHeight / 2f; // 540
            m_RenderCamera.nearClipPlane = 0.1f;
            m_RenderCamera.farClipPlane = 100f;
            m_RenderCamera.backgroundColor = new Color(0.2f, 0.2f, 0.3f, 1f);
            m_RenderCamera.clearFlags = CameraClearFlags.SolidColor;
            m_RenderCamera.cullingMask = -1; // 渲染所有层

            // 缓存 UI 组件
            CacheUIComponents();

            // 设置初始摄像机位置
            UpdateCameraPosition();

            // 隐藏到场景中（不干扰主场景，但会在 Scene 视图显示）
            m_UIInstance.hideFlags = HideFlags.HideAndDontSave;
            foreach (var trans in m_UIInstance.GetComponentsInChildren<Transform>())
            {
                trans.gameObject.hideFlags = HideFlags.HideAndDontSave;
            }
        }

        private void CacheUIComponents()
        {
            if (m_Canvas == null) return;

            // 查找背景图片
            var bgTransform = m_Canvas.transform.Find("Background");
            if (bgTransform != null)
            {
                m_BackgroundImage = bgTransform.GetComponent<Image>();

                // 查找立绘
                m_CharacterImages.Clear();
                var charPlate = bgTransform.Find("CharacterPlate");
                if (charPlate != null)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        var charImg = charPlate.Find($"CharacterImage{i}");
                        if (charImg != null)
                        {
                            var img = charImg.GetComponent<Image>();
                            if (img != null)
                            {
                                m_CharacterImages.Add(img);
                            }
                        }
                    }
                }

                // 查找对话框
                var textPlate = bgTransform.Find("TextPlate");
                if (textPlate != null)
                {
                    var nameTrans = textPlate.Find("CharacterName");
                    if (nameTrans != null)
                    {
                        m_CharacterNameText = nameTrans.GetComponentInChildren<Text>();
                    }

                    var dialoguePlate = textPlate.Find("DialoguePlate");
                    if (dialoguePlate != null)
                    {
                        m_DialogueText = dialoguePlate.GetComponentInChildren<Text>();
                    }
                }
            }
        }

        public void ApplySnapshot(StoryStateSnapshot snapshot)
        {
            if (snapshot == null) return;

            // 先隐藏所有立绘
            foreach (var img in m_CharacterImages)
            {
                img.gameObject.SetActive(false);
            }

            // 应用立绘：只处理 Left, Center, Right 三个位置 (枚举值 0, 1, 2)
            foreach (var charData in snapshot.CharacterRoster)
            {
                if (charData == null || string.IsNullOrEmpty(charData.SpritePath)) continue;

                // 只处理左中右三个基础位置，忽略 EX1-EX4
                int positionIndex = (int)charData.Position;
                if (positionIndex < 0 || positionIndex > 2) continue;
                if (positionIndex >= m_CharacterImages.Count) continue;

                var charSprite = AssetDatabase.LoadAssetAtPath<Sprite>(charData.SpritePath);
                if (charSprite != null)
                {
                    m_CharacterImages[positionIndex].sprite = charSprite;
                    m_CharacterImages[positionIndex].gameObject.SetActive(true);
                }
            }

            // 应用台词
            if (m_DialogueText != null)
            {
                m_DialogueText.text = snapshot.DialogText ?? "";
            }

            // 应用角色名
            if (m_CharacterNameText != null)
            {
                m_CharacterNameText.text = snapshot.CharacterName ?? "";
            }

            // 应用背景图（无论是否有值都要更新，避免残留旧背景）
            if (m_BackgroundImage != null)
            {
                if (!string.IsNullOrEmpty(snapshot.BackgroundPath))
                {
                    var bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(snapshot.BackgroundPath);
                    if (bgSprite != null)
                    {
                        m_BackgroundImage.sprite = bgSprite;
                    }
                }
                else
                {
                    // 没有背景图时，清空背景
                    m_BackgroundImage.sprite = null;
                }
            }
        }

        public Texture Render(Rect previewRect)
        {
            if (m_RenderCamera == null || m_Canvas == null) return null;

            // 使用固定高分辨率 RenderTexture，避免模糊
            if (m_RenderTexture == null)
            {
                m_RenderTexture = new RenderTexture(k_RenderWidth, k_RenderHeight, 24);
                m_RenderTexture.antiAliasing = 4; // 开启抗锯齿
                m_RenderCamera.targetTexture = m_RenderTexture;
            }

            // 渲染一帧
            m_RenderCamera.Render();

            return m_RenderTexture;
        }

        private void UpdateCameraPosition()
        {
            if (m_RenderCamera == null) return;

            float camZ = -10f;
            m_RenderCamera.transform.position = new Vector3(m_PanOffset.x, m_PanOffset.y, camZ);
            m_RenderCamera.orthographicSize = (k_CanvasHeight / 2f) / m_Zoom;
        }

        public void PanCamera(Vector2 mouseDelta)
        {
            float panSpeed = 2f / m_Zoom;
            m_PanOffset.x -= mouseDelta.x * panSpeed;
            m_PanOffset.y += mouseDelta.y * panSpeed;
            UpdateCameraPosition();
        }

        public void ZoomCamera(float scrollDelta)
        {
            float zoomFactor = 1f - scrollDelta * 0.1f;
            m_Zoom *= zoomFactor;
            m_Zoom = Mathf.Clamp(m_Zoom, 0.2f, 5f);
            UpdateCameraPosition();
        }

        public void ResetView()
        {
            m_Zoom = 1f;
            m_PanOffset = Vector2.zero;
            UpdateCameraPosition();
        }

        public void Cleanup()
        {
            if (m_RenderTexture != null)
            {
                m_RenderTexture.Release();
                m_RenderTexture = null;
            }

            if (m_RenderCamera != null)
            {
                Object.DestroyImmediate(m_RenderCamera.gameObject);
                m_RenderCamera = null;
            }

            if (m_UIInstance != null)
            {
                Object.DestroyImmediate(m_UIInstance);
                m_UIInstance = null;
            }

            m_Canvas = null;
            m_BackgroundImage = null;
            m_DialogueText = null;
            m_CharacterNameText = null;
            m_CharacterImages.Clear();
        }
    }
}
