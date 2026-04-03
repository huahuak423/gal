using UnityGameFramework.Runtime;


namespace AVGGame
{
    /// <summary>
    /// 演出效果配置表 (极简字符串传参 + 安全防空版)
    /// </summary>
    public class PerformanceDataRow : DataRowBase
    {
        // 第一列默认是 public int Id; (继承自 DataRowBase)
        private int m_Id;
        public override int Id
        {
            get { return m_Id; }
        }
        public string BackgroundEffect { get; private set; }
        public string CharacterEffect { get; private set; }
        public string ScreenEffect { get; private set; }

        /// <summary>
        /// UGF 核心解析逻辑
        /// </summary>
        public override bool ParseDataRow(string dataRowString, object userData)
        {
            // 假设使用制表符分割 (具体根据你的 UGF 导表工具配置调整)
            string[] columnStrings = dataRowString.Split('\t');
            int index = 0;
            
            // 1. 解析主键
            m_Id = int.Parse(columnStrings[index++]); 
            
            // 2. 使用安全清洗函数读取字符串
            BackgroundEffect = ParseStringSafe(columnStrings[index++]);
            CharacterEffect  = ParseStringSafe(columnStrings[index++]);
            ScreenEffect     = ParseStringSafe(columnStrings[index++]);

            return true;
        }

        /// <summary>
        /// 安全读取字符串：遇到 null、空字符或纯空格时，自动补上 "" 
        /// 并去除首尾多余的不可见字符
        /// </summary>
        private string ParseStringSafe(string value)
        {
            // string.IsNullOrWhiteSpace 会把 null、""、以及纯空格 "   " 全部精准拦截
            if (string.IsNullOrWhiteSpace(value))
            {
                return ""; 
            }
            
            // 顺手去掉字符串前后的多余空格，防止策划手抖多敲了空格导致 "Shake " 无法匹配 "Shake"
            return value.Trim();
        }
    }
}