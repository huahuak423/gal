using System;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime; // 【关键】引入 UGF 的数据表命名空间

namespace AVGGame
{
    public enum StoryNodeType
    {
        Dialogue = 0,
        Choice = 1,
        ChangeGraph = 2,
        Reward = 3,
        Video = 4 // CG视频节点
    }


    /// <summary>
    /// 【核心修改】继承自 UGF 的 DataRowBase
    /// 这样 UGF 的 DataTable 模块才能认识它、读取它
    /// </summary>
    public class StoryRowData : DataRowBase
    {
        private int m_Id = 0;

        /// <summary>
        /// 重写基类的 Id 属性 (UGF 强制要求)
        /// </summary>
        public override int Id
        {
            get { return m_Id; }
        }

        // --- 我们自己的业务数据 ---
        public int NextId;
        public int NodeType;
        public string SpeakerName;
        public string DialogText;
        public string CharacterActionsJson;
        public string ChoicesJson;
        public string TargetGraphName;
        public string RewardsJson;
        public string BgmPath;
        public string SePath;
        public string PerformanceKey;
        public string BackgroundPath;
        public string VoicePath;
        public bool HideDialoguePanel;
        public string VideoPath;       // 第16列：CG视频文件名（放在 StreamingAssets/CG/ 下）

        /// <summary>
        /// 【核心魔法】UGF 引擎在加载 txt 数据表时，每读一行都会自动调用这个方法
        /// dataRowString 就是 txt 里的一行文本（比如："10000\t10001\t0\t主角\t你好\t\t\t"）
        /// </summary>
        public override bool ParseDataRow(string dataRowString, object userData)
        {
            // 1. 按照制表符 (Tab键, '\t') 切割这一行文本
            string[] columnTexts = dataRowString.Split('\t');

            // 2. 防御性编程：检查列数是否对得上我们导出的 16 列
            if (columnTexts.Length < 16)
            {
                // 补足空列，防止后续访问越界（向后兼容只有15列的旧表）
                string[] tempArray = new string[16];
                for (int i = 0; i < columnTexts.Length; i++)
                {
                    tempArray[i] = columnTexts[i];
                }
                columnTexts = tempArray;
            }

            // 3. 依次把切割出来的文本， 转换类型并赋值给我们的变量
            // 这里的顺序【必须】和 StoryGraphExporter 导出的表头顺序一模一样！
            int index = 0;

            m_Id = SafeGetInt(columnTexts, index++);             // 第1列：Id
            NextId = SafeGetInt(columnTexts, index++);           // 第2列：NextId
            NodeType = SafeGetInt(columnTexts, index++);         // 第3列：NodeType

            SpeakerName = SafeGetString(columnTexts, index++);   // 第4列：说话人
            DialogText = SafeGetString(columnTexts, index++);    // 第5列：台词文本
            CharacterActionsJson = SafeGetString(columnTexts, index++);  // 第6列：立绘JSON
            ChoicesJson = SafeGetString(columnTexts, index++);   // 第7列：选项JSON
            TargetGraphName = SafeGetString(columnTexts, index++); // 第8列：目标图名
            RewardsJson = SafeGetString(columnTexts, index++);    // 第9列：奖励JSON
            BgmPath = SafeGetString(columnTexts, index++);        // 第10列：BGM
            SePath = SafeGetString(columnTexts, index++);         // 第11列：SE
            PerformanceKey = SafeGetString(columnTexts, index++);  //第12列：动画效果Key
            BackgroundPath = SafeGetString(columnTexts, index++);  //第13列：背景图
            VoicePath = SafeGetString(columnTexts, index++);       //第14列：语音
            HideDialoguePanel = SafeGetBool(columnTexts, index++); //第15列：隐藏对话框
            VideoPath = SafeGetString(columnTexts, index++);       //第16列：CG视频文件名

            Debug.Log($"[StoryRowData] 解析成功! Id={m_Id}, NextId={NextId}, NodeType={NodeType}");
            return true;
        }

        /// <summary>
        /// UGF 要求的二进制读取重写 (如果是加密的 bytes 格式表会调用这个)
        /// 如果你项目里暂时只读 txt，这个方法空着返回 false 或者抛异常也行，但必须写上。
        /// </summary>
        public override bool ParseDataRow(byte[] dataRowBytes, int startIndex, int length, object userData)
        {
            // 简单的二进制读取通常需要配合 UGF 官方的 DataTableGenerator 生成代码
            // 这里为了初学者跑通， 先略过二进制解析实现
            throw new NotImplementedException("暂未实现二进制读取，请确保使用 TXT 格式的数据表！");
        }

        // 安全获取字符串， 处理空值或索引越界
        private string SafeGetString(string[] array, int index)
        {
            if (index < 0 || index >= array.Length)
                return "";
            return array[index];
        }

        // 安全获取整数， 处理空值或索引越界
        private int SafeGetInt(string[] array, int index)
        {
            if (index < 0 || index >= array.Length)
                return 0;
            string value = array[index];
            if (string.IsNullOrEmpty(value))
                return 0;
            if (int.TryParse(value, out int result))
                return result;
            return 0;
        }

        // 安全获取布尔值
        private bool SafeGetBool(string[] array, int index)
        {
            if (index < 0 || index >= array.Length)
                return false;
            string value = array[index]?.Trim();
            if (string.IsNullOrEmpty(value))
                return false;
            return value == "1" || value.Equals("true", System.StringComparison.OrdinalIgnoreCase);
        }
    }

    [System.Serializable]
    public class CharacterDisplayData
    {
        public string CharacterName;
        public CharacterActionType ActionType;
        public CharacterPosition Position;
        public string SpritePath;

        /// <summary>
        /// X轴偏移（像素），基于1920x1080分辨率
        /// </summary>
        public float OffsetX = 0f;

        /// <summary>
        /// Y轴偏移（像素），基于1920x1080分辨率
        /// </summary>
        public float OffsetY = 0f;

        /// <summary>
        /// 缩放比率，1.0 = 原始大小
        /// </summary>
        public float Scale = 1f;
    }

    [System.Serializable]
    public class ChoiceItemData
    {
        public string ChoiceText;
        public List<ChoiceCondition> Conditions = new List<ChoiceCondition>();
        public List<ChoiceReward> Rewards = new List<ChoiceReward>();
    }


    [Serializable]
    public class RuntimeChoiceData
    {
        public string ChoiceText;
        public int NextId;
        public List<ChoiceCondition> Conditions;
        public List<ChoiceReward> Rewards;
    }

    [Serializable]
    public class RuntimeActionListWrapper { public List<CharacterDisplayData> actions; }

    [Serializable]
    public class RuntimeChoiceListWrapper
    {
        public List<RuntimeChoiceData> Choices;
    }

    [Serializable]
    public class RuntimeRewardListWrapper
    {
        public string RewardTitle;
        public List<ChoiceReward> Rewards;
    }
}