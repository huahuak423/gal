# StoryPreviewTool 详细操作文档

本文档详细介绍 `StoryPreviewTool` 模块的所有类、方法及其用法。

---

## 目录

1. [概述](#概述)
2. [CharacterMemoryData.cs](#charactermemorydatacs)
3. [CharacterMemoryEditorWindow.cs](#charactermemoryeditorwindowcs)
4. [PreviewSandboxEngine.cs](#previewsandboxenginecs)
5. [StoryStateSnapshot.cs](#storystatesnapshotcs)
6. [StoryStateTracer.cs](#storystatetracercs)
7. [StoryPreviewWindow.cs](#storypreviewwindowcs)
8. [核心API参考](#核心api参考)

---

## 概述

StoryPreviewTool 是一个 Unity Editor 扩展工具集，用于在编辑器中实时预览 AVG 剧情效果。主要功能包括：

- **剧情预览**：实时渲染对话界面，包括背景图、立绘、台词
- **立绘编辑**：拖拽、缩放、偏移立绘，并将修改保存到节点
- **偏移记忆**：记录每个立绘在不同节点图、不同槽位的偏移和缩放
- **状态溯源**：正向遍历节点图，计算某个节点的完整画面状态

---

## CharacterMemoryData.cs

### 文件作用

定义立绘偏移记忆的数据结构和管理器，用于持久化存储立绘的偏移和缩放信息。

### 类定义

#### CharacterMemoryEntry

立绘偏移记忆条目，记录单个立绘在特定节点图、特定槽位的偏移和缩放。

```csharp
[Serializable]
public class CharacterMemoryEntry
{
    public string GraphName;           // 节点图名称
    public string SpritePath;          // 立绘资源路径
    public CharacterPosition Position; // 槽位 (Left/Center/Right)
    public float OffsetX;              // X轴偏移
    public float OffsetY;              // Y轴偏移
    public float Scale = 1f;           // 缩放比例
    public string LastNodeGuid;        // 最后一次设置的节点GUID

    public string GetKey();            // 生成唯一标识键
}
```

| 字段 | 类型 | 说明 |
|------|------|------|
| `GraphName` | string | 节点图资产名称，用于区分不同节点图的记忆 |
| `SpritePath` | string | 立绘图片的资源路径，如 `"Assets/Art/Characters/温叙/温叙_普通.png"` |
| `Position` | CharacterPosition | 立绘槽位，枚举值：`Left(0)`, `Center(1)`, `Right(2)` |
| `OffsetX` | float | X轴偏移量，正值向右 |
| `OffsetY` | float | Y轴偏移量，正值向上 |
| `Scale` | float | 缩放比例，1.0 = 原始大小 |
| `LastNodeGuid` | string | 最后一次设置该记忆的节点名称 |

**GetKey()** 方法：
- **返回值**：`string`，格式为 `"{GraphName}|{SpritePath}|{(int)Position}"`
- **用途**：生成唯一键用于字典查找

---

#### CharacterMemoryData

用于 JSON 序列化的包装类。

```csharp
[Serializable]
public class CharacterMemoryData
{
    public List<CharacterMemoryEntry> Entries = new List<CharacterMemoryEntry>();
}
```

---

#### CharacterMemoryManager

立绘偏移记忆管理器（单例），负责记忆的增删改查和持久化。

```csharp
public class CharacterMemoryManager
{
    // 单例访问
    public static CharacterMemoryManager Instance { get; }

    // 当前保存路径（可修改）
    public string CurrentSavePath { get; set; }
}
```

### 方法详解

#### RecordMemory

记录一条立绘偏移记忆。

```csharp
public void RecordMemory(
    string graphName,
    string spritePath,
    CharacterPosition position,
    float offsetX,
    float offsetY,
    float scale,
    string nodeGuid = ""
)
```

| 参数 | 类型 | 说明 |
|------|------|------|
| `graphName` | string | 节点图名称 |
| `spritePath` | string | 立绘资源路径 |
| `position` | CharacterPosition | 槽位 |
| `offsetX` | float | X轴偏移 |
| `offsetY` | float | Y轴偏移 |
| `scale` | float | 缩放比例 |
| `nodeGuid` | string | 可选，节点标识 |

**行为**：如果相同键已存在，会覆盖旧记录。

---

#### TryGetMemory

尝试获取立绘偏移记忆。

```csharp
public bool TryGetMemory(
    string graphName,
    string spritePath,
    CharacterPosition position,
    out float offsetX,
    out float offsetY,
    out float scale
)
```

| 参数 | 类型 | 说明 |
|------|------|------|
| `graphName` | string | 节点图名称 |
| `spritePath` | string | 立绘资源路径 |
| `position` | CharacterPosition | 槽位 |
| `offsetX` | out float | 输出：X轴偏移 |
| `offsetY` | out float | 输出：Y轴偏移 |
| `scale` | out float | 输出：缩放比例 |

**返回值**：`bool`，是否找到匹配的记忆

---

#### GetMemoriesByGraphAndSprite

获取指定节点图和立绘的所有位置记忆。

```csharp
public List<CharacterMemoryEntry> GetMemoriesByGraphAndSprite(
    string graphName,
    string spritePath
)
```

**返回值**：该立绘在该节点图中所有槽位的记忆列表

---

#### GetMemoriesByGraph

获取指定节点图的所有记忆。

```csharp
public List<CharacterMemoryEntry> GetMemoriesByGraph(string graphName)
```

---

#### GetMemoriesBySprite

获取指定立绘跨节点图的所有记忆。

```csharp
public List<CharacterMemoryEntry> GetMemoriesBySprite(string spritePath)
```

---

#### Load / Save

从磁盘加载 / 保存到磁盘。

```csharp
public void Load()   // 从 CurrentSavePath 加载 JSON
public void Save()   // 保存到 CurrentSavePath
```

**默认保存路径**：`{项目根目录}/UserData/CharacterMemory.json`

---

#### ClearGraphMemory / ClearAll / RemoveMemory

清除记忆。

```csharp
public void ClearGraphMemory(string graphName)  // 清除指定节点图的所有记忆
public void ClearAll()                          // 清除所有记忆
public void RemoveMemory(string graphName, string spritePath, CharacterPosition position)  // 删除单条
```

---

#### CopyMemoriesFrom

从源节点图复制记忆到目标节点图。

```csharp
public void CopyMemoriesFrom(string sourceGraphName, string targetGraphName)
```

---

#### GetAllMemories

获取所有记忆条目。

```csharp
public IEnumerable<CharacterMemoryEntry> GetAllMemories()
```

---

## CharacterMemoryEditorWindow.cs

### 文件作用

立绘记忆可视化编辑器窗口，提供记忆的查看、编辑、导入导出功能。

### 打开方式

```
菜单栏 → AVG Tools → 立绘记忆管理
```

### 类定义

```csharp
public class CharacterMemoryEditorWindow : EditorWindow
```

### 窗口功能

1. **记忆路径设置**：可修改记忆文件的保存路径
2. **参考复制**：从一个节点图复制记忆到另一个节点图
3. **记忆列表**：支持按节点图或按立绘分组显示
4. **编辑功能**：直接修改偏移和缩放值
5. **导入导出**：支持 CSV 格式

### 主要方法

#### ShowWindow

打开窗口。

```csharp
[MenuItem("AVG Tools/立绘记忆管理")]
public static void ShowWindow()
```

#### DrawToolbar

绘制工具栏按钮。

```csharp
private void DrawToolbar()
```

包含：
- 从节点图导入记忆
- 导出记忆到CSV
- 从CSV导入

#### DrawMemoryList

绘制记忆列表。

```csharp
private void DrawMemoryList()
```

支持三种显示模式：
- 按节点图分组 (`m_GroupByGraph = true`)
- 按立绘分组 (`m_GroupBySprite = true`)
- 扁平列表

#### ExportToCsv / ImportFromCsv

CSV 导入导出。

```csharp
private void ExportToCsv()  // 导出为 CSV 文件
private void ImportFromCsv() // 从 CSV 文件导入
```

**CSV 格式**：
```
节点图,立绘路径,槽位,偏移X,偏移Y,缩放,最后节点GUID
```

---

## PreviewSandboxEngine.cs

### 文件作用

预览沙盒引擎：在编辑器中创建隔离的 UI 预览环境，负责渲染背景图、立绘、台词。

### 类定义

```csharp
public class PreviewSandboxEngine
{
    private GameObject m_UIInstance;      // UI 实例
    private Canvas m_Canvas;              // Canvas 组件
    private Camera m_RenderCamera;        // 渲染摄像机
    private RenderTexture m_RenderTexture; // 渲染纹理

    private const float k_CanvasWidth = 1920f;   // Canvas 宽度
    private const float k_CanvasHeight = 1080f;  // Canvas 高度
    private const int k_RenderWidth = 1920;      // 渲染分辨率宽
    private const int k_RenderHeight = 1080;     // 渲染分辨率高

    public int SelectedCharacterIndex { get; }  // 当前选中的立绘槽位
    public const float k_RecommendedSpacing = 300f;  // 推荐立绘间距
}
```

### 方法详解

#### Initialize

初始化沙盒引擎。

```csharp
public void Initialize(GameObject uiPrefab)
```

| 参数 | 类型 | 说明 |
|------|------|------|
| `uiPrefab` | GameObject | 对话 UI 预制体 |

**行为**：
1. 实例化 UI 预制体
2. 设置 Canvas 为 World Space 模式
3. 创建正交摄像机用于渲染
4. 缓存 UI 组件（背景图、立绘、台词等）

---

#### ApplySnapshot

应用状态快照到预览。

```csharp
public void ApplySnapshot(StoryStateSnapshot snapshot)
```

| 参数 | 类型 | 说明 |
|------|------|------|
| `snapshot` | StoryStateSnapshot | 剧情状态快照 |

**行为**：
1. 隐藏所有立绘
2. 根据 `CharacterRoster` 显示立绘
3. 应用台词和角色名
4. 应用背景图

---

#### Render

渲染预览画面。

```csharp
public Texture Render(Rect previewRect)
```

| 参数 | 类型 | 说明 |
|------|------|------|
| `previewRect` | Rect | 预览区域矩形 |

**返回值**：`Texture`，渲染结果纹理

---

#### GetCharacterScreenRect

获取立绘在屏幕空间的包围盒。

```csharp
public Rect GetCharacterScreenRect(int index, Rect previewRect)
```

| 参数 | 类型 | 说明 |
|------|------|------|
| `index` | int | 立绘槽位索引 (0=左, 1=中, 2=右) |
| `previewRect` | Rect | 预览区域 |

**返回值**：`Rect`，屏幕坐标的包围盒

---

#### HitTestCharacterFixed

使用固定大小区域进行点击检测。

```csharp
public int HitTestCharacterFixed(Vector2 screenPos, Rect previewRect, float fixedSize = 60f)
```

| 参数 | 类型 | 说明 |
|------|------|------|
| `screenPos` | Vector2 | 屏幕坐标 |
| `previewRect` | Rect | 预览区域 |
| `fixedSize` | float | 固定检测区域大小 |

**返回值**：`int`，被点击的立绘索引，-1 表示未点击到

---

#### GetCharacterScreenCenter

获取立绘中心的屏幕坐标。

```csharp
public Vector2 GetCharacterScreenCenter(int index, Rect previewRect)
```

---

#### SelectCharacter / DeselectCharacter

选中/取消选中立绘。

```csharp
public void SelectCharacter(int index)
public void DeselectCharacter()
```

---

#### MoveSelectedCharacterToPosition

移动选中的立绘到指定位置。

```csharp
public void MoveSelectedCharacterToPosition(
    Vector2 mouseScreenPos,
    Rect previewRect,
    Vector2 dragOffset,
    bool constrainX = false,
    bool constrainY = false
)
```

| 参数 | 类型 | 说明 |
|------|------|------|
| `mouseScreenPos` | Vector2 | 鼠标屏幕位置 |
| `previewRect` | Rect | 预览区域 |
| `dragOffset` | Vector2 | 拖拽偏移量 |
| `constrainX` | bool | 是否约束X轴（true则不移动X） |
| `constrainY` | bool | 是否约束Y轴（true则不移动Y） |

---

#### ScaleSelectedCharacter

缩放选中的立绘。

```csharp
public void ScaleSelectedCharacter(float scaleDelta)
```

| 参数 | 类型 | 说明 |
|------|------|------|
| `scaleDelta` | float | 缩放增量（正值缩小，负值放大） |

---

#### GetSelectedCharacterTransform

获取选中立绘的当前变换数据。

```csharp
public (float offsetX, float offsetY, float scale) GetSelectedCharacterTransform()
```

**返回值**：元组，包含偏移X、偏移Y、缩放

---

#### AutoArrangeCharacters

一键排位立绘。

```csharp
public void AutoArrangeCharacters(float spacing)
```

| 参数 | 类型 | 说明 |
|------|------|------|
| `spacing` | float | 立绘间距 |

---

#### AddCharacterToSlot / AddCharacterToSlotWithOffset

添加立绘到指定槽位。

```csharp
public void AddCharacterToSlot(int slotIndex, string spritePath)
public void AddCharacterToSlotWithOffset(int slotIndex, string spritePath, float offsetX, float offsetY, float scale)
```

---

#### IsSlotOccupied

检查槽位是否有立绘。

```csharp
public bool IsSlotOccupied(int slotIndex)
```

---

#### GetCurrentSnapshot

获取当前快照。

```csharp
public StoryStateSnapshot GetCurrentSnapshot()
```

---

#### PanCamera / ZoomCamera / ResetView

摄像机控制。

```csharp
public void PanCamera(Vector2 mouseDelta)      // 平移摄像机
public void ZoomCamera(float scrollDelta)      // 缩放摄像机
public void ResetView()                        // 重置视图
```

---

#### Cleanup

清理资源。

```csharp
public void Cleanup()
```

释放 RenderTexture，销毁摄像机和 UI 实例。

---

## StoryStateSnapshot.cs

### 文件作用

剧情状态快照：记录某个对话节点对应的完整画面状态。

### 类定义

```csharp
public class StoryStateSnapshot
{
    /// <summary>
    /// 当前台词文本
    /// </summary>
    public string DialogText;

    /// <summary>
    /// 角色名称（说话人）
    /// </summary>
    public string CharacterName;

    /// <summary>
    /// 立绘列表（只处理左中右三个位置）
    /// </summary>
    public List<CharacterDisplayData> CharacterRoster = new List<CharacterDisplayData>();

    /// <summary>
    /// 当前背景音乐路径
    /// </summary>
    public string BgmPath;

    /// <summary>
    /// 当前背景图路径
    /// </summary>
    public string BackgroundPath;
}
```

### 字段说明

| 字段 | 类型 | 说明 |
|------|------|------|
| `DialogText` | string | 当前对话台词 |
| `CharacterName` | string | 说话人名称 |
| `CharacterRoster` | List\<CharacterDisplayData\> | 当前显示的立绘列表 |
| `BgmPath` | string | 背景音乐资源路径 |
| `BackgroundPath` | string | 背景图资源路径 |

---

## StoryStateTracer.cs

### 文件作用

剧情溯源引擎：正向遍历 xNode 图表，从起始节点遍历到目标节点，收集完整剧情状态。

### 类定义

```csharp
public static class StoryStateTracer
{
    private static List<DialogueNode> s_ClickHistory;    // 点击历史
    private static NodeGraph s_CurrentGraph;             // 当前节点图
    private static Dictionary<CharacterPosition, DialogueNode> s_LastActionNodeDict; // 最后操作节点记录
}
```

### 方法详解

#### CheckAndClearHistoryIfGraphChanged

检测是否切换了节点图，如果是则清空历史记录。

```csharp
public static void CheckAndClearHistoryIfGraphChanged(DialogueNode node)
```

| 参数 | 类型 | 说明 |
|------|------|------|
| `node` | DialogueNode | 当前节点 |

---

#### RecordClick

记录策划点击了哪个节点（用于分支选择）。

```csharp
public static void RecordClick(DialogueNode node)
```

---

#### GetSnapshot

**核心方法**：从起始节点正向遍历到目标节点，生成状态快照。

```csharp
public static StoryStateSnapshot GetSnapshot(DialogueNode targetNode)
```

| 参数 | 类型 | 说明 |
|------|------|------|
| `targetNode` | DialogueNode | 目标对话节点 |

**返回值**：`StoryStateSnapshot`，该节点的完整状态快照

**算法流程**：
1. 找到图表的起始节点（没有输入连接的节点）
2. 从起始节点开始正向遍历
3. 沿途收集背景音乐、背景图、立绘指令
4. 到达目标节点时，收集台词和角色名
5. 返回完整快照

---

#### FindStartNode

找到图表的起始节点。

```csharp
private static Node FindStartNode(DialogueNode anyNode)
```

**逻辑**：遍历图表中所有节点，找到 Entry 端口没有连接的 DialogueNode 或 ChoiceNode。

---

#### TraverseForward

从起始节点正向遍历到目标节点。

```csharp
private static void TraverseForward(Node startNode, DialogueNode targetNode, StoryStateSnapshot snapshot)
```

---

#### CollectNodeDataForward

收集单个节点的数据。

```csharp
private static void CollectNodeDataForward(DialogueNode node, StoryStateSnapshot snapshot, bool isTargetNode = false)
```

**收集逻辑**：
- **背景音乐**：覆盖式，最新的生效
- **背景图**：覆盖式，最新的生效
- **立绘**：指令式处理（Enter/Leave/ChangeSprite）
- **台词和角色名**：只有目标节点才收集

---

#### ProcessCharacterAction

处理单条立绘指令。

```csharp
private static void ProcessCharacterAction(
    DialogueNode currentNode,
    CharacterDisplayData display,
    StoryStateSnapshot snapshot
)
```

**指令处理**：

| 指令类型 | 条件 | 行为 |
|---------|------|------|
| `Enter` | 位置上不能有立绘 | 添加立绘到 CharacterRoster |
| `Leave` | 位置上必须有立绘 | 从 CharacterRoster 移除 |
| `ChangeSprite` | 位置上必须有立绘 | 替换立绘数据 |

**错误检测**：如果指令逻辑有误（如连续两次 Enter），会弹出对话框提示。

---

#### GetNextNode / GetNextFromDialogueNode / GetNextFromChoiceNode

获取下一个节点。

```csharp
private static Node GetNextNode(Node currentNode, HashSet<Node> visited)
private static Node GetNextFromDialogueNode(DialogueNode node, HashSet<Node> visited)
private static Node GetNextFromChoiceNode(ChoiceNode choiceNode, HashSet<Node> visited)
```

---

#### SelectBranchIndex

选择分支：根据历史记录决定走哪条分支。

```csharp
private static int SelectBranchIndex(ChoiceNode choiceNode, List<NodePort> ports, HashSet<Node> visited)
```

**分支选择逻辑**：
1. 从最近的点击历史开始查找
2. 检查历史节点在哪个分支路径上
3. 找到则选择该分支
4. 未找到则选择第一条分支（默认首选法则）

---

#### IsNodeInBranchPath

检查目标节点是否在从分支起点开始的路径上。

```csharp
private static bool IsNodeInBranchPath(Node branchStart, DialogueNode targetNode, HashSet<Node> globalVisited)
```

使用 BFS（广度优先搜索）遍历。

---

#### ClearHistory

清空点击历史。

```csharp
public static void ClearHistory()
```

---

## StoryPreviewWindow.cs

### 文件作用

剧情实时预览窗口：提供可视化的预览界面，支持立绘拖拽、编辑、记忆等功能。

### 打开方式

```
菜单栏 → Window → AVG → 剧情实时预览 (Story Preview)
```

### 类定义

```csharp
public class StoryPreviewWindow : EditorWindow
{
    private PreviewSandboxEngine m_SandboxEngine;           // 沙盒引擎
    private CharacterMemoryManager m_MemoryManager;         // 记忆管理器
    private DialogueNode m_SelectedNode;                    // 当前选中节点

    private bool m_IsDraggingSprite;                        // 拖拽状态
    private bool m_IsEditingCharacter;                      // 编辑状态
    private bool m_IsMovingCharacter;                       // 移动状态

    // 槽位颜色
    private static readonly Color k_SlotColorLeft;          // 红色
    private static readonly Color k_SlotColorCenter;        // 绿色
    private static readonly Color k_SlotColorRight;         // 蓝色
    private static readonly Color k_SelectionColor;         // 黄色（选中高亮）
}
```

### 生命周期方法

#### OnEnable

窗口启用时初始化。

```csharp
private void OnEnable()
{
    m_SandboxEngine = new PreviewSandboxEngine();
    Selection.selectionChanged += OnSelectionChanged;
    m_MemoryManager.Load();
}
```

---

#### OnDisable

窗口禁用时清理。

```csharp
private void OnDisable()
{
    // 如果正在编辑，先保存
    if (m_IsEditingCharacter) SaveCharacterTransform();

    Selection.selectionChanged -= OnSelectionChanged;
    m_SandboxEngine.Cleanup();
    m_MemoryManager.Save();
}
```

---

#### OnSelectionChanged

选择变化时刷新预览。

```csharp
private void OnSelectionChanged()
```

**行为**：
1. 如果正在编辑，先保存数据
2. 检查选中对象是否为 DialogueNode
3. 检测节点图切换
4. 记录点击历史
5. 生成并应用快照
6. 退出编辑状态

---

### 核心方法

#### HandleDragAndDrop

处理拖拽图片事件。

```csharp
private void HandleDragAndDrop(Rect previewRect)
```

**支持拖入类型**：
- `Sprite`
- `Texture2D`
- `DefaultAsset`（.png/.jpg 文件）

**事件类型**：
- `DragUpdated`：更新拖拽状态和悬停槽位
- `DragPerform`：添加立绘到槽位
- `DragExited`：重置拖拽状态

---

#### HandleCharacterInteraction

处理立绘交互事件。

```csharp
private void HandleCharacterInteraction(Rect previewRect)
```

**快捷键**：

| 按键 | 功能 |
|------|------|
| `Z` | 保存并退出编辑模式 |
| `Q` | 添加退出指令 |
| `滚轮` | 缩放立绘（编辑模式）或缩放视图 |
| `左键点击` | 选中立绘进入编辑模式 |
| `左键拖拽` | 移动立绘 |
| `Shift+拖拽` | 仅X轴移动 |
| `Ctrl+拖拽` | 仅Y轴移动 |
| `右键拖拽` | 平移视图 |
| `双击右键` | 重置视图 |

---

#### AddCharacterToSlot

添加立绘到指定槽位。

```csharp
private void AddCharacterToSlot(int slotIndex, string spritePath)
```

**流程**：
1. 检查槽位是否已有立绘
2. 尝试从记忆获取偏移和缩放
3. 添加立绘到预览引擎
4. 创建 CharacterDisplayData 并添加到节点
5. 记录到记忆管理器
6. 刷新预览

---

#### SaveCharacterTransform

保存立绘变换数据到节点和记忆。

```csharp
private void SaveCharacterTransform()
```

---

#### AddLeaveInstruction

按 Q 键添加退出指令。

```csharp
private void AddLeaveInstruction()
```

---

#### ChangeCharacterSprite

修改立绘图片。

```csharp
private void ChangeCharacterSprite(int slotIndex, string newSpritePath)
```

**优先级**：
1. 从上一节点同一槽位获取偏移和缩放
2. 从记忆获取

---

#### GetPreviousNode

获取当前节点的上一节点。

```csharp
private DialogueNode GetPreviousNode()
```

---

## 核心API参考

### Unity Editor API

#### EditorWindow

```csharp
// 获取窗口实例
var window = GetWindow<StoryPreviewWindow>("窗口标题");

// 重绘窗口
Repaint();

// 最小尺寸
window.minSize = new Vector2(500, 400);
```

#### Selection

```csharp
// 选择变化事件
Selection.selectionChanged += OnSelectionChanged;

// 获取选中对象
if (Selection.activeObject is DialogueNode node) { ... }

// 设置选中对象
Selection.activeObject = asset;
```

#### AssetDatabase

```csharp
// 加载资产
Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

// 获取资产路径
string path = AssetDatabase.GetAssetPath(obj);

// 查找资产
string[] guids = AssetDatabase.FindAssets("t:NodeGraph", new[] { folderPath });

// GUID 转路径
string assetPath = AssetDatabase.GUIDToAssetPath(guid);

// 标记资产为脏
EditorUtility.SetDirty(obj);

// 保存资产
AssetDatabase.SaveAssets();
AssetDatabase.Refresh();
```

#### DragAndDrop

```csharp
// 拖拽对象
Object[] objects = DragAndDrop.objectReferences;

// 设置视觉模式
DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

// 活动控件ID
int id = DragAndDrop.activeControlID;
```

#### EditorGUI / EditorGUILayout

```csharp
// 绘制矩形
EditorGUI.DrawRect(rect, color);

// 对象字段
GameObject obj = (GameObject)EditorGUILayout.ObjectField("标签", obj, typeof(GameObject), false);

// 帮助框
EditorGUILayout.HelpBox("消息", MessageType.Info);

// 按钮布局
if (GUILayout.Button("按钮", GUILayout.Width(80))) { ... }
```

#### EditorUtility

```csharp
// 显示对话框
EditorUtility.DisplayDialog("标题", "消息", "确定");

// 文件保存对话框
string path = EditorUtility.SaveFilePanel("标题", "目录", "文件名", "扩展名");

// 文件打开对话框
string path = EditorUtility.OpenFilePanel("标题", "目录", "扩展名");

// 设置脏
EditorUtility.SetDirty(obj);
```

#### EditorGUIUtility

```csharp
// 显示对象选择器
EditorGUIUtility.ShowObjectPicker<Texture2D>(null, false, "t:texture2d", 0);

// 获取选择器选中的对象
Object obj = EditorGUIUtility.GetObjectPickerObject();
```

### XNode API

#### Node / NodeGraph

```csharp
// 获取端口
NodePort port = node.GetPort("Exit");
NodePort inputPort = node.GetInputPort("Entry");
NodePort outputPort = node.GetOutputPort("Exit");

// 获取连接
NodePort connection = port.Connection;          // 单连接
List<NodePort> connections = port.GetConnections(); // 多连接

// 访问节点
NodeGraph graph = node.graph;
List<Node> nodes = graph.nodes;

// 节点名称
string name = node.name;
```

### Unity Runtime API

#### JsonUtility

```csharp
// 序列化
string json = JsonUtility.ToJson(data, true);

// 反序列化
MyData data = JsonUtility.FromJson<MyData>(json);
```

#### RenderTexture

```csharp
// 创建渲染纹理
RenderTexture rt = new RenderTexture(width, height, depth);
rt.antiAliasing = 4;

// 设置摄像机目标
camera.targetTexture = rt;

// 释放
rt.Release();
```

---

## 工作流程图

```
┌─────────────────────────────────────────────────────────────┐
│                    StoryPreviewWindow                        │
├─────────────────────────────────────────────────────────────┤
│  用户选择 DialogueNode                                       │
│         │                                                    │
│         ▼                                                    │
│  OnSelectionChanged()                                        │
│         │                                                    │
│         ├─→ StoryStateTracer.CheckAndClearHistoryIfGraphChanged()
│         ├─→ StoryStateTracer.RecordClick()
│         ├─→ StoryStateTracer.GetSnapshot() ──→ StoryStateSnapshot
│         │                                              │
│         ▼                                              │
│  PreviewSandboxEngine.ApplySnapshot() ◄───────────────┘
│         │
│         ▼
│  PreviewSandboxEngine.Render() → RenderTexture → GUI.DrawTexture
│
│  用户交互：
│  ├─ 拖拽图片 → AddCharacterToSlot() → CharacterMemoryManager.RecordMemory()
│  ├─ 编辑立绘 → SaveCharacterTransform() → CharacterMemoryManager.RecordMemory()
│  └─ 按 Z 保存退出 → SaveCharacterTransform()
└─────────────────────────────────────────────────────────────┘
```

---

## 记忆数据流向

```
┌──────────────────┐    RecordMemory()    ┌─────────────────────┐
│ StoryPreviewWindow│ ─────────────────→  │CharacterMemoryManager│
└──────────────────┘                      └─────────────────────┘
                                                   │
                                         Save()    │    Load()
                                                   ▼
                                          ┌───────────────┐
                                          │ CharacterMemory│
                                          │    .json      │
                                          └───────────────┘
```

---

## 注意事项

1. **内存管理**：`PreviewSandboxEngine` 创建的 GameObject 和 RenderTexture 需要在 `Cleanup()` 中正确释放

2. **单例模式**：`CharacterMemoryManager` 使用懒加载单例，首次访问 `Instance` 时创建

3. **编辑器持久化**：记忆数据保存在磁盘上，关闭 Unity 后依然存在

4. **节点图区分**：不同节点图的记忆是完全隔离的，使用 `GraphName` 作为区分键

5. **槽位限制**：只处理 `Left(0)`、`Center(1)`、`Right(2)` 三个槽位，忽略扩展位置

---

*文档版本：1.0*
*最后更新：2026-04-08*
