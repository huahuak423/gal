# UGF（Unity Game Framework）资源打包指南

> 本文档基于实际打包踩坑经验总结，适用于本项目（gal）的交接和维护。

---

## 一、核心概念

### 编辑器模式 vs 打包模式

| | 编辑器模式 | 打包模式（AssetBundle） |
|---|---|---|
| 触发条件 | `Application.isEditor && EditorResourceMode` | 打包后的 Build |
| 资源加载方式 | `AssetDatabase.LoadAssetAtPath()` 直接读 Assets 目录 | 从 `.dat`（AssetBundle）中加载 |
| 是否需要 InitResources | **不需要** | **必须调用** |
| 版本清单 | 不需要 | 必须存在 `GameFrameworkVersion.dat` |

### 关键文件说明

| 文件 | 作用 |
|------|------|
| `ResourceEditor.xml` | 编辑器资源配置（扫描目录、标签过滤），**仅编辑器使用**，不打包 |
| `ResourceCollection.xml` | 资源到 AssetBundle 的映射关系，**构建时使用** |
| `ResourceBuilder.xml` | Resource Builder 工具的配置（输出目录、平台、版本号等） |
| `GameFrameworkVersion.dat` | 运行时版本清单，**必须放在 StreamingAssets 根目录** |
| `*.dat`（Art/DataTable/Prefab/Scenes） | AssetBundle 资源包，运行时加载 |

---

## 二、资源打包流程

```
1. 资源标签标注 → 给需要打包的资源打上 ResourceInclusive 标签
2. Resource Editor → 配置资源到 Resource 分组，生成 ResourceCollection.xml
3. Resource Builder → 构建 AssetBundle，输出到 StreamingAssets 子目录
4. 手动复制 → 将 Package 输出的 .dat 文件复制到 StreamingAssets 根目录
5. Unity Build → 构建最终可执行文件
```

---

## 三、详细步骤

### 步骤 1：给资源打标签

在 Unity Inspector 底部的 Asset Labels 区域，给需要打包的资源添加标签：

```
ResourceInclusive
```

> **必须打此标签**，否则 Resource Editor 中看不到该资源。
> `ResourceEditor.xml` 中配置了 `SourceAssetUnionLabelFilter` 为 `l:ResourceInclusive`。

**需要打标签的资源类型：**
- UI 预制体（`Assets/GameMain/UI/Forms/*.prefab`）
- 数据表（`Assets/GameMain/DataTables/*.txt`）
- 立绘、背景图等美术资源
- 场景文件（`.unity`）

### 步骤 2：Resource Editor 配置

菜单：`Game Framework → Resource Tools → Resource Editor`

1. 左侧资源列表中勾选需要打包的资源
2. 分配到对应的 Resource 分组：
   - `Art` — 美术资源（立绘、背景图等）
   - `DataTable` — 数据表（.txt）
   - `Prefab` — UI 预制体
   - `Scenes` — 场景文件
3. 点击 Save 保存，生成 `ResourceCollection.xml`

**验证：** `Assets/GameFramework/Configs/ResourceCollection.xml` 中应包含所有资源，格式如：
```xml
<Asset Guid="xxxx" ResourceName="Prefab" />
```
> 不需要 `Name` 属性，Builder 会通过 Guid 自动解析资源路径。

### 步骤 3：Resource Builder 构建

菜单：`Game Framework → Resource Tools → Resource Builder`

**配置项：**

| 参数 | 说明 | 推荐值 |
|------|------|--------|
| Internal Resource Version | 内部版本号 | 每次打包递增（如 7、8、9...） |
| Platforms | 目标平台 | Windows64 = 2 |
| Compression | 压缩方式 | LZ4 = 1 |
| Output Directory | 输出目录 | `D:/unityProject/gal/Assets/StreamingAssets` |
| Output Package | 是否输出 Package | **勾选** |
| Force Rebuild | 强制重建 | 首次或修改资源后**勾选** |

点击 **Build** 开始构建。

**构建完成后的目录结构：**
```
Assets/StreamingAssets/
├── Full/1_0_7/Windows64/       ← 完整资源包
├── Package/1_0_7/Windows64/    ← ★ 运行时需要的包
├── Packed/1_0_7/Windows64/     ← 压缩包
└── Working/Windows/            ← 工作缓存
```

### 步骤 4：复制 Package 文件到根目录（关键！）

框架运行时从 `Application.streamingAssetsPath`（即 StreamingAssets 根目录）加载 `GameFrameworkVersion.dat`。
但 Resource Builder 输出到了子目录，**必须手动复制**。

**从 `Package/1_0_7/Windows64/` 复制以下文件到 `StreamingAssets/` 根目录：**

```
StreamingAssets/
├── GameFrameworkVersion.dat   ← 版本清单（必须）
├── Art.dat                    ← 美术资源包
├── DataTable.dat              ← 数据表包
├── Prefab.dat                 ← UI 预制体包
└── Scenes.dat                 ← 场景包
```

> **只复制 `.dat` 文件，不要复制 `.meta` 文件。**

### 步骤 5：ProcedurePreload 必须调用 InitResources

在打包模式下，`ProcedurePreload`（入口流程）必须调用 `ResourceComponent.InitResources()` 初始化资源管理器，
否则所有资源加载都会返回 `NotExist`。

```csharp
// ProcedurePreload.cs 关键代码
if (!GameEntry.Base.EditorResourceMode)
{
    GameEntry.Resource.InitResources(OnInitResourcesComplete);
}
```

> 编辑器模式下跳过此步骤（直接用 AssetDatabase 加载）。

### 步骤 6：Unity Build

菜单：`File → Build Settings`

1. 选择目标平台（Windows）
2. 确认场景列表中有 `SampleScene`
3. 点击 **Build**（不要用 Build And Run）

---

## 四、新增资源的打包流程

当项目中新增了资源（如新 UI 预制体、新数据表、新立绘）时：

1. **打标签**：给新资源添加 `ResourceInclusive` 标签
2. **Resource Editor**：打开后新资源会出现在列表中，分配到对应分组并 Save
3. **Resource Builder**：递增版本号，勾选 Force Rebuild，点击 Build
4. **复制文件**：将新的 Package 文件复制到 StreamingAssets 根目录
5. **Unity Build**：重新打包

---

## 五、代码中资源路径规则

代码中加载资源使用的路径必须与 AssetBundle 中的路径一致：

```csharp
// UI 预制体 — AssetUtility.cs
"Assets/GameMain/UI/Forms/MainMenu.prefab"

// 数据表 — StoryGraphLoader.cs
"Assets/GameMain/DataTables/Home.txt"

// 立绘 — DialoguePanel.cs 中的 SpritePath
"Assets/GameMain/Art/New Characters/女主/女主常规.png"
```

> 这些路径会在 Resource Builder 构建时自动写入 AssetBundle 清单。
> **不需要手动维护路径映射**，只要资源在 ResourceCollection.xml 中有 Guid 配置即可。

---

## 六、常见问题

### Q1：打包后黑屏，日志显示 "NotExist"

**原因：** `ProcedurePreload` 没有调用 `InitResources()`

**解决：** 确认 `ProcedurePreload.cs` 中在非编辑器模式下调用了 `GameEntry.Resource.InitResources()`

---

### Q2：日志显示 "GameFrameworkVersion.dat is invalid, 404 Not Found"

**原因：** `GameFrameworkVersion.dat` 不在 StreamingAssets 根目录

**解决：** 从 `Package/{版本号}/{平台}/` 目录复制所有 `.dat` 文件到 `StreamingAssets/` 根目录

---

### Q3：Resource Editor 中看不到新增资源

**原因：** 新资源没有 `ResourceInclusive` 标签

**解决：** 在 Inspector 底部 Asset Labels 区域给资源添加 `ResourceInclusive` 标签

---

### Q4：某个 UI 预制体加载失败

**排查步骤：**
1. 确认预制体文件存在于 `Assets/GameMain/UI/Forms/` 目录
2. 确认预制体在 `ResourceCollection.xml` 中有 Guid 配置
3. 确认 `AssetUtility.GetUIFormAsset()` 返回的路径与预制体实际路径一致
4. 确认已重新 Build 并复制了新的 .dat 文件

---

### Q5：编辑器中正常，打包后报错

**排查方法：**
1. 在 `GameEntry.cs` 中临时关闭编辑器模式：`m_EditorResourceMode = false`
2. 在编辑器中运行，模拟打包环境
3. 根据日志定位具体哪个资源加载失败

---

## 七、关键代码文件索引

| 文件 | 作用 |
|------|------|
| `Scripts/Base/GameEntry.cs` | 游戏入口，控制 EditorResourceMode 切换 |
| `Scripts/Procedure/ProcedurePreload.cs` | 入口流程，负责 InitResources |
| `Scripts/UI/Extension/AssetUtility.cs` | UI 预制体资源路径映射 |
| `Scripts/SaveAndLoad/StoryGraphLoader.cs` | 剧本数据表加载 |
| `Scripts/Procedure/ProcedureGame.cs` | 游戏主流程，事件表加载 |
| `Editor/AutoConfigResources.cs` | 从 BuildReport 自动生成 ResourceCollection |

---

## 八、文件清单（哪些需要/不需要打包）

| 文件/目录 | 是否需要打包 | 说明 |
|-----------|-------------|------|
| `GameMain/UI/Forms/*.prefab` | 是 | UI 预制体 |
| `GameMain/DataTables/*.txt` | 是 | 数据表 |
| `GameMain/Art/**` | 是 | 立绘、背景图等 |
| `GameMain/Scenes/*.unity` | 是 | 场景 |
| `GameMain/Configs/ResourceEditor.xml` | **否** | 仅编辑器工具使用 |
| `GameFramework/Configs/ResourceCollection.xml` | **否** | 构建时使用 |
| `GameFramework/Configs/ResourceBuilder.xml` | **否** | 构建时使用 |
| `StreamingAssets/Package/` | **否** | 构建中间产物 |
| `StreamingAssets/Full/` | **否** | 构建中间产物 |
| `StreamingAssets/Working/` | **否** | 构建中间产物 |
| `StreamingAssets/*.dat`（根目录） | **是** | 运行时实际加载的文件 |
