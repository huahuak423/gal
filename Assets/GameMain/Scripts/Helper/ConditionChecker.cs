using System;
using System.Collections.Generic;
using UnityGameFramework.Runtime;

namespace AVGGame
{
   /// <summary>
    /// 统一的条件检测工具类
    /// </summary>
    public static class ConditionChecker
    {
        /// <summary>
        /// 核心判断函数：传入条件字符串和玩家数据，返回是否满足所有条件
        /// </summary>
        public static bool CheckCondition(string conditionStr, PlayerDataComponent playerData)
        {
            // 如果字符串为空，代表没有任何条件限制，直接放行 (return true)
            if (string.IsNullOrWhiteSpace(conditionStr))
            {
                return true;
            }

            // 0. 清洗：去除首尾引号和空白（兼容 Excel/策划手动加引号的情况）
            conditionStr = conditionStr.Trim().Trim('"');

            // 1. 按逗号拆分出所有并列的条件规则 (比如 "2|2|1,5|10|1" 会被拆成两个)
            string[] conditions = conditionStr.Split(',');

            foreach (string rawCond in conditions)
            {
                // 2. 清洗每条子条件后按竖线拆分参数
                string cond = rawCond.Trim().Trim('"');
                string[] parts = cond.Split('|');

                if (parts.Length < 3)
                {
                    Log.Warning($"[ConditionChecker] 条件格式错误: {cond}");
                    continue;
                }

                // 解析基本参数
                int ruleType;
                int param1;
                int param2;
                try
                {
                    ruleType = int.Parse(parts[0]);
                    param1 = int.Parse(parts[1]);
                    param2 = int.Parse(parts[2]);
                }
                catch (FormatException)
                {
                    Log.Warning($"[ConditionChecker] 条件参数解析失败: {cond}");
                    return false;
                }

                // 3. 根据不同的规则序号，执行对应逻辑。
                // 只要有一条规则不满足，直接返回 false！
                switch (ruleType)
                {
                    case 1: // NPC事件前置 (1 | NpcId | EventId)
                        if (!playerData.HasCompletedNpcEvent(param1, param2)) return false;
                        break;

                    case 5: // 物品检测 (5 | ItemId | 1为必须有, 2为必须没有)
                        bool hasItem = playerData.HasItem(param1);
                        if (param2 == 1 && !hasItem) return false;      // 要求有但没有
                        if (param2 == 2 && hasItem) return false;       // 要求没有却有
                        break;

                    case 6: // NPC好感度检测 (6 | NpcId | Value | Operator)
                        if (parts.Length < 4)
                        {
                            Log.Warning($"[ConditionChecker] NPC好感度条件格式错误(需要4段): {cond}");
                            return false;
                        }
                        int npcId = param1;
                        int favorValue = param2;
                        int favorOp = int.Parse(parts[3]);
                        int favorability = playerData.GetFavorability(npcId);
                        if (!CompareValue(favorability, favorValue, favorOp)) return false;
                        break;

                    default:
                        Log.Warning($"[ConditionChecker] 未知的规则类型: {ruleType}");
                        break;
                }
            }

            // 所有条件都熬过来了，说明全部满足！
            return true;
        }

        /// <summary>
        /// 提取出来的比较通用方法（1：大于等于，2：小于等于，3：等于）
        /// </summary>
        private static bool CompareValue(int playerValue, int targetValue, int operatorType)
        {
            switch (operatorType)
            {
                case 1: return playerValue >= targetValue;
                case 2: return playerValue <= targetValue;
                case 3: return playerValue == targetValue;
                default: return false;
            }
        }
    }
}