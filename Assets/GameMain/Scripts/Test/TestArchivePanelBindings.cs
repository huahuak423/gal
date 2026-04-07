//------------------------------------------------------------
// Game Framework
// AVG Game Project
// ArchivePanel 绑定测试脚本
//------------------------------------------------------------

using UnityEngine;
using UnityGameFramework.Runtime;

namespace AVGGame
{
    /// <summary>
    /// ArchivePanel 绑定测试脚本
    /// </summary>
    public class TestArchivePanelBindings : MonoBehaviour
    {
        private void Start()
        {
            // 查找场景中的 ArchivePanel
            ArchivePanel[] panels = FindObjectsOfType<ArchivePanel>();

            if (panels.Length == 0)
            {
                Log.Warning("[TestArchivePanelBindings] 场景中没有找到 ArchivePanel");
                return;
            }

            foreach (var panel in panels)
            {
                Log.Info($"[TestArchivePanelBindings] 测试 ArchivePanel: {panel.name}");
                TestPanelBindings(panel);
            }
        }

        private void TestPanelBindings(ArchivePanel panel)
        {
            Button[] saveSlots = new[]
            {
                panel.m_SaveSlot1,
                panel.m_SaveSlot2,
                panel.m_SaveSlot3,
                panel.m_SaveSlot4,
                panel.m_SaveSlot5,
                panel.m_SaveSlot6,
                panel.m_SaveSlot7,
                panel.m_SaveSlot8,
                panel.m_SaveSlot9,
                panel.m_SaveSlot10,
                panel.m_SaveSlot11,
                panel.m_SaveSlot12
            };

            Log.Info("[TestArchivePanelBindings] 检查存档按钮绑定:");

            for (int i = 0; i < saveSlots.Length; i++)
            {
                if (saveSlots[i] != null)
                {
                    Log.Info($"[TestArchivePanelBindings] ✓ SaveSlot{i + 1}: {saveSlots[i].name} (Path: {GetGameObjectPath(saveSlots[i].gameObject)})");
                }
                else
                {
                    Log.Error($"[TestArchivePanelBindings] ✗ SaveSlot{i + 1} is null");
                }
            }

            Log.Info("[TestArchivePanelBindings] 检查其他组件:");

            Log.Info($"[TestArchivePanelBindings] ConfirmButton: {(panel.m_ConfirmButton != null ? "✓" : "✗")}");
            Log.Info($"[TestArchivePanelBindings] CancelButton: {(panel.m_CancelButton != null ? "✓" : "✗")}");
            Log.Info($"[TestArchivePanelBindings] ConfirmPlate: {(panel.m_ConfirmPlate != null ? "✓" : "✗")}");
            Log.Info($"[TestArchivePanelBindings] ButtonBack: {(panel.m_ButtonBack != null ? "✓" : "✗")}");
            Log.Info($"[TestArchivePanelBindings] TitleText: {(panel.m_TitleText != null ? "✓" : "✗")}");
        }

        private string GetGameObjectPath(GameObject obj)
        {
            if (obj == null) return "null";

            string path = obj.name;
            Transform parent = obj.transform.parent;

            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }

            return path;
        }
    }
}