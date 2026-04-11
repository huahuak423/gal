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

            Debug.Log($"[EventRowData] 解析行数据，列数: {columnTexts.Length}");
            Debug.Log($"[EventRowData] 原始数据: {dataRowString}");

            // EventPool.txt 开头有两个制表符，跳过两个空列
            int index = 2;

            // 先解析前几列来获取 EventType
            if (columnTexts.Length < 5)
            {
                Debug.LogWarning($"[EventRowData] 列数过少，无法解析基本字段，实际: {columnTexts.Length}");
                return false;
            }

            m_Id = ParseInt(columnTexts[index++]);
            MapId = ParseInt(columnTexts[index++]);
            EventType = ParseInt(columnTexts[index++]);

            // 根据 EventType 判断列结构
            // EventType=1: 12列（地图入口，有额外的空列）
            // EventType=2: 11列（角色事件，无额外空列）
            if (EventType == 1 && columnTexts.Length < 12)
            {
                Debug.LogWarning($"[EventRowData] EventType=1 需要12列，实际: {columnTexts.Length}");
                return false;
            }
            if (EventType == 2 && columnTexts.Length < 11)
            {
                Debug.LogWarning($"[EventRowData] EventType=2 需要11列，实际: {columnTexts.Length}");
                return false;
            }

            // 依次解析字段
            Title = columnTexts[index++];
            CostAP = ParseInt(columnTexts[index++]);
            ReqNpcId = ParseInt(columnTexts[index++]);
            VisibleConditions = columnTexts[index++];
            PlayableConditions = columnTexts[index++];

            // EventType=1 有额外的空列，需要跳过
            if (EventType == 1)
            {
                index++;
            }

            TargetGraphName = columnTexts[index++];

            Debug.Log($"[EventRowData] 解析成功 - ID: {m_Id}, MapId: {MapId}, EventType: {EventType}, Title: {Title}, TargetGraphName: {TargetGraphName}");
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
            if (string.IsNullOrWhiteSpace(text))
            {
                Debug.Log("[EventRowData] ParseInt: 空字符串，返回 0");
                return 0;
            }
            if (int.TryParse(text, out int result))
            {
                Debug.Log($"[EventRowData] ParseInt: '{text}' -> {result}");
                return result;
            }
            Debug.LogWarning($"[EventRowData] ParseInt: 无法解析 '{text}'，返回 0");
            return 0;
        }
    }
}