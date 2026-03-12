using System;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime; // 【关键】引入 UGF 的数据表命名空间

namespace AVGGame.Runtime
{
    public enum StoryNodeType
    {
        Dialogue = 0,
        Choice = 1,
        ChangeGraph = 2
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

        /// <summary>
        /// 【核心魔法】UGF 引擎在加载 txt 数据表时，每读一行都会自动调用这个方法
        /// dataRowString 就是 txt 里的一行文本（比如："10000\t10001\t0\t主角\t你好\t\t\t"）
        /// </summary>
        public override bool ParseDataRow(string dataRowString, object userData)
        {
            // 1. 按照制表符 (Tab键, '\t') 切割这一行文本
            string[] columnTexts = dataRowString.Split('\t');

            // 2. 防御性编程：检查列数是否对得上我们导出的 8 列
            if (columnTexts.Length < 8)
            {
                Debug.LogError($"解析剧情数据表失败！行文本列数不足: {dataRowString}");
                return false;
            }

            // 3. 依次把切割出来的文本，转换类型并赋值给我们的变量
            // 这里的顺序【必须】和 StoryGraphExporter 导出的表头顺序一模一样！
            int index = 0;
            
            m_Id = int.Parse(columnTexts[index++]);             // 第1列：Id
            NextId = int.Parse(columnTexts[index++]);           // 第2列：NextId
            NodeType = int.Parse(columnTexts[index++]);         // 第3列：NodeType
            
            SpeakerName = columnTexts[index++];                 // 第4列：说话人
            DialogText = columnTexts[index++];                  // 第5列：台词文本
            CharacterActionsJson = columnTexts[index++];        // 第6列：立绘JSON
            ChoicesJson = columnTexts[index++];                 // 第7列：选项JSON
            TargetGraphName = columnTexts[index++];             // 第8列：目标图名

            return true;
        }

        /// <summary>
        /// UGF 要求的二进制读取重写 (如果是加密的 bytes 格式表会调用这个)
        /// 如果你项目里暂时只读 txt，这个方法空着返回 false 或者抛异常也行，但必须写上。
        /// </summary>
        public override bool ParseDataRow(byte[] dataRowBytes, int startIndex, int length, object userData)
        {
            // 简单的二进制读取通常需要配合 UGF 官方的 DataTableGenerator 生成代码
            // 这里为了初学者跑通，先略过二进制解析实现
            throw new NotImplementedException("暂未实现二进制读取，请确保使用 TXT 格式的数据表！");
        }
    }

    // ==========================================
    // 运行时的 JSON 包装结构 (保持不变)
    // ==========================================

    [Serializable]
    public class RuntimeChoiceData
    {
        public string ChoiceText;
        public int NextId; 
        public List<ChoiceCondition> Conditions;
        public List<ChoiceReward> Rewards;
    }

    [Serializable]
    public class RuntimeChoiceListWrapper
    {
        public List<RuntimeChoiceData> Choices;
    }
}