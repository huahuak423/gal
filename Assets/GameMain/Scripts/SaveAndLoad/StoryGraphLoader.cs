using GameFramework.Event;
using UnityGameFramework.Runtime;
using GameFramework.DataTable;

namespace AVGGame
{
   public class StoryGraphLoader 
    {
        // 记得在系统初始化时调用这个监听，或者写在 Procedure 的 OnEnter 里
        public void InitListener()
        {
            GameEntry.Event.Subscribe(LoadDataTableSuccessEventArgs.EventId, OnLoadSuccess);
            GameEntry.Event.Subscribe(LoadDataTableFailureEventArgs.EventId, OnLoadFailure);
        }

        public void RemoveListener()
        {
            GameEntry.Event.Unsubscribe(LoadDataTableSuccessEventArgs.EventId, OnLoadSuccess);
            GameEntry.Event.Unsubscribe(LoadDataTableFailureEventArgs.EventId, OnLoadFailure);
        }

        // ==========================================
        // 1. 加载目标图 (传入如 "Story_Park_01")
        // ==========================================
        public void LoadGraph(string graphName)
        {
            // 防御：如果内存里已经有这张图了，直接开始播，不重复加载
            // 注意：这里传入了 graphName 作为表的别名！
            if (GameEntry.DataTable.HasDataTable<StoryRowData>(graphName))
            {
                Log.Info($"[StoryLoader] 剧情图 {graphName} 已在内存中，直接播放！");
                StartPlayStory(graphName);
                return;
            }

            Log.Info($"[StoryLoader] 开始从硬盘加载剧情图: {graphName} ...");

            // 【核心大招】：向 UGF 申请一个以图名命名的专属空账本
            IDataTable<StoryRowData> newTable = GameEntry.DataTable.CreateDataTable<StoryRowData>(graphName);

            // 强转为底层基类准备吸数据
            DataTableBase tableBase = (DataTableBase)newTable;
            
            // 拼凑 txt 文件的真实路径 (请根据你的实际文件夹结构修改)
            string path = $"Assets/GameMain/DataTables/Story/{graphName}.txt";

            // 【妙用取衣小票】：我们把 graphName 作为 userData 传进去！
            // 这样等回调成功时，我们就知道到底是哪个图加载完了。
            tableBase.ReadData(path, 0, graphName); 
        }

        // ==========================================
        // 2. 卸载目标图 (在退回大地图时调用)
        // ==========================================
        public void UnloadGraph(string graphName)
        {
            if (GameEntry.DataTable.HasDataTable<StoryRowData>(graphName))
            {
                // 瞬间从内存中抹除这张表，释放空间！
                GameEntry.DataTable.DestroyDataTable<StoryRowData>(graphName);
                Log.Info($"<color=cyan>[StoryLoader] 剧情图 {graphName} 已安全卸载！</color>");
            }
        }

        // ==========================================
        // 3. 异步回调处理
        // ==========================================
        private void OnLoadSuccess(object sender, GameEventArgs e)
        {
            LoadDataTableSuccessEventArgs ne = (LoadDataTableSuccessEventArgs)e;

            // 核对小票：看看传回来的 UserData 是不是一个字符串 (即我们的 graphName)
            string loadedGraphName = ne.UserData as string;
            
            // 如果不是字符串，说明是其他表（比如 EventPool）加载成功了，我们不理它
            if (string.IsNullOrEmpty(loadedGraphName)) return; 

            Log.Info($"<color=green>[StoryLoader] 剧情图 {loadedGraphName} 加载成功！</color>");
            
            // 数据已经安稳躺在内存里了，马上拉起 UI 播放！
            StartPlayStory(loadedGraphName);
        }

        private void OnLoadFailure(object sender, GameEventArgs e)
        {
            LoadDataTableFailureEventArgs ne = (LoadDataTableFailureEventArgs)e;
            
            string failedGraphName = ne.UserData as string;
            if (string.IsNullOrEmpty(failedGraphName)) return;

            Log.Error($"[StoryLoader] 翻车了！剧情图 {failedGraphName} 找不到。报错: {ne.ErrorMessage}");
        }

        // ==========================================
        // 4. 正式播放逻辑 (对接你的 UI)
        // ==========================================
        private void StartPlayStory(string graphName)
        {
            // 以后你要拿这个图的数据，都要带上 graphName 这个别名去拿：
            IDataTable<StoryRowData> dt = GameEntry.DataTable.GetDataTable<StoryRowData>(graphName);
            
            // TODO: 获取图里的第一句话（比如 dt.GetDataRow(1)），然后发给 DialoguePanel 显示！
        }
    }
}