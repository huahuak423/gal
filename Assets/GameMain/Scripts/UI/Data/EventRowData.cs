using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime; // 必须引入 UGF 运行时

namespace AVGGame
{
    /// <summary>
    /// 事件池数据表实体类
    /// </summary>
    public class EventRowData : DataRowBase
    {
        // UGF 强制要求重写的 ID 属性
        private int m_Id = 0;
        public override int Id => m_Id;

        // --- 对应 Excel 里的字段 ---
        public int MapId { get; private set; }
        public int EventType { get; private set; }
        public string Title { get; private set; }
        public int CostAP { get; private set; }
        public int ReqNpcId { get; private set; }
        public string VisibleConditions { get; private set; }
        public string PlayableConditions { get; private set; }
        public string TargetGraphName { get; private set; }

        /// <summary>
        /// UGF 核心解析方法：读取 txt 的每一行时自动调用
        /// </summary>
        public override bool ParseDataRow(string dataRowString, object userData)
        {
            string[] columnTexts = dataRowString.Split('\t');

            // 防御检查：现在我们至少需要 10 列（即使描述列是空的也要占位）
            // 假设第 0 列是“描述”，第 1 列是 Id
            if (columnTexts.Length < 10)
            {
                Log.Warning($"[EventRowData] 列数不足，解析失败: {dataRowString}");
                return false;
            }

            int index = 0;
            
            // 跳过第 0 列 (中文描述列，机器不读)
            index++; 

            // 依次解析你的最新字段
            m_Id = int.Parse(columnTexts[index++]);
            MapId = ParseInt(columnTexts[index++]);
            EventType = ParseInt(columnTexts[index++]); // 用封装的安全解析法
            Title = columnTexts[index++];
            CostAP = ParseInt(columnTexts[index++]);
            ReqNpcId = ParseInt(columnTexts[index++]);
            VisibleConditions = columnTexts[index++];
            PlayableConditions = columnTexts[index++];
            TargetGraphName = columnTexts[index++];

            return true;
        }

        /// <summary>
        /// 二进制读取接口（极简路线暂不需要，但必须保留空实现以防报错）
        /// </summary>
        public override bool ParseDataRow(byte[] dataRowBytes, int startIndex, int length, object userData)
        {
            return false; 
        }
        
        /// <summary>
        /// 安全的 int 解析，防止 Excel 里格子为空导致崩溃
        /// </summary>
        private int ParseInt(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0;
            if (int.TryParse(text, out int result)) return result;
            return 0;
        }
    }
}