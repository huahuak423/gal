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
        public string EventNum { get; private set; }
        public string VisibleConditions { get; private set; }
        public string PlayableConditions { get; private set; }
        public string TargetGraphName { get; private set; }

        /// <summary>
        /// UGF 核心解析方法：读取 txt 的每一行时自动调用
        /// </summary>
        public override bool ParseDataRow(string dataRowString, object userData)
        {
            string[] columnTexts = dataRowString.Split('\t');

            if (columnTexts.Length < 9)
            {
                string[] tempArray = new string[9];
                for (int i = 0; i < columnTexts.Length; i++)
                {
                    tempArray[i] = columnTexts[i];
                }
                columnTexts = tempArray;
            }

            int index = 0;
            if (columnTexts.Length > 0 && columnTexts[0] == "##")
            {
                index = 0;
            }
            else
            {
                index = 1;
            }

            m_Id = SafeGetInt(columnTexts, index++);
            MapId = SafeGetInt(columnTexts, index++);
            EventType = SafeGetInt(columnTexts, index++);
            Title = SafeGetString(columnTexts, index++);
            CostAP = SafeGetInt(columnTexts, index++);
            EventNum = SafeGetString(columnTexts, index++);
            VisibleConditions = SafeGetString(columnTexts, index++);
            PlayableConditions = SafeGetString(columnTexts, index++);
            TargetGraphName = SafeGetString(columnTexts, index++);

            return true;
        }

        public override bool ParseDataRow(byte[] dataRowBytes, int startIndex, int length, object userData)
        {
            return false;
        }

        private int SafeGetInt(string[] array, int index)
        {
            if (index < 0 || index >= array.Length) return 0;
            string value = array[index];
            if (string.IsNullOrEmpty(value)) return 0;
            if (int.TryParse(value, out int result)) return result;
            return 0;
        }

        private string SafeGetString(string[] array, int index)
        {
            if (index < 0 || index >= array.Length) return "";
            return array[index];
        }
    }
}