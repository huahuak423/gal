using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections.Generic;
using AVGGame;

namespace AVGGame.Editor
{
    /// <summary>
    /// 预览沙盒引擎：在主场景中创建隔离的 UI 预览
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
        private List<RectTransform> m_CharacterRects = new List<RectTransform>();

        // 立绘原始位置（用于重置）
        private List<Vector2> m_OriginalPositions = new List<Vector2>();

        // 当前快照缓存（用于获取立绘数据）
        private StoryStateSnapshot m_CurrentSnapshot;

        // 当前选中的立绘槽位（-1 = 未选中）
        public int SelectedCharacterIndex { get; private set; } = -1;

        // 推荐的立绘间距（基于2500x3971像素立绘在1080p屏幕上）
        public const float k_RecommendedSpacing = 300f;

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
            canvasRect.localScale = Vector3.one;
            canvasRect.localPosition = Vector3.zero;
            canvasRect.sizeDelta = new Vector2(k_CanvasWidth, k_CanvasHeight);

            // 创建渲染摄像机
            GameObject cameraObj = new GameObject("PreviewCamera");
            cameraObj.hideFlags = HideFlags.HideAndDontSave;
            m_RenderCamera = cameraObj.AddComponent<Camera>();
            m_RenderCamera.orthographic = true;
            m_RenderCamera.orthographicSize = k_CanvasHeight / 2f;
            m_RenderCamera.nearClipPlane = 0.1f;
            m_RenderCamera.farClipPlane = 100f;
            m_RenderCamera.backgroundColor = new Color(0.2f, 0.2f, 0.3f, 1f);
            m_RenderCamera.clearFlags = CameraClearFlags.SolidColor;
            m_RenderCamera.cullingMask = -1;

            // 缓存 UI 组件
            CacheUIComponents();

            // 设置初始摄像机位置
            UpdateCameraPosition();

            // 隐藏到场景中
            m_UIInstance.hideFlags = HideFlags.HideAndDontSave;
            foreach (var trans in m_UIInstance.GetComponentsInChildren<Transform>())
            {
                trans.gameObject.hideFlags = HideFlags.HideAndDontSave;
            }
        }

        private void CacheUIComponents()
        {
            if (m_Canvas == null) return;

            m_CharacterImages.Clear();
            m_CharacterRects.Clear();
            m_OriginalPositions.Clear();

            var bgTransform = m_Canvas.transform.Find("Background");
            if (bgTransform != null)
            {
                m_BackgroundImage = bgTransform.GetComponent<Image>();

                var charPlate = bgTransform.Find("CharacterPlate");
                if (charPlate != null)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        var charImg = charPlate.Find($"CharacterImage{i}");
                        if (charImg != null)
                        {
                            var img = charImg.GetComponent<Image>();
                            var rect = charImg.GetComponent<RectTransform>();
                            if (img != null && rect != null)
                            {
                                m_CharacterImages.Add(img);
                                m_CharacterRects.Add(rect);
                                m_OriginalPositions.Add(rect.anchoredPosition);
                            }
                        }
                    }
                }

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

            m_CurrentSnapshot = snapshot;
            SelectedCharacterIndex = -1; // 重置选中状态

            // 先隐藏所有立绘
            foreach (var img in m_CharacterImages)
            {
                img.gameObject.SetActive(false);
            }

            // 应用立绘：只处理 Left, Center, Right 三个位置 (枚举值 0, 1, 2)
            foreach (var charData in snapshot.CharacterRoster)
            {
                if (charData == null || string.IsNullOrEmpty(charData.SpritePath)) continue;

                int positionIndex = (int)charData.Position;
                if (positionIndex < 0 || positionIndex > 2) continue;
                if (positionIndex >= m_CharacterImages.Count) continue;

                var charSprite = AssetDatabase.LoadAssetAtPath<Sprite>(charData.SpritePath);
                if (charSprite != null)
                {
                    m_CharacterImages[positionIndex].sprite = charSprite;
                    m_CharacterImages[positionIndex].gameObject.SetActive(true);

                    // 应用偏移和缩放
                    ApplyCharacterTransform(positionIndex, charData);
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

            // 应用背景图
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
                    m_BackgroundImage.sprite = null;
                }
            }
        }

        /// <summary>
        /// 应用单个立绘的变换（偏移和缩放）
        /// </summary>
        private void ApplyCharacterTransform(int index, CharacterDisplayData data)
        {
            if (index < 0 || index >= m_CharacterRects.Count) return;

            var rect = m_CharacterRects[index];

            // 应用偏移（基于原始位置）
            Vector2 originalPos = m_OriginalPositions[index];
            rect.anchoredPosition = originalPos + new Vector2(data.OffsetX, data.OffsetY);

            // 应用缩放
            rect.localScale = Vector3.one * data.Scale;
        }

        /// <summary>
        /// 获取立绘在屏幕空间的包围盒（用于点击检测）
        /// </summary>
        public Rect GetCharacterScreenRect(int index, Rect previewRect)
        {
            if (index < 0 || index >= m_CharacterRects.Count) return Rect.zero;
            if (!m_CharacterImages[index].gameObject.activeSelf) return Rect.zero;

            var rectTransform = m_CharacterRects[index];
            var image = m_CharacterImages[index];

            // 获取实际显示的 sprite 尺寸
            Vector2 displaySize;
            if (image.sprite != null)
            {
                // 使用 sprite 的实际尺寸，考虑缩放
                displaySize = image.sprite.rect.size * rectTransform.localScale.x;
            }
            else
            {
                displaySize = rectTransform.sizeDelta * rectTransform.localScale.x;
            }

            // 获取 RectTransform 的中心位置（Canvas 坐标）
            Vector2 center = rectTransform.anchoredPosition;

            // 考虑 pivot 偏移
            Vector2 pivotOffset = new Vector2(
                (rectTransform.pivot.x - 0.5f) * rectTransform.sizeDelta.x,
                (rectTransform.pivot.y - 0.5f) * rectTransform.sizeDelta.y
            );
            center += pivotOffset;

            // 转换到屏幕坐标（Canvas中心为原点）
            float screenX = (center.x + k_CanvasWidth / 2f) / k_CanvasWidth * previewRect.width + previewRect.x;
            float screenY = (k_CanvasHeight / 2f - center.y) / k_CanvasHeight * previewRect.height + previewRect.y;

            // 考虑摄像机缩放和平移
            screenX = (screenX - previewRect.x - previewRect.width / 2f) * m_Zoom + previewRect.width / 2f + m_PanOffset.x * previewRect.width / k_CanvasWidth * m_Zoom + previewRect.x;
            screenY = (screenY - previewRect.y - previewRect.height / 2f) * m_Zoom + previewRect.height / 2f - m_PanOffset.y * previewRect.height / k_CanvasHeight * m_Zoom + previewRect.y;

            float screenW = displaySize.x / k_CanvasWidth * previewRect.width * m_Zoom;
            float screenH = displaySize.y / k_CanvasHeight * previewRect.height * m_Zoom;

            return new Rect(screenX - screenW / 2f, screenY - screenH / 2f, screenW, screenH);
        }

        /// <summary>
        /// 检测屏幕坐标点击了哪个立绘（使用固定大小的点击区域）
        /// </summary>
        public int HitTestCharacterFixed(Vector2 screenPos, Rect previewRect, float fixedSize = 60f)
        {
            for (int i = m_CharacterImages.Count - 1; i >= 0; i--) // 从后往前检测（后面的在上层）
            {
                if (!m_CharacterImages[i].gameObject.activeSelf) continue;

                Vector2 center = GetCharacterScreenCenter(i, previewRect);
                Rect fixedRect = new Rect(
                    center.x - fixedSize / 2f,
                    center.y - fixedSize / 2f,
                    fixedSize,
                    fixedSize
                );
                if (fixedRect.Contains(screenPos))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// 获取立绘中心的屏幕坐标
        /// </summary>
        public Vector2 GetCharacterScreenCenter(int index, Rect previewRect)
        {
            if (index < 0 || index >= m_CharacterRects.Count) return Vector2.zero;
            if (!m_CharacterImages[index].gameObject.activeSelf) return Vector2.zero;

            var rectTransform = m_CharacterRects[index];

            // 获取 RectTransform 的中心位置（Canvas 坐标）
            Vector2 center = rectTransform.anchoredPosition;

            // 考虑 pivot 偏移
            Vector2 pivotOffset = new Vector2(
                (rectTransform.pivot.x - 0.5f) * rectTransform.sizeDelta.x,
                (rectTransform.pivot.y - 0.5f) * rectTransform.sizeDelta.y
            );
            center += pivotOffset;

            // 转换到屏幕坐标（Canvas中心为原点）
            float screenX = (center.x + k_CanvasWidth / 2f) / k_CanvasWidth * previewRect.width + previewRect.x;
            float screenY = (k_CanvasHeight / 2f - center.y) / k_CanvasHeight * previewRect.height + previewRect.y;

            // 考虑摄像机缩放和平移
            screenX = (screenX - previewRect.x - previewRect.width / 2f) * m_Zoom + previewRect.width / 2f + m_PanOffset.x * previewRect.width / k_CanvasWidth * m_Zoom + previewRect.x;
            screenY = (screenY - previewRect.y - previewRect.height / 2f) * m_Zoom + previewRect.height / 2f - m_PanOffset.y * previewRect.height / k_CanvasHeight * m_Zoom + previewRect.y;

            return new Vector2(screenX, screenY);
        }

        /// <summary>
        /// 选中立绘
        /// </summary>
        public void SelectCharacter(int index)
        {
            SelectedCharacterIndex = index;
        }

        /// <summary>
        /// 取消选中
        /// </summary>
        public void DeselectCharacter()
        {
            SelectedCharacterIndex = -1;
        }

        /// <summary>
        /// 移动选中的立绘到指定的屏幕位置（保持偏移量）
        /// </summary>
        public void MoveSelectedCharacterToPosition(Vector2 mouseScreenPos, Rect previewRect, Vector2 dragOffset, bool constrainX = false, bool constrainY = false)
        {
            if (SelectedCharacterIndex < 0 || SelectedCharacterIndex >= m_CharacterRects.Count) return;

            // 计算目标立绘中心位置（鼠标位置 + 偏移量）
            Vector2 targetCenter = mouseScreenPos + dragOffset;

            // 目标屏幕位置转换为 Canvas 坐标
            // 先转换到相对于 previewRect 中心的坐标
            float relX = (targetCenter.x - previewRect.x - previewRect.width / 2f) / previewRect.width;
            float relY = (targetCenter.y - previewRect.y - previewRect.height / 2f) / previewRect.height;

            // 考虑缩放和平移
            float canvasX = relX * k_CanvasWidth / m_Zoom + m_PanOffset.x;
            float canvasY = -relY * k_CanvasHeight / m_Zoom + m_PanOffset.y;

            // 约束
            var rect = m_CharacterRects[SelectedCharacterIndex];
            Vector2 newPos = rect.anchoredPosition;
            if (!constrainX) newPos.x = canvasX;
            if (!constrainY) newPos.y = canvasY;

            rect.anchoredPosition = newPos;
        }

        /// <summary>
        /// 缩放选中的立绘
        /// </summary>
        public void ScaleSelectedCharacter(float scaleDelta)
        {
            if (SelectedCharacterIndex < 0 || SelectedCharacterIndex >= m_CharacterRects.Count) return;

            var rect = m_CharacterRects[SelectedCharacterIndex];
            float newScale = rect.localScale.x * (1f - scaleDelta * 0.02f);
            newScale = Mathf.Clamp(newScale, 0.1f, 3f);
            rect.localScale = Vector3.one * newScale;
        }

               /// <summary>
        /// 获取选中立绘的当前偏移和缩放数据
        /// </summary>
        public (float offsetX, float offsetY, float scale) GetSelectedCharacterTransform()
        {
            if (SelectedCharacterIndex < 0 || SelectedCharacterIndex >= m_CharacterRects.Count)
                return (0, 0, 1);

            var rect = m_CharacterRects[SelectedCharacterIndex];
            Vector2 originalPos = m_OriginalPositions[SelectedCharacterIndex];
            Vector2 offset = rect.anchoredPosition - originalPos;
            float scale = rect.localScale.x;

            return (offset.x, offset.y, scale);
        }

        /// <summary>
        /// 一键排位立绘
        /// </summary>
        public void AutoArrangeCharacters(float spacing)
        {
            // 统计当前有多少个立绘
            List<int> activeIndices = new List<int>();
            for (int i = 0; i < m_CharacterImages.Count; i++)
            {
                if (m_CharacterImages[i].gameObject.activeSelf)
                {
                    activeIndices.Add(i);
                }
            }

            if (activeIndices.Count == 0) return;

            // 计算居中排列的起始位置
            float totalWidth = (activeIndices.Count - 1) * spacing;
            float startX = -totalWidth / 2f;

            // 设置每个立绘的位置
            for (int i = 0; i < activeIndices.Count; i++)
            {
                int idx = activeIndices[i];
                float targetX = startX + i * spacing;
                float originalY = m_OriginalPositions[idx].y;

                m_CharacterRects[idx].anchoredPosition = new Vector2(targetX, originalY);
                m_OriginalPositions[idx] = new Vector2(m_OriginalPositions[idx].x, originalY); // 更新原始位置的Y
            }
        }

        /// <summary>
        /// 添加立绘到指定槽位
        /// </summary>
        public void AddCharacterToSlot(int slotIndex, string spritePath)
        {
            if (slotIndex < 0 || slotIndex >= m_CharacterImages.Count) return;

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (sprite != null)
            {
                m_CharacterImages[slotIndex].sprite = sprite;
                m_CharacterImages[slotIndex].gameObject.SetActive(true);

                // 重置变换
                m_CharacterRects[slotIndex].anchoredPosition = m_OriginalPositions[slotIndex];
                m_CharacterRects[slotIndex].localScale = Vector3.one;
            }
        }

        /// <summary>
        /// 添加立绘到指定槽位，并应用指定的偏移和缩放
        /// </summary>
        public void AddCharacterToSlotWithOffset(int slotIndex, string spritePath, float offsetX, float offsetY, float scale)
        {
            if (slotIndex < 0 || slotIndex >= m_CharacterImages.Count) return;

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (sprite != null)
            {
                m_CharacterImages[slotIndex].sprite = sprite;
                m_CharacterImages[slotIndex].gameObject.SetActive(true);

                // 应用偏移和缩放
                m_CharacterRects[slotIndex].anchoredPosition = m_OriginalPositions[slotIndex] + new Vector2(offsetX, offsetY);
                m_CharacterRects[slotIndex].localScale = Vector3.one * scale;
            }
        }

        /// <summary>
        /// 获取槽位是否有立绘
        /// </summary>
        public bool IsSlotOccupied(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= m_CharacterImages.Count) return false;
            return m_CharacterImages[slotIndex].gameObject.activeSelf;
        }

        /// <summary>
        /// 获取当前快照
        /// </summary>
        public StoryStateSnapshot GetCurrentSnapshot()
        {
            return m_CurrentSnapshot;
        }

        public Texture Render(Rect previewRect)
        {
            if (m_RenderCamera == null || m_Canvas == null) return null;

            if (m_RenderTexture == null)
            {
                m_RenderTexture = new RenderTexture(k_RenderWidth, k_RenderHeight, 24);
                m_RenderTexture.antiAliasing = 4;
                m_RenderCamera.targetTexture = m_RenderTexture;
            }

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
            m_CharacterRects.Clear();
            m_OriginalPositions.Clear();
            m_CurrentSnapshot = null;
            SelectedCharacterIndex = -1;
        }
    }
}
