using UnityEngine;
using UnityGameFramework.Runtime; // 必须引入 UGF 运行时

namespace AVGGame
{
    /// <summary>
    /// 男主（角色）信息配置表行实体类
    /// 对应 Assets/GameMain/DataTables/CharacterTable.txt
    /// 供 InformationPanel（男主好感度界面）读取，替代代码内硬编码数据
    /// </summary>
    public class CharacterRowData : DataRowBase
    {
        // UGF 强制要求重写的 ID 属性
        private int m_Id = 0;
        public override int Id => m_Id;

        // --- 对应表里的字段 ---
        public string Name { get; private set; }            // 姓名
        public string Age { get; private set; }             // 年龄
        public string Work { get; private set; }            // 职业
        public string PortraitPath { get; private set; }    // 立绘路径
        public string QutePath { get; private set; }        // Q版立绘/按钮图路径

        /// <summary>
        /// UGF 核心解析方法：读取 txt 的每一行时自动调用
        /// </summary>
        public override bool ParseDataRow(string dataRowString, object userData)
        {
            string[] columnTexts = dataRowString.Split('\t');

            if (columnTexts.Length < 6)
            {
                string[] tempArray = new string[6];
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
            Name = SafeGetString(columnTexts, index++);
            Age = SafeGetString(columnTexts, index++);
            Work = SafeGetString(columnTexts, index++);
            PortraitPath = SafeGetString(columnTexts, index++);
            QutePath = SafeGetString(columnTexts, index++);

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
