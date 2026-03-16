# GameMain 模块架构文档

本文档描述 GameMain 模块的架构设计和各文件的功能说明。

---

## 目录结构概览

```
Assets/GameMain/
├── Scripts/                    # 脚本文件
│   ├── Base/                   # 基础入口
│   ├── Procedure/              # 流程管理
│   ├── UI/                     # UI系统
│   ├── AVGSystem/              # AVG游戏系统
│   └── Editor/                 # 编辑器扩展
├── UI/                         # UI资源
│   └── Forms/                  # UI预制体
├── Scenes/                     # 场景文件
└── Document/                   # 文档
```

---

## 一、Base 文件夹 - 基础入口

### 1.1 GameEntry.cs

**功能：** 游戏启动入口，挂载到场景中的 GameObject 上

**职责：**
- 作为游戏启动的第一个脚本
- 在 `Awake()` 中初始化单例
- 在 `Start()` 中调用 `BuiltinEntry.Initialize()` 初始化所有框架组件
- 提供静态访问点访问框架组件（如 `GameEntry.UI`、`GameEntry.Procedure` 等）

**使用方式：**
1. 将 `GameEntry` 脚本挂载到场景中的 GameObject 上
2. GameFramework prefab 作为其子物体
3. 游戏启动时自动初始化

**关键代码：**
```csharp
// 在 Start() 中初始化组件
private void Start()
{
    InitializeComponents();
}

// 提供快捷访问
public static UIComponent UI => BuiltinEntry.UI;
public static ProcedureComponent Procedure => BuiltinEntry.Procedure;
```

### 1.2 BuiltinEntry.cs

**功能：** 框架内置组件管理器，静态类

**职责：**
- 从框架的 `GameEntry` 静态类获取所有 GameFramework 组件
- 缓存组件引用，提供快速访问
- 管理 framework 的初始化和关闭状态

**获取的组件包括：**
- **核心组件：** Base、Event、Fsm、ObjectPool、ReferencePool
- **数据组件：** DataNode、DataTable、Config
- **资源组件：** Resource
- **游戏对象组件：** Entity、UI、Sound、Scene
- **功能组件：** Localization、Setting、Download、Network、FileSystem、WebRequest、Debugger
- **流程组件：** Procedure

### 1.3 CustomEntry.cs

**功能：** 自定义组件管理器，静态类

**职责：**
- 管理项目特有的自定义组件（非框架组件）
- 提供自定义组件的添加、获取、移除功能

**使用方式：**
```csharp
// 添加自定义组件
CustomEntry.AddCustomComponent<MyManager>();

// 获取自定义组件
var manager = CustomEntry.GetCustomComponent<MyManager>();
```

---

## 二、Procedure 文件夹 - 流程管理

流程（Procedure）是 GameFramework 的核心概念，使用状态机管理游戏的不同阶段。

### 2.1 流程启动顺序

```
游戏启动
    ↓
ProcedurePreload (预加载) ← 【流程入口】
    ↓
ProcedureSplash (启动画面)
    ↓
ProcedureMainMenu (主菜单)
    ↓
ProcedureGame (游戏主流程)
```

### 2.2 ProcedurePreload.cs - 预加载流程

**功能：** 游戏启动后的第一个流程，是所有流程的入口

**职责：**
- 初始化 UI 环境（创建 MainCanvas、EventSystem）
- 执行资源预加载、数据表加载
- 创建必要的游戏基础设施

**关键代码：**
```csharp
protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
{
    base.OnEnter(procedureOwner);

    // 初始化 UI 环境
    InitializeUIEnvironment();

    // TODO: 在这里进行资源预加载、数据表加载等
    m_PreloadComplete = true;
}

protected override void OnUpdate(...)
{
    if (m_PreloadComplete)
    {
        // 预加载完成，切换到启动画面流程
        ChangeState<ProcedureSplash>(procedureOwner);
    }
}
```

**UI 环境初始化内容：**
- 创建 MainCanvas（分辨率 1920x1080，Screen Space Overlay 模式）
- 添加 CanvasScaler（ScaleWithScreenSize）
- 添加 GraphicRaycaster
- 创建 EventSystem（如果不存在）

### 2.3 ProcedureSplash.cs - 启动画面流程

**功能：** 显示游戏 Logo、公司标志、加载提示等

**职责：**
- 显示启动画面 UI
- 控制最少显示时间（避免一闪而过）
- 完成后切换到主菜单

**配置项：**
```csharp
private float m_MinSplashTime = 0.5f; // 最少显示时间（秒）
```

### 2.4 ProcedureMainMenu.cs - 主菜单流程

**功能：** 显示游戏主菜单界面

**职责：**
- 打开主菜单 UI (MainMenuPanel)
- 处理"新游戏"、"继续游戏"、"设置"、"退出"等操作
- 根据用户选择切换到对应流程

**公共方法：**
| 方法 | 说明 |
|------|------|
| `StartNewGame()` | 开始新游戏，切换到 ProcedureGame |
| `ContinueGame()` | 继续游戏（需有存档） |
| `QuitGame()` | 退出游戏 |

**关键代码：**
```csharp
private void OpenMainMenuUI()
{
    string uiFormAssetName = AssetUtility.GetUIFormAsset(UIFormId.MainMenu);
    GameEntry.UI.OpenUIForm(uiFormAssetName, UIGroupDefinition.Main, Constant.AssetPriority.UIAsset);
}
```

### 2.5 ProcedureGame.cs - 游戏主流程

**功能：** AVG 游戏的核心流程，处理游戏内所有逻辑

**职责：**
- 初始化游戏数据
- 加载游戏场景
- 管理游戏暂停/恢复
- 处理对话系统、剧情系统

**公共方法：**
| 方法 | 说明 |
|------|------|
| `PauseGame()` | 暂停游戏 |
| `ResumeGame()` | 恢复游戏 |
| `ReturnToMainMenu()` | 返回主菜单 |

---

## 三、AVGSystem 文件夹 - AVG 游戏系统

AVGSystem 是本项目的核心系统，包含剧情数据定义和 xNode 可视化编辑节点。

### 3.1 目录结构

```
AVGSystem/
├── Runtime/                    # 运行时数据定义
│   ├── StoryDataDefine.cs      # 枚举和数据结构定义
│   └── StoryRowData.cs         # 数据表行定义（UGF兼容）
└── xNodeGraph/                 # xNode 可视化节点
    ├── StoryGraph.cs           # 故事图容器
    ├── DialogueNode.cs         # 对话节点
    ├── ChoiceNode.cs           # 分支选项节点
    ├── RewardNode.cs           # 奖励结算节点
    └── SubGraphNode.cs         # 子图跳转节点
```

### 3.2 Runtime 子文件夹

#### 3.2.1 StoryDataDefine.cs - 数据定义

**功能：** 定义 AVG 系统使用的所有枚举和数据结构

**枚举定义：**

| 枚举名 | 说明 | 可选值 |
|--------|------|--------|
| `PlayerAttributeType` | 玩家属性类型 | None, Charm(魅力), Inspiration(灵感), Sanity(理智) |
| `ConditionOperator` | 条件运算符 | GreaterThanOrEqual, LessThanOrEqual, Equal |
| `ConditionType` | 条件类型 | PlayerAttribute, NpcFavorability, SpecialItem |
| `CharacterActionType` | 角色动作 | Enter(入场), Leave(离场), ChangeSprite(换立绘) |
| `CharacterPosition` | 角色位置 | Left, Center, Right |

**数据结构：**

```csharp
// 选择条件 - 用于判断选项是否可用
public class ChoiceCondition
{
    public ConditionType Type;              // 条件类型
    public PlayerAttributeType AttributeType; // 属性类型
    public string NpcId;                    // NPC ID
    public ConditionOperator Operator;      // 比较运算符
    public int Value;                       // 数值
    public string ItemId;                   // 物品ID
    public bool RequireItem;                // 是否需要拥有物品
}

// 选择奖励 - 选择后的奖励内容
public class ChoiceReward
{
    public ConditionType Type;              // 奖励类型
    public PlayerAttributeType AttributeType;
    public string NpcId;
    public int Value;
    public string ItemId;
}
```

#### 3.2.2 StoryRowData.cs - 数据表行定义

**功能：** 定义 UGF 数据表的行结构，继承自 `DataRowBase`

**职责：**
- 定义数据表的列结构
- 实现 txt 文件的解析逻辑
- 支持 UGF 的 DataTable 模块读取

**数据列定义：**

| 列名 | 类型 | 说明 |
|------|------|------|
| Id | int | 节点唯一ID |
| NextId | int | 下一个节点ID（0表示结束） |
| NodeType | int | 节点类型（0对话/1选项/2跳转/3奖励） |
| SpeakerName | string | 说话人名称 |
| DialogText | string | 对话文本内容 |
| CharacterActionsJson | string | 角色立绘动作（JSON格式） |
| ChoicesJson | string | 选项列表（JSON格式） |
| TargetGraphName | string | 目标子图名称 |
| RewardsJson | string | 奖励列表（JSON格式） |
| BgmPath | string | 背景音乐资源路径 |
| SePath | string | 音效资源路径 |

**节点类型枚举：**
```csharp
public enum StoryNodeType
{
    Dialogue = 0,       // 对话节点
    Choice = 1,         // 选项节点
    ChangeGraph = 2,    // 子图跳转
    Reward = 3          // 奖励节点
}
```

### 3.3 xNodeGraph 子文件夹

#### 3.3.1 StoryGraph.cs - 故事图容器

**功能：** xNode 的 Graph 容器类，用于创建剧本图资源

**创建方式：**
```
右键 Project 窗口 → Create → AVG → 剧本图 (Story Graph)
```

#### 3.3.2 DialogueNode.cs - 对话节点

**功能：** 显示对话文本的基础节点

**端口：**
- `Entry` (Input) - 入口连接
- `Exit` (Output) - 出口连接

**字段说明：**

| 字段 | 类型 | 说明 |
|------|------|------|
| SpeakerName | string | 说话人名称 |
| DialogText | string | 对话文本（支持多行） |
| CharacterDisplays | List | 角色立绘显示配置 |
| BgmPath | string | 背景音乐路径（留空则不切换） |
| SePath | string | 临时音效路径 |
| EditorNote | string | 编辑器备注（不影响运行） |

#### 3.3.3 ChoiceNode.cs - 分支选项节点

**功能：** 创建分支选择，支持条件判断

**端口：**
- `Entry` (Input) - 入口连接
- `Choices` (Output, 动态端口) - 每个选项一个出口

**节点颜色：** 蓝色 (#5271FF)

**选项数据结构：**
```csharp
public class ChoiceItemData
{
    public string ChoiceText;                       // 选项文本
    public List<ChoiceCondition> Conditions;        // 触发条件
    public List<ChoiceReward> Rewards;              // 选择奖励
}
```

#### 3.3.4 RewardNode.cs - 奖励结算节点

**功能：** 独立的奖励发放节点，用于章节结算、事件奖励

**端口：**
- `Entry` (Input) - 入口连接
- `Exit` (Output) - 出口连接

**节点颜色：** 金色 (#FFB300)

**字段说明：**

| 字段 | 类型 | 说明 |
|------|------|------|
| RewardTitle | string | 结算标题（如"第一章完成"） |
| Rewards | List | 奖励列表 |

#### 3.3.5 SubGraphNode.cs - 子图跳转节点

**功能：** 跳转到另一个 StoryGraph，实现模块化剧情设计

**端口：**
- `Entry` (Input) - 入口连接
- `Exit` (Output) - 出口连接（用于子图结束后返回）

**节点颜色：** 绿色 (#4CAF50)

**字段说明：**

| 字段 | 类型 | 说明 |
|------|------|------|
| TargetGraphAsset | NodeGraph | 目标图资产（直接拖拽） |
| TargetGraphNameFallback | string | 备用图名称（手动填写） |

---

## 四、UI 文件夹 - UI 系统

UI 系统基于 UGF 的 UIComponent，采用分组管理和基类继承的设计模式。

### 4.1 目录结构

```
UI/
├── Base/                       # UI 基础框架
│   ├── UIFormBase.cs           # UI 界面基类
│   ├── UIFormId.cs             # UI ID 定义
│   └── UIGroupDefinition.cs    # UI 分组定义
├── Extension/                  # UI 扩展工具
│   ├── AssetUtility.cs         # 资源路径工具
│   ├── Constant.cs             # 常量定义
│   ├── Toast.cs                # 消息提示组件
│   ├── UIExtension.cs          # 扩展方法
│   └── UIHelper.cs             # UI 辅助类
└── Forms/                      # 具体 UI 界面
    ├── DialoguePanel.cs        # 对话面板
    ├── LoadingPanel.cs         # 加载面板
    └── MainMenuPanel.cs        # 主菜单面板
```

### 4.2 Base 子文件夹

#### 4.2.1 UIFormBase.cs - UI 基类

**功能：** 所有 UI 界面的基类，继承自 `UIFormLogic`

**提供的属性：**
| 属性 | 类型 | 说明 |
|------|------|------|
| CachedCanvas | Canvas | 缓存的 Canvas 组件 |
| CanvasGroup | CanvasGroup | 用于控制透明度和交互 |
| Rect | RectTransform | UI 的 RectTransform |
| SortingOrder | int | UI 层级（可重写） |

**生命周期方法：**
| 方法 | 调用时机 |
|------|----------|
| `OnInit()` | UI 初始化时 |
| `OnOpen()` | UI 打开时 |
| `OnClose()` | UI 关闭时 |
| `OnPause()` | UI 暂停时（被其他 UI 覆盖） |
| `OnResume()` | UI 恢复时 |

**提供的辅助方法：**
```csharp
// 获取子物体组件
protected T GetChild<T>(string path) where T : Component

// 关闭当前 UI
protected virtual void CloseSelf()
```

#### 4.2.2 UIFormId.cs - UI ID 定义

**功能：** 定义所有 UI 界面的唯一标识 ID

**ID 分配规则：**

| ID 范围 | 用途 | 示例 |
|---------|------|------|
| 1000-1999 | 系统 UI | Loading(1000), MessageBox(1001) |
| 2000-2999 | 主菜单相关 | MainMenu(2000), Settings(2001) |
| 3000-3999 | 游戏内 UI | Dialogue(3000), Choice(3001) |

#### 4.2.3 UIGroupDefinition.cs - UI 分组定义

**功能：** 定义 UI 的层级分组，控制渲染顺序

**分组定义（层级从低到高）：**

| 常量 | 名称 | 用途 |
|------|------|------|
| Background | 底层 | 背景图片、场景背景 |
| Scene | 场景层 | 对话框、剧情元素 |
| Main | 主界面 | 菜单、HUD、功能面板 |
| Popup | 弹窗层 | 提示框、确认框、选择框 |
| Top | 顶层 | Loading、Toast、全屏遮罩 |

### 4.3 Extension 子文件夹

#### 4.3.1 AssetUtility.cs - 资源路径工具

**功能：** 根据 UI ID 获取对应的资源路径

**资源路径格式：**
```
Assets/GameMain/UI/Forms/{UIName}.prefab
```

**使用示例：**
```csharp
string path = AssetUtility.GetUIFormAsset(UIFormId.MainMenu);
// 返回: "Assets/GameMain/UI/Forms/MainMenu.prefab"

string name = AssetUtility.GetUIFormName(UIFormId.Dialogue);
// 返回: "DialoguePanel"
```

#### 4.3.2 Constant.cs - 常量定义

**功能：** 定义资源加载优先级

**优先级定义：**
| 常量 | 值 | 说明 |
|------|-----|------|
| UIAsset | 100 | UI 资源（最高） |
| DataTableAsset | 60 | 数据表 |
| ConfigAsset | 50 | 配置文件 |
| AudioAsset | 30 | 音频资源 |
| SceneAsset | 0 | 场景资源（最低） |

#### 4.3.3 Toast.cs - 消息提示组件

**功能：** 显示简短的消息提示，支持淡入淡出效果

**初始化（游戏启动时调用）：**
```csharp
Toast.Initialize(canvasTransform);
```

**使用方法：**
```csharp
// 普通消息
Toast.Show("保存成功", 2f);

// 成功提示（绿色）
Toast.ShowSuccess("操作成功");

// 错误提示（红色）
Toast.ShowError("加载失败", 3f);

// 警告提示（黄色）
Toast.ShowWarning("请注意");
```

#### 4.3.4 UIExtension.cs - 扩展方法

**功能：** 为 UIComponent 提供便捷的扩展方法

**扩展方法列表：**
| 方法 | 说明 |
|------|------|
| `OpenUIFormById(int id)` | 通过 ID 打开 UI |
| `OpenUIFormById(int id, string group)` | 通过 ID 打开 UI 并指定分组 |
| `CloseUIFormsByGroup(string group)` | 关闭指定分组的所有 UI |
| `IsUIFormOpen(int id)` | 检查 UI 是否已打开 |

#### 4.3.5 UIHelper.cs - UI 辅助类

**功能：** 提供更高层级的 UI 操作封装

**快捷方法：**
```csharp
// 打开各种 UI
UIHelper.OpenMainMenu();
UIHelper.OpenSettings();
UIHelper.OpenDialogue();
UIHelper.OpenLoading();

// 关闭 UI
UIHelper.CloseAllUI();
UIHelper.CloseUIGroup(UIGroupDefinition.Popup);
```

### 4.4 Forms 子文件夹

#### 4.4.1 DialoguePanel.cs - 对话面板

**功能：** AVG 游戏的核心 UI，显示角色对话

**序列化字段：**
| 字段 | 类型 | 说明 |
|------|------|------|
| m_CharacterPortrait | Image | 角色头像/立绘 |
| m_CharacterNameText | TMP_Text | 角色名称文本 |
| m_DialogueText | TMP_Text | 对话内容文本 |
| m_ContinueIndicator | GameObject | 继续提示（闪烁箭头等） |
| m_BackgroundImage | Image | 背景图片 |

**公共方法：**
| 方法 | 说明 |
|------|------|
| `SetDialogue(name, text, portrait)` | 设置对话内容 |
| `SetBackground(sprite)` | 设置背景图片 |
| `SetCompleteCallback(action)` | 设置对话完成回调 |
| `SkipTypewriter()` | 跳过打字机效果 |

**特性：**
- 打字机效果（逐字显示）
- 可跳过打字机效果
- 自动显示/隐藏继续提示

#### 4.4.2 LoadingPanel.cs - 加载面板

**功能：** 显示加载进度

**序列化字段：**
| 字段 | 类型 | 说明 |
|------|------|------|
| m_ProgressSlider | Slider | 进度条滑块 |
| m_ProgressFillImage | Image | 进度条填充 |
| m_ProgressText | TMP_Text | 进度百分比文本 |
| m_TipText | TMP_Text | 提示文本 |
| m_LoadingIcon | Image | 加载图标（旋转） |

**公共方法：**
| 方法 | 说明 |
|------|------|
| `SetProgress(float progress)` | 设置加载进度（0-1） |
| `SetTip(string tip)` | 设置提示文本 |
| `ForceComplete()` | 立即完成到 100% |

**特性：**
- 平滑进度动画
- 自动旋转加载图标

#### 4.4.3 MainMenuPanel.cs - 主菜单面板

**功能：** 游戏主菜单界面

**按钮字段：**
| 字段 | 说明 |
|------|------|
| m_ButtonNewGame | 新游戏按钮 |
| m_ButtonContine | 继续游戏按钮 |
| m_ButtonCgShows | CG 画廊按钮 |
| m_ButtonSetting | 设置按钮 |
| m_ButtonExit | 退出按钮 |

**功能：**
- 检测存档存在性，控制"继续"按钮可用状态
- 按钮点击事件处理

---

## 五、Editor 文件夹 - 编辑器扩展

编辑器工具用于提高开发效率，位于菜单栏 `AVG Tools` 下。

### 5.1 工具列表

| 菜单项 | 快捷键 | 功能 |
|--------|--------|------|
| 1. 从 CSV 导入剧本到 xNode | - | 导入策划配置表 |
| 2. 导出 xNode 为 UGF 数据表 | - | 导出运行时数据 |
| 3. 节点快速导航器 | - | 搜索和定位节点 |

### 5.2 StoryGraphImporter.cs - CSV 导入工具

**菜单位置：** `AVG Tools → 1. 从 CSV 导入剧本到 xNode`

**功能：** 将策划配置的 CSV 文件导入为 xNode 剧本图

**使用步骤：**
1. 在 Project 窗口选中一个 StoryGraph 文件
2. 点击菜单项
3. 选择 CSV 文件
4. 自动生成节点并连接

**CSV 格式要求：**
| 列号 | 内容 | 说明 |
|------|------|------|
| 第1列 | 角色编号 | 用于映射角色名 |
| 第2列 | 保留 | - |
| 第3列 | 对话文本 | 主要内容 |
| 第4列 | 特殊标记 | 可选，用于立绘/演出 |

**角色编号映射表：**
| 编号 | 角色名 |
|------|--------|
| 00 | 旁白（空字符串） |
| 01 | 女主名 |
| 02 | 许映月 |
| 03 | 周杉 |
| 04 | 陈予宁 |
| 05 | 陈予荣 |
| 06 | 温叙 |
| 07 | 何行舟 |
| 08 | 群众 |

### 5.3 StoryGraphExporter.cs - 数据表导出工具

**菜单位置：** `AVG Tools → 2. 导出 xNode 为 UGF 数据表`

**功能：** 将 xNode 剧本图导出为 UGF 可读取的 txt 数据表

**使用步骤：**
1. 在 Project 窗口选中要导出的 StoryGraph
2. 点击菜单项
3. 选择保存位置（建议保存到 `Assets/GameMain/DataTables/`）
4. 导出完成

**导出格式：**
- 文件格式：TXT（制表符分隔）
- 编码：UTF-8
- 起始 ID：10000

**数据表结构：**
```
Id    NextId    NodeType    SpeakerName    DialogText    ...
int   int       int         string         string        ...
编号  下一句ID  类型        说话人         台词          ...
```

### 5.4 GraphNodeSearchWindow.cs - 节点导航器

**菜单位置：** `AVG Tools → 3. 节点快速导航器`

**功能：** 在复杂的剧本图中快速搜索和定位节点

**搜索类型：**
- All - 全部节点
- Dialogue - 对话节点
- Choice - 选项节点
- SubGraph - 子图节点

**使用方法：**
1. 先打开一个 xNode 编辑器窗口
2. 打开节点导航器窗口
3. 输入搜索关键词
4. 选择过滤类型
5. 点击结果项或"跳转"按钮定位到节点

### 5.5 DialogueNodeEditor.cs - 节点编辑器

**功能：** 自定义对话节点在编辑器中的显示外观

**特性：**
- 当 `NeedsAttention = true` 时，节点显示为橙色
- 用于标记需要特殊处理的节点

---

## 六、资源文件夹说明

| 文件夹 | 内容 | 说明 |
|--------|------|------|
| Configs/ | 配置文件 | 游戏配置，如难度、参数等 |
| DataTables/ | 数据表 | UGF 格式的 txt 数据表 |
| Entities/ | 实体资源 | 角色、物品等预制体 |
| Fonts/ | 字体 | 游戏使用的字体文件 |
| Libraries/ | 库文件 | 第三方库资源 |
| Localization/ | 本地化 | 多语言资源 |
| Materials/ | 材质 | Shader 材质文件 |
| Meshes/ | 模型 | 3D 模型网格 |
| Music/ | 背景音乐 | BGM 音频文件 |
| Scenes/ | 场景 | Unity 场景文件 |
| Sounds/ | 音效 | SE 音效文件 |
| StoryGraphs/ | 剧本图 | xNode 的 StoryGraph 资源 |
| Textures/ | 纹理 | 图片纹理资源 |
| UI/Forms/ | UI 预制体 | UI 界面预制体 |
| UI/Textures/ | UI 图片 | UI 专用图片资源 |

---

## 七、快速上手

### 7.1 创建新场景

1. 创建新场景
2. 创建空物体，命名为 `GameEntry`
3. 挂载 `GameEntry.cs` 脚本
4. 确保 GameFramework 的各个 Component 已正确配置

### 7.2 创建剧情

1. 右键 Project → Create → AVG → 剧本图 (Story Graph)
2. 双击打开 xNode 编辑器
3. 右键添加节点类型
4. 连接节点端口
5. 填写节点内容

### 7.3 导出运行

1. 选中 StoryGraph
2. AVG Tools → 2. 导出 xNode 为 UGF 数据表
3. 保存到 DataTables 文件夹
4. 运行游戏测试

### 7.4 添加新 UI

1. 在 `UIFormId.cs` 添加 ID 定义
2. 在 `AssetUtility.cs` 添加路径映射
3. 创建 UI 预制体
4. 创建脚本继承 `UIFormBase`
5. 绑定 UI 元素

---

## 八、扩展指南

### 8.1 添加新的流程

```csharp
// 1. 创建新类，继承 ProcedureBase
public class ProcedureNewFeature : ProcedureBase
{
    protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
    {
        base.OnEnter(procedureOwner);
        // 初始化逻辑
    }

    protected override void OnUpdate(...)
    {
        base.OnUpdate(...);
        // 更新逻辑，检测状态切换
    }
}

// 2. 在现有流程中切换到新流程
ChangeState<ProcedureNewFeature>(procedureOwner);
```

### 8.2 添加新的节点类型

```csharp
// 1. 创建新类，继承 Node
[CreateNodeMenu("AVG/自定义节点")]
[NodeWidth(300)]
public class CustomNode : Node
{
    [Input] public int Entry;
    [Output] public int Exit;

    // 自定义字段...

    public override object GetValue(NodePort port) => null;
}

// 2. 在 StoryGraphExporter 中添加导出逻辑
```

### 8.3 添加自定义组件

```csharp
// 1. 创建组件类
public class StoryManager : MonoBehaviour
{
    // 管理剧情逻辑...
}

// 2. 在 CustomEntry.Initialize() 中注册
AddCustomComponent<StoryManager>();

// 3. 使用组件
var storyManager = CustomEntry.GetCustomComponent<StoryManager>();
```

---

*文档版本: 1.0*
*最后更新: 2024*