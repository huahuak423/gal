//------------------------------------------------------------
// Game Framework
// AVG Game Project
//------------------------------------------------------------

using GameFramework.UI;
using UnityGameFramework.Runtime;
using AVGGame;

namespace AVGGame
{
    /// <summary>
    /// UI 扩展方法
    /// </summary>
    public static class UIExtension
    {
        /// <summary>
        /// 打开 UI（使用 UIFormId）
        /// </summary>
        public static int OpenUIFormById(this UIComponent uiComponent, int uiFormId, object userData = null)
        {
            string assetName = AssetUtility.GetUIFormAsset(uiFormId);
            return uiComponent.OpenUIForm(assetName, UIGroupDefinition.Main, Constant.AssetPriority.UIAsset, userData);
        }

        /// <summary>
        /// 打开 UI（指定分组）
        /// </summary>
        public static int OpenUIFormById(this UIComponent uiComponent, int uiFormId, string uiGroupName, object userData = null)
        {
            string assetName = AssetUtility.GetUIFormAsset(uiFormId);
            return uiComponent.OpenUIForm(assetName, uiGroupName, Constant.AssetPriority.UIAsset, userData);
        }

        /// <summary>
        /// 关闭指定分组的所有 UI
        /// </summary>
        public static void CloseUIFormsByGroup(this UIComponent uiComponent, string uiGroupName)
        {
            IUIGroup group = uiComponent.GetUIGroup(uiGroupName);
            if (group != null)
            {
                IUIForm[] forms = group.GetAllUIForms();
                for (int i = forms.Length - 1; i >= 0; i--)
                {
                    uiComponent.CloseUIForm(forms[i].SerialId);
                }
            }
        }

        /// <summary>
        /// 检查 UI 是否打开
        /// </summary>
        public static bool IsUIFormOpen(this UIComponent uiComponent, int uiFormId)
        {
            string assetName = AssetUtility.GetUIFormAsset(uiFormId);
            IUIForm[] forms = uiComponent.GetAllLoadedUIForms();
            foreach (var form in forms)
            {
                if (form.UIFormAssetName == assetName)
                {
                    return true;
                }
            }
            return false;
        }
    }
}