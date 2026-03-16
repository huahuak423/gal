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

## 二、Procedure 文件夹 - 流程