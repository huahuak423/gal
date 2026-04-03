using System.Collections.Generic;
using AVGGame;
    
namespace AVGGame.Editor
{
    /// <summary>
    /// 单个槽位的记忆偏移数据
    /// </summary>
    [System.Serializable]
    public class CharacterSlotOffsetMemory
    {
        public float OffsetX;
        public float OffsetY;
        public float Scale;

        public CharacterSlotOffsetMemory()
        {
            OffsetX = 0;
            OffsetY = 0;
            Scale = 1f;
        }

        public CharacterSlotOffsetMemory(float offsetX, float offsetY, float scale)
        {
            OffsetX = offsetX;
            OffsetY = offsetY;
            Scale = scale;
        }
    }

    /// <summary>
    /// 剧情状态快照：记录某一个对话节点所对应的完整画面状态
    /// </summary>
    public class StoryStateSnapshot
    {
        /// <summary>
        /// 当前台词文本
        /// </summary>
        public string DialogText;

        /// <summary>
        /// 角色名称（说话人）
        /// </summary>
        public string CharacterName;

        /// <summary>
        /// 立绘列表（直接复用 CharacterDisplayData，只处理左中右三个位置）
        /// </summary>
        public List<CharacterDisplayData> CharacterRoster = new List<CharacterDisplayData>();

        /// <summary>
        /// 当前背景音乐路径
        /// </summary>
        public string BgmPath;

        /// <summary>
        /// 当前背景图路径
        /// </summary>
        public string BackgroundPath;

        /// <summary>
        /// 槽位偏移记忆（同一个节点图内，三个槽位独立记忆最近的偏移和缩放）
        /// </summary>
        public CharacterSlotOffsetMemory[] SlotOffsetMemory = new CharacterSlotOffsetMemory[3]
        {
            new CharacterSlotOffsetMemory(),
            new CharacterSlotOffsetMemory(),
            new CharacterSlotOffsetMemory()
        };
    }
}
