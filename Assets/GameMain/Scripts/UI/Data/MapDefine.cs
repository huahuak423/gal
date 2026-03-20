using System.Collections.Generic;

namespace AVGGame
{
    public static class MapDefine
    {
        public static readonly Dictionary<int, string> MapNames = new Dictionary<int, string>
        {
            { 1, "居家" },
            { 2, "商店" },
            { 3, "医院" },
            { 4, "公园" },
            { 5, "书店" },
            { 6, "游乐园" },
            { 7, "电影院" },
            { 8, "美甲店" },
            { 9, "理发店" }
        };

        public static string GetMapName(int mapId)
        {
            return MapNames.TryGetValue(mapId, out string name) ? name : "none";
        }
    }
}