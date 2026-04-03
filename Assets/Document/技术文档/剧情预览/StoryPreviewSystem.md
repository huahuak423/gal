# AVG 剧情实时预览系统 - 技术文档

## 概述

本系统是一个 Unity Editor 扩展工具，允许策划人员在 xNode 图表中点击对话节点时，实时预览对应的 UI 效果。

## 为什么 Scene 里会有对话页面？

这是**正常行为**，不是 Bug。

### 原理说明

预览系统采用 **"虚拟场景 + 独立摄像机渲染"** 的方案：

```
┌─────────────────────────────────────────────────────────────┐
│                      Unity Editor                           │
│                                                             │
│  ┌──────────────────┐     ┌──────────────────────────────┐ │
│  │   xNode 图表      │     │    StoryPreviewWindow        │ │
│  │                  │     │    (预览窗口)                 │ │
│  │  [DialogueNode] ─┼────►│                              │ │
│  │                  │     │   ┌────────────────────┐     │ │
│  └──────────────────┘     │   │  RenderTexture     │     │ │
│                           │   │  (显示预览画面)     │     │ │
│                           │   └────────────────────┘     │ │
│                           └──────────────────────────────┘ │
│                                                             │
│  ┌──────────────────────────────────────────────────────┐  │
│  │                    隐藏的虚拟场景                      │  │
│  │                                                      │  │
│  │   [PreviewCamera] ──渲染──> RenderTexture           │  │
│  │         │                                            │  │
│  │         ▼                                            │  │
│  │   [Canvas (WorldSpace)]  <-- 这就是你在 Scene 里看到的 │  │
│  │      └── Dialogue UI Prefab                         │  │
│  │                                                      │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

### 技术细节

1. **实例化 UI 预制体**
   - 当你拖入 UI 预制体时，系统会 `Instantiate` 一个副本
   - 这个副本被放置在场景中（但标记为 `HideAndDontSave`）

2. **为什么你在 Scene 里能看到？**
   - `HideAndDontSave` 只是让对象不出现在 Hierarchy 且不保存到场景
   - 但 Scene 视图的摄像机仍然能看到这些对象
   - 这是 Unity 的设计，无法完全隐藏

3. **为什么不直接销毁？**
   - 需要保留实例来修改 UI 组件（文本、立绘等）
   - 每次预览都重新实例化会非常慢且产生 GC

## 系统架构

### 四个核心脚本

| 脚本 | 职责 |
|------|------|
| `StoryPreviewWindow.cs` | Editor 窗口 UI，处理用户交互和鼠标事件 |
| `PreviewSandboxEngine.cs` | 渲染引擎，管理虚拟场景和摄像机 |
| `StoryStateTracer.cs` | 逆向遍历图表，收集完整状态快照 |
| `StoryStateSnapshot.cs` | 数据结构，存储某一时刻的剧情状态 |

### 工作流程

```
1. 策划点击 DialogueNode
         │
         ▼
2. StoryPreviewWindow.OnSelectionChanged() 触发
         │
         ▼
3. StoryStateTracer.RecordClick() 记录点击历史
         │
         ▼
4. StoryStateTracer.GetSnapshot() 逆向遍历收集数据
         │
         ├── 收集台词、角色名
         ├── 收集立绘（处理分支选择）
         └── 返回 StoryStateSnapshot
         │
         ▼
5. PreviewSandboxEngine.ApplySnapshot() 应用到 UI
         │
         ├── 设置对话文本
         ├── 设置角色名
         └── 设置立绘图片
         │
         ▼
6. PreviewSandboxEngine.Render() 渲染一帧
         │
         ├── Camera.Render() 渲染到 RenderTexture
         └── 返回 Texture 给窗口显示
         │
         ▼
7. 窗口 Repaint() 显示最新画面
```

## 关键技术点

### 1. Canvas 渲染模式转换

UI 预制体原本是 `Screen Space - Overlay` 模式，预览时转换为 `World Space`：

```csharp
m_Canvas.renderMode = RenderMode.WorldSpace;
canvasRect.localScale = Vector3.one;  // 关键：重置 scale
canvasRect.localPosition = Vector3.zero;
canvasRect.sizeDelta = new Vector2(1920, 1080);
```

> **注意**：Overlay 模式下 Canvas 的 scale 是 (0,0,0)，必须重置为 (1,1,1) 才能正确渲染。

### 2. 摄像机控制

```csharp
// 正交摄像机，半高 = 540 (1080 / 2)
m_RenderCamera.orthographic = true;
m_RenderCamera.orthographicSize = 540f / zoom;

// 平移：移动摄像机位置
m_RenderCamera.transform.position = new Vector3(panX, panY, -10);

// 缩放：调整正交尺寸
m_RenderCamera.orthographicSize = 540f / zoom;
```

### 3. 分支选择算法

当逆向遍历遇到多分支汇合点时：

```csharp
// 优先级：点击历史 > 默认首选
foreach (var conn in connections)
{
    if (s_ClickHistory.Contains(prevNode))
        return prevNode;  // 找到历史记录
}
return connections[0];  // 默认选第一条
```

### 4. 立绘位置映射

只处理左中右三个基础位置：

```csharp
CharacterPosition enum: Left(0), Center(1), Right(2), EX1-EX4(3-6)

// 过滤扩展位置
int posIndex = (int)display.Position;
if (posIndex > 2) continue;  // 忽略 EX1-EX4

// 映射到 UI Image 索引
m_CharacterImages[posIndex].sprite = charSprite;
```

## 操作说明

| 操作 | 功能 |
|------|------|
| 滚轮 | 缩放预览画面 |
| 右键拖拽 | 平移画面 |
| 中键拖拽 | 平移画面 |
| 双击右键 | 重置视图 |
| 点击节点 | 更新预览内容 |

## 文件结构

```
Assets/GameMain/Scripts/Editor/StoryPreviewTool/
├── StoryPreviewWindow.cs      # 窗口入口
├── PreviewSandboxEngine.cs    # 渲染引擎
├── StoryStateTracer.cs        # 状态追踪
└── StoryStateSnapshot.cs      # 快照数据
```

## 常见问题

### Q: Scene 里看到多余的 UI 怎么办？

A: 这是正常的，关闭预览窗口后会自动清理。如果残留，可以手动在 Scene 视图里删除。

### Q: 预览画面是空白的？

A: 检查：
1. UI 预制体是否正确拖入
2. Canvas 下是否有子物体
3. 查看控制台是否有错误信息

### Q: 立绘不显示？

A: 检查：
1. DialogueNode 的 CharacterDisplays 是否有数据
2. SpritePath 是否是有效的资源路径
3. Position 是否在 Left/Center/Right 范围内
