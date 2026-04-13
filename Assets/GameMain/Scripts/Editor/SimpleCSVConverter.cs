#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Text.RegularExpressions; // 【新增】必须引入正则命名空间

namespace AVGGame.Editor
{
    public class SimpleCSVConverter : EditorWindow
    {
        [MenuItem("AVG Tools/3. 极简 CSV 转换 UGF 事件池数据表")]
        public static void ConvertCSVToTXT()
        {
            string inputPath = EditorUtility.OpenFilePanel("选择要导出的 CSV 表格", "Assets/", "csv");
            if (string.IsNullOrEmpty(inputPath)) return;

            string[] lines = File.ReadAllLines(inputPath, Encoding.UTF8);
            StringBuilder sb = new StringBuilder();

            // 写入表头（## 开头会被 UGF 数据表解析跳过）
            sb.AppendLine("##\t\tId\tMapId\tEventType\tTitle\tCostAP\tEventNum\tVisibleConditions\tPlayableConditions\t\tTargetGraphName");
            sb.AppendLine("##\t\tint\tint\tint\tstring\tint\tstring\tstring\tstring\t\tstring");
            sb.AppendLine("##\t\t编号\t地图ID\t类型\t标题\t消耗AP\t事件编号\t可见条件\t可玩条件\t\t目标图名称");

            for (int i = 3; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;

                // 【核心升级】：这句魔法正则的意思是“只切割后面跟着偶数个引号的逗号”
                // 这样就能完美避开被双引号包裹的内部逗号
                string[] columns = Regex.Split(line, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");

                for (int j = 0; j < columns.Length; j++)
                {
                    string col = columns[j];
                    // Excel 会给带逗号的文本加上首尾双引号，我们需要把它脱掉
                    if (col.StartsWith("\"") && col.EndsWith("\""))
                    {
                        col = col.Substring(1, col.Length - 2);
                        // Excel 会把文本里原本的双引号转义为两个双引号 ("")，我们还原回去
                        col = col.Replace("\"\"", "\"");
                    }
                    columns[j] = col;
                }

                // 用 UGF 标准的制表符 \t 重新把它们拼接起来
                string replacedLine = string.Join("\t", columns);
                sb.AppendLine(replacedLine);
            }

            string outputPath = inputPath.Replace(".csv", ".txt");
            File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
            
            AssetDatabase.Refresh();
            Debug.Log($"<color=green>导表成功！已安全处理所有特殊符号: {outputPath}</color>");
        }
    }
}
#endif