using UnityEngine;
using UnityEditor;
using XNode;

namespace AVGGame.Editor
{
    /// <summary>
    /// 剧情实时预览窗口 (UI 界面)
    /// </summary>
    public class StoryPreviewWindow : EditorWindow
    {
        private PreviewSandboxEngine m_SandboxEngine;

        [Tooltip("把你的对话 UI 预制体拖到这里")]
        public GameObject UIPrefab;

        // 在顶部菜单栏注册入口
        [MenuItem("Window/AVG/剧情实时预览 (Story Preview)")]
        public static void ShowWindow()
        {
            // 创建并停靠窗口
            var window = GetWindow<StoryPreviewWindow>("剧情预览");
            window.minSize = new Vector2(400, 300);
        }

        private void OnEnable()
        {
            m_SandboxEngine = new PreviewSandboxEngine();
            // 监听鼠标在工程里或 xNode 里点选了什么东西
            Selection.selectionChanged += OnSelectionChanged;
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= OnSelectionChanged;
            // 窗口关闭时，必须把假场景炸掉，归还内存
            if (m_SandboxEngine != null) m_SandboxEngine.Cleanup();
        }

        /// <summary>
        /// 当策划在 xNode 里用鼠标点击节点时触发
        /// </summary>
        private void OnSelectionChanged()
        {
            // 【核心规则落实】：只响应 DialogueNode 的点击！
            // 如果点的是 ChoiceNode 或者 SubGraphNode，if 会直接跳过，
            // SandboxEngine 不会收到新的快照，从而完美保持上一次画面的残留！
            if (Selection.activeObject is DialogueNode selectedNode)
            {
                if (UIPrefab == null) return;

                // 检测是否切换了节点图，如果是则清空历史记录
                StoryStateTracer.CheckAndClearHistoryIfGraphChanged(selectedNode);

                // 1. 喂给大脑，记录点击历史（用于解决分支悖论）
                StoryStateTracer.RecordClick(selectedNode);

                // 2. 呼叫大脑逆向爬树，生成快照
                StoryStateSnapshot snapshot = StoryStateTracer.GetSnapshot(selectedNode);

                // 3. 把快照发给沙盒引擎，给假 UI 换皮
                m_SandboxEngine.ApplySnapshot(snapshot);

                // 4. 通知当前窗口立刻刷新重绘 (调用一次 OnGUI)
                Repaint();
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            GUILayout.Label("🎬 AVG 剧情实时预览系统", EditorStyles.boldLabel);
            GUILayout.Label("💡 操作: 滚轮缩放 | 右键拖拽平移 | 中键拖拽平移", EditorStyles.helpBox);

            // 绘制一个可以让策划拖拽 Prefab 的槽位
            EditorGUI.BeginChangeCheck();
            UIPrefab = (GameObject)EditorGUILayout.ObjectField("UI 预制体", UIPrefab, typeof(GameObject), false);
            if (EditorGUI.EndChangeCheck() && UIPrefab != null)
            {
                // 如果策划刚拖进了一个预制体，立刻初始化影棚
                m_SandboxEngine.Initialize(UIPrefab);
                OnSelectionChanged(); // 尝试渲染一下当前选中的节点
            }

            if (UIPrefab == null)
            {
                EditorGUILayout.HelpBox("请先将包含了背景、立绘和对话框的 UI Prefab 拖入上方槽位。", MessageType.Info);
                return;
            }

            // 工具栏：重置视图按钮
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("重置视图", GUILayout.Width(80)))
            {
                m_SandboxEngine.ResetView();
                Repaint();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(2);

            // 划出一块区域用来显示照片，这里我们强制按 16:9 的比例来画
            Rect previewRect = GUILayoutUtility.GetRect(position.width, position.width * (9f / 16f));

            /// 1. 呼叫沙盒引擎按下快门
            Texture previewTexture = m_SandboxEngine.Render(previewRect);
            if (previewTexture != null)
            {
                GUI.DrawTexture(previewRect, previewTexture, ScaleMode.ScaleToFit);
            }

            // 2. 处理鼠标的漫游控制
            HandleCameraControls(previewRect);
        }

        /// <summary>
        /// 拦截鼠标事件并指挥摄像机移动
        /// </summary>
        private void HandleCameraControls(Rect previewRect)
        {
            Event e = Event.current;
            Vector2 mousePos = e.mousePosition;

            // 如果鼠标不在画面区域内，就不响应
            if (!previewRect.Contains(mousePos)) return;

            // 检测：鼠标滚轮 (缩放)
            if (e.type == EventType.ScrollWheel)
            {
                m_SandboxEngine.ZoomCamera(e.delta.y);
                e.Use();
                Repaint();
                return;
            }

            // 检测：鼠标拖拽 (右键=1 或 中键=2)
            if (e.type == EventType.MouseDrag && (e.button == 1 || e.button == 2))
            {
                m_SandboxEngine.PanCamera(e.delta);
                e.Use();
                Repaint();
                return;
            }

            // 检测：双击右键重置视图
            if (e.type == EventType.MouseDown && e.button == 1 && e.clickCount == 2)
            {
                m_SandboxEngine.ResetView();
                e.Use();
                Repaint();
                return;
            }
        }
    }
}
