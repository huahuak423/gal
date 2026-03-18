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
        private int m_Id = 0;
        
        // UGF 强制要求重写的 ID 属性
        public override int Id => m_Id;

        // --- 对应 Excel 里的字段 ---
        public string MapId { get; private set; }
        public string Title { get; private set; }
        public int CostAP { get; private set; }
        public string VisibleConditions { get; private set; }
        public string PlayableConditions { get; private set; }
        public string UnplayableReason { get; private set; }

        /// <summary>
        /// UGF 核心解析方法：读取 txt 的每一行时自动调用
        /// </summary>
        public override bool ParseDataRow(string dataRowString, object userData)
        {
            // 按制表符切开
            string[] columnTexts = dataRowString.Split('\t');

            // 防御性检查：我们的表头有 7 列
            if (columnTexts.Length < 7)
            {
                Debug.LogError($"解析 EventRowData 失败，列数不足: {dataRowString}");
                return false;
            }

            // 依次赋值，注意这里的顺序必须和 Excel 里的列顺序完全一致！
            int index = 0;
            m_Id = int.Parse(columnTexts[index++]);
            MapId = columnTexts[index++];
            Title = columnTexts[index++];
            CostAP = int.Parse(columnTexts[index++]);
            VisibleConditions = columnTexts[index++];
            PlayableConditions = columnTexts[index++];
            UnplayableReason = columnTexts[index++];

            return true;
        }

        /// <summary>
        /// 二进制读取接口（极简路线暂不需要，但必须保留空实现以防报错）
        /// </summary>
        public override bool ParseDataRow(byte[] dataRowBytes, int startIndex, int length, object userData)
        {
            return false; 
        }
    }
}