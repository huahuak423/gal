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

            Debug.Log($"[EventRowData] 解析行数据，列数: {columnTexts.Length}");
            Debug.Log($"[EventRowData] 原始数据: {dataRowString}");

            // EventPool.txt 最多12列，统一补齐防止越界
            const int requiredColCount = 12;
            if (columnTexts.Length < requiredColCount)
            {
                Debug.LogWarning($"[EventRowData] 列数不足{requiredColCount}列，当前{columnTexts.Length}列，补空列");
                string[] tempArray = new string[requiredColCount];
                for (int i = 0; i < columnTexts.Length; i++)
                {
                    tempArray[i] = columnTexts[i];
                }
                columnTexts = tempArray;
            }

            // 检测表头的空列数（跳过 ## 前缀），动态确定起始索引
            // 表头格式: ##\t\tId\tMapId... （2个空列）
            // 数据格式: \tId\tMapId...     （1个空列）
            // 通过检测前几个非空列来判断实际偏移
            int index = 0;
            if (columnTexts.Length > 0 && columnTexts[0] == "##")
            {
                // 表头行，跳过 ## 后找到第一个实际列名
                index = 0;
            }
            else
            {
                // 数据行：找到第一个非空且非数字前缀的列作为 Id
                // 数据行第一个元素通常是空的（制表符开头），第二个元素是 Id
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

            // EventType=1 有额外的空列，需要跳过
            if (EventType == 1)
            {
                index++;
            }

            TargetGraphName = SafeGetString(columnTexts, index++);

            Debug.Log(
                $"[EventRowData] 解析成功 - ID: {m_Id}, MapId: {MapId}, EventType: {EventType}, Title: {Title}, TargetGraphName: {TargetGraphName}");
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

        /// <summary>
        /// 安全的 string 解析，防止索引越界
        /// </summary>
        private string SafeGetString(string[] array, int index)
        {
            if (index < 0 || index >= array.Length)
                return "";
            return array[index];
        }
    }
}