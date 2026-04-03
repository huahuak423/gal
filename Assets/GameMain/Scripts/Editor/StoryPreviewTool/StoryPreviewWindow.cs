using UnityEngine;
using UnityEditor;
using XNode;
using AVGGame;
using System.Collections.Generic;

namespace AVGGame.Editor
{
    /// <summary>
    /// 剧情实时预览窗口
    /// </summary>
    public class StoryPreviewWindow : EditorWindow
    {
        private PreviewSandboxEngine m_SandboxEngine;

        [Tooltip("把你的对话 UI 预制体拖到这里")]
        public GameObject UIPrefab;

        // 当前选中的对话节点
        private DialogueNode m_SelectedNode;

        // 拖拽状态
        private bool m_IsDraggingSprite = false;
        private int m_DragOverSlotIndex = -1;

        // 立绘编排间距
        private float m_ArrangeSpacing = PreviewSandboxEngine.k_RecommendedSpacing;

        // 立绘编辑状态
        private bool m_IsEditingCharacter = false;
        private bool m_IsMovingCharacter = false;
        private Vector2 m_LastMousePos;
        private Vector2 m_DragOffset; // 鼠标和立绘中心之间的偏移量

        // 槽位颜色（半透明)
        private static readonly Color k_SlotColorLeft = new Color(1f, 0.3f, 0.4f);   // 红色 - 左
        private static readonly Color k_SlotColorCenter = new Color(0.3f, 1f, 0.4f); // 绿色 - 中
        private static readonly Color k_SlotColorRight = new Color(0.3f, 0.3f, 1f, 0.4f);  // 蓝色 - 右

        // 选中高亮颜色
        private static readonly Color k_SelectionColor = new Color(1f, 1f, 0f, 0.3f); // 黄色

        [MenuItem("Window/AVG/剧情实时预览 (Story Preview)")]
        public static void ShowWindow()
        {
            var window = GetWindow<StoryPreviewWindow>("剧情预览");
            window.minSize = new Vector2(500, 400);
        }

        private void OnEnable()
        {
            m_SandboxEngine = new PreviewSandboxEngine();
            Selection.selectionChanged += OnSelectionChanged;
        }

        private void OnDisable()
        {
            // 如果正在编辑立绘，先保存数据
            if (m_IsEditingCharacter)
            {
                SaveCharacterTransform();
            }

            Selection.selectionChanged -= OnSelectionChanged;
            if (m_SandboxEngine != null) m_SandboxEngine.Cleanup();
        }

        private void OnSelectionChanged()
        {
            // 如果正在编辑立绘，先保存数据
            if (m_IsEditingCharacter)
            {
                SaveCharacterTransform();
            }

            if (Selection.activeObject is DialogueNode selectedNode)
            {
                if (UIPrefab == null) return;

                // 检测是否切换了节点图
                StoryStateTracer.CheckAndClearHistoryIfGraphChanged(selectedNode);

                // 记录点击历史
                StoryStateTracer.RecordClick(selectedNode);

                // 生成快照
                StoryStateSnapshot snapshot = StoryStateTracer.GetSnapshot(selectedNode);

                // 应用快照
                m_SandboxEngine.ApplySnapshot(snapshot);

                // 记录当前节点
                m_SelectedNode = selectedNode;

                // 退出编辑状态
                m_IsEditingCharacter = false;
                m_SandboxEngine.DeselectCharacter();

                Repaint();
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            GUILayout.Label("🎬 AVG 剧情实时预览系统", EditorStyles.boldLabel);

            // 操作提示
            string helpText = m_IsEditingCharacter
                ? "💡 编辑模式: 左键拖拽移动 | 按Shift仅X轴 | 按住Ctrl仅Y轴 | 滚轮缩放 | 按 Z 键保存并退出"
                : "💡 操作: 从Project拖入图片 | 左键选中立绘 | 右键拖拽平移 | 滚轮缩放";
            GUILayout.Label(helpText, EditorStyles.helpBox);

            // UI预制体槽位
            EditorGUI.BeginChangeCheck();
            UIPrefab = (GameObject)EditorGUILayout.ObjectField("UI 预制体", UIPrefab, typeof(GameObject), false);
            if (EditorGUI.EndChangeCheck() && UIPrefab != null)
            {
                m_SandboxEngine.Initialize(UIPrefab);
                OnSelectionChanged();
            }

            if (UIPrefab == null)
            {
                EditorGUILayout.HelpBox("请先将包含了背景、立绘和对话框的 UI Prefab 拖入上方槽位。", MessageType.Info);
                return;
            }

            // 工具栏
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            // 重置视图按钮
            if (GUILayout.Button("重置视图", GUILayout.Width(80)))
            {
                m_SandboxEngine.ResetView();
                Repaint();
            }

            // 一键排位
            GUILayout.Label("间距:", GUILayout.Width(40));
            m_ArrangeSpacing = EditorGUILayout.FloatField(m_ArrangeSpacing, GUILayout.Width(60));
            if (GUILayout.Button("一键排位", GUILayout.Width(80)))
            {
                m_SandboxEngine.AutoArrangeCharacters(m_ArrangeSpacing);
                SaveCharacterTransform();
                Repaint();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(2);

            // 预览区域
            Rect previewRect = GUILayoutUtility.GetRect(position.width, position.width * (9f / 16f));

            // 渲染预览
            Texture previewTexture = m_SandboxEngine.Render(previewRect);
            if (previewTexture != null)
            {
                GUI.DrawTexture(previewRect, previewTexture, ScaleMode.ScaleToFit);
            }

            // 绘制拖拽槽位高亮
            if (m_IsDraggingSprite)
            {
                DrawDragSlots(previewRect);
            }

            // 绘制选中高亮（实心透明框）
            if (m_SandboxEngine.SelectedCharacterIndex >= 0)
            {
                DrawSelectionHighlight(previewRect);
            }

            // 处理事件
            HandleEvents(previewRect);
        }

        #region 私有方法

        private void DrawDragSlots(Rect previewRect)
        {
            // 计算三个槽位的区域
            float slotWidth = previewRect.width / 3f;
            float slotHeight = previewRect.height;

            Color[] colors = { k_SlotColorLeft, k_SlotColorCenter, k_SlotColorRight };
            string[] labels = { "左", "中", "右" };

            for (int i = 0; i < 3; i++)
            {
                // 如果槽位已有立绘，隐藏槽位显示
                if (m_SandboxEngine.IsSlotOccupied(i))
                {
                    continue;
                }

                Rect slotRect = new Rect(
                    previewRect.x + i * slotWidth,
                    previewRect.y,
                    slotWidth,
                    slotHeight
                );

                // 绘制半透明颜色
                EditorGUI.DrawRect(slotRect, colors[i]);

                // 绘制标签
                GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 24
                };
                GUI.Label(slotRect, labels[i], labelStyle);
            }

            // 高亮当前悬停的槽位
            if (m_DragOverSlotIndex >= 0 && m_DragOverSlotIndex < 3)
            {
                Rect highlightRect = new Rect(
                    previewRect.x + m_DragOverSlotIndex * slotWidth,
                    previewRect.y,
                    slotWidth,
                    slotHeight
                );
                EditorGUI.DrawRect(highlightRect, k_SelectionColor);
            }
        }

        private void DrawSelectionHighlight(Rect previewRect)
        {
            int index = m_SandboxEngine.SelectedCharacterIndex;
            if (index < 0) return;

            Vector2 center = m_SandboxEngine.GetCharacterScreenCenter(index, previewRect);

            // 固定大小的实心透明框（不随图片缩放）
            float fixedSize = 35f;
            Rect fixedRect = new Rect(
                center.x - fixedSize / 2f,
                center.y - fixedSize / 2f,
                fixedSize,
                fixedSize
            );

            // 绘制实心透明框
            Color fillColor = new Color(1f, 0.6f, 0f, 0.25f); // 半透明橙色
            EditorGUI.DrawRect(fixedRect, fillColor);
        }

        private void HandleEvents(Rect previewRect)
        {
            Event e = Event.current;
            Vector2 mousePos = e.mousePosition;

            if (!previewRect.Contains(mousePos)) return;

            // 拖拽检测
            HandleDragAndDrop(previewRect);

            // 立绘交互
            HandleCharacterInteraction(previewRect);
        }

        private void HandleDragAndDrop(Rect previewRect)
        {
            Event e = Event.current;
            Vector2 mousePos = e.mousePosition;

            if (e.type == EventType.DragPerform)
            {
                // 检查拖入的是否是图片
                var draggedObjects = DragAndDrop.objectReferences;
                foreach (var obj in draggedObjects)
                {
                    string spritePath = null;

                    if (obj is Sprite sprite)
                    {
                        spritePath = AssetDatabase.GetAssetPath(sprite);
                    }
                    else if (obj is Texture2D tex)
                    {
                        spritePath = AssetDatabase.GetAssetPath(tex);
                    }
                    else if (obj is DefaultAsset asset)
                    {
                        string path = AssetDatabase.GetAssetPath(asset);
                        if (!string.IsNullOrEmpty(path) && (path.EndsWith(".png") || path.EndsWith(".jpg")))
                        {
                            spritePath = path;
                        }
                    }

                    if (!string.IsNullOrEmpty(spritePath))
                    {
                        // 根据鼠标位置确定槽位
                        float relativeX = (mousePos.x - previewRect.x) / previewRect.width;
                        int slotIndex = Mathf.FloorToInt(relativeX * 3);
                        slotIndex = Mathf.Clamp(slotIndex, 0, 2);

                        // 添加立绘
                        AddCharacterToSlot(slotIndex, spritePath);
                        e.Use();
                        break;
                    }
                }
                m_IsDraggingSprite = false;
                Repaint();
            }
            else if (e.type == EventType.DragUpdated)
            {
                // 检查拖入的是否是有效图片类型
                bool isValidDrag = false;
                foreach (var obj in DragAndDrop.objectReferences)
                {
                    if (obj is Sprite || obj is Texture2D)
                    {
                        isValidDrag = true;
                        break;
                    }
                    else if (obj is DefaultAsset asset)
                    {
                        string path = AssetDatabase.GetAssetPath(asset);
                        if (!string.IsNullOrEmpty(path) && (path.EndsWith(".png") || path.EndsWith(".jpg")))
                        {
                            isValidDrag = true;
                            break;
                        }
                    }
                }

                if (isValidDrag)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy; // 关键：设置视觉模式
                    m_IsDraggingSprite = true;
                    // 更新悬停槽位
                    float relativeX = (mousePos.x - previewRect.x) / previewRect.width;
                    m_DragOverSlotIndex = Mathf.FloorToInt(relativeX * 3);
                    m_DragOverSlotIndex = Mathf.Clamp(m_DragOverSlotIndex, 0, 2);
                    e.Use();
                }
                else
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                }
                Repaint();
            }
            else if (e.type == EventType.DragExited)
            {
                m_IsDraggingSprite = false;
                m_DragOverSlotIndex = -1;
                Repaint();
            }
        }

        private void HandleCharacterInteraction(Rect previewRect)
        {
            Event e = Event.current;
            Vector2 mousePos = e.mousePosition;

            // 按 Z 键保存并退出编辑模式
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Z)
            {
                if (m_IsEditingCharacter)
                {
                    SaveCharacterTransform();
                    m_IsEditingCharacter = false;
                    m_SandboxEngine.DeselectCharacter();
                    e.Use();
                    Repaint();
                    Debug.Log("[预览] 已保存并退出编辑模式");
                }
                return;
            }

            // 滚轮缩放
            if (e.type == EventType.ScrollWheel)
            {
                if (m_IsEditingCharacter && m_SandboxEngine.SelectedCharacterIndex >= 0)
                {
                    // 缩放选中的立绘
                    m_SandboxEngine.ScaleSelectedCharacter(e.delta.y);
                    e.Use();
                    Repaint();
                }
                else
                {
                    // 缩放摄像机
                    m_SandboxEngine.ZoomCamera(e.delta.y);
                    e.Use();
                    Repaint();
                }
                return;
            }

            // 左键点击（编辑模式下禁止选中其他图片）
            if (e.type == EventType.MouseDown && e.button == 0 && !m_IsEditingCharacter)
            {
                // 使用固定大小的点击检测（点击选中框的位置）
                float clickSize = 35f; // 和选中框一样大
                int hitIndex = m_SandboxEngine.HitTestCharacterFixed(mousePos, previewRect, clickSize);

                if (hitIndex >= 0)
                {
                    m_SandboxEngine.SelectCharacter(hitIndex);
                    m_IsEditingCharacter = true;

                    // 记录鼠标和立绘中心之间的偏移量
                    Vector2 charCenter = m_SandboxEngine.GetCharacterScreenCenter(hitIndex, previewRect);
                    m_DragOffset = charCenter - mousePos;

                    m_LastMousePos = mousePos;
                    e.Use();
                    Repaint();
                }
            }

            // 左键拖拽移动立绘
            if (e.type == EventType.MouseDrag && e.button == 0 && m_IsEditingCharacter)
            {
                // 检查约束键
                bool constrainX = (e.modifiers & EventModifiers.Shift) != 0;
                bool constrainY = (e.modifiers & EventModifiers.Control) != 0;

                // 将鼠标位置和偏移量传给引擎进行移动
                m_SandboxEngine.MoveSelectedCharacterToPosition(mousePos, previewRect, m_DragOffset, constrainX, constrainY);

                m_LastMousePos = mousePos;
                e.Use();
                Repaint();
            }

            // 右键拖拽平移摄像机
            if (e.type == EventType.MouseDrag && e.button == 1 && !m_IsEditingCharacter)
            {
                m_SandboxEngine.PanCamera(e.delta);
                e.Use();
                Repaint();
            }

            // 双击右键重置视图
            if (e.type == EventType.MouseDown && e.button == 1 && e.clickCount == 2)
            {
                m_SandboxEngine.ResetView();
                e.Use();
                Repaint();
            }
        }

        private void AddCharacterToSlot(int slotIndex, string spritePath)
        {
            // 检查槽位是否已有立绘
            if (m_SandboxEngine.IsSlotOccupied(slotIndex))
            {
                Debug.LogWarning($"[预览] 槽位 {slotIndex} 已有立绘，请先删除后再添加。");
                return;
            }

            // 添加立绘到预览
            m_SandboxEngine.AddCharacterToSlot(slotIndex, spritePath);

            // 获取当前快照的记忆偏移
            float memoryOffsetX = 0;
            float memoryOffsetY = 0;
            float memoryScale = 1f;
            var snapshot = m_SandboxEngine.GetCurrentSnapshot();
            if (snapshot != null && snapshot.SlotOffsetMemory != null && slotIndex < snapshot.SlotOffsetMemory.Length)
            {
                memoryOffsetX = snapshot.SlotOffsetMemory[slotIndex].OffsetX;
                memoryOffsetY = snapshot.SlotOffsetMemory[slotIndex].OffsetY;
                memoryScale = snapshot.SlotOffsetMemory[slotIndex].Scale;

                // 应用记忆偏移到预览
                if (memoryOffsetX != 0 || memoryOffsetY != 0 || memoryScale != 1f)
                {
                    m_SandboxEngine.ApplySlotMemoryOffset(slotIndex, memoryOffsetX, memoryOffsetY, memoryScale);
                    Debug.Log($"[预览] 槽位 {slotIndex} 应用记忆偏移: ({memoryOffsetX:F1}, {memoryOffsetY:F1}), 缩放 {memoryScale:F2}");
                }
            }

            // 创建新的立绘数据
            var newCharData = new CharacterDisplayData
            {
                CharacterName = "",
                ActionType = CharacterActionType.Enter,
                Position = (CharacterPosition)slotIndex,
                SpritePath = spritePath,
                OffsetX = memoryOffsetX,
                OffsetY = memoryOffsetY,
                Scale = memoryScale
            };

            // 保存到当前节点
            if (m_SelectedNode != null)
            {
                // 添加到节点的立绘列表
                Undo.RecordObject(m_SelectedNode, "CharacterDisplays");
                m_SelectedNode.CharacterDisplays.Add(newCharData);

                // 标记节点为脏
                EditorUtility.SetDirty(m_SelectedNode);

                Debug.Log($"[预览] 添加立绘到槽位 {slotIndex}: {spritePath}" +
                    (memoryOffsetX != 0 || memoryOffsetY != 0 || memoryScale != 1f
                        ? $" (应用记忆偏移: ({memoryOffsetX:F1}, {memoryOffsetY:F1}), 缩放 {memoryScale:F2})"
                        : ""));
            }

            // 刷新预览（直接应用快照，避免依赖 Selection.activeObject 在拖拽时不可靠）
            if (snapshot != null)
            {
                // 将新添加的立绘加入快照的 CharacterRoster，以便 ApplySnapshot 能正确显示
                snapshot.CharacterRoster.Add(newCharData);
                m_SandboxEngine.ApplySnapshot(snapshot);
            }
            Repaint();
        }

        private void SaveCharacterTransform()
        {
            if (m_SelectedNode == null || m_SandboxEngine.SelectedCharacterIndex < 0) return;

            var (offsetX, offsetY, scale) = m_SandboxEngine.GetSelectedCharacterTransform();

            int slotIndex = m_SandboxEngine.SelectedCharacterIndex;

            // 找到对应的 CharacterDisplayData
            var snapshot = m_SandboxEngine.GetCurrentSnapshot();
            if (snapshot == null) return;

            CharacterDisplayData targetData = null;
            foreach (var charData in snapshot.CharacterRoster)
            {
                if ((int)charData.Position == slotIndex)
                {
                    targetData = charData;
                    break;
                }
            }

            if (targetData == null)
            {
                Debug.LogWarning($"[预览] 未找到槽位 {slotIndex} 对应的立绘数据");
                return;
            }

            // 更新数据
            Undo.RecordObject(m_SelectedNode, "CharacterDisplays");
            targetData.OffsetX = offsetX;
            targetData.OffsetY = offsetY;
            targetData.Scale = scale;

            EditorUtility.SetDirty(m_SelectedNode);

            Debug.Log($"[预览] 保存立绘变换: 槽位 {slotIndex}, 偏移({offsetX:F1}, {offsetY:F1}), 缩放 {scale:F2}");
        }

        #endregion
    }
}