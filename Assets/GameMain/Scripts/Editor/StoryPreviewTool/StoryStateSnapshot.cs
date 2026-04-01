using System.Collections.Generic;
using AVGGame;
    
namespace AVGGame.Editor
{
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
    }
}
