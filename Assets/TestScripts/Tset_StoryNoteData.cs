using System;
using System.Collections.Generic;
using UnityEngine;

namespace AVGGame.Runtime
{
    /// <summary>
    /// 这对应了 UGF 数据表 (DataTable) 里的【一行数据】
    /// 你的 UGF 解析脚本需要按列读取这些字段
    /// </summary>
    public class StoryRowData
    {
        public int Id;                  // 行号 (主键，例如 10001)
        public int NextId;              // 下一行剧情的 ID。如果是 0 代表结束，如果有选项则跳转由选项决定

        public string SpeakerName;      // 说话人
        public string DialogText;       // 台词

        // 将 xNode 中的 List<CharacterDisplayData> 转成 JSON 字符串存入表格
        // 游戏运行时读取这行字符串，用 JsonUtility 解析回 List 即可执行角色动作
        public string CharacterActionsJson; 
        
        // 分支选项数据，也可以存为 JSON 字符串
        // 例如: [{"Text":"去开门", "NextId":10005}, {"Text":"不开门", "NextId":10008}]
        public string ChoicesJson;

        // 其他多媒体配置 (通常可以沿用上文的设计)
        public string BgPath;
        public string BgmPath;
    }

    // --- 以下为辅助反序列化的类 (和编辑器里的类保持一致) ---
    [Serializable]
    public class RuntimeCharacterAction
    {
        public string CharacterName;
        public int ActionType; // 0:Enter, 1:Leave, 2:ChangeSprite
        public int Position;   // 0:Left, 1:Center, 2:Right
        public string SpritePath;
    }

    [Serializable]
    public class RuntimeChoice
    {
        public string ChoiceText;
        public int NextId;
    }
}