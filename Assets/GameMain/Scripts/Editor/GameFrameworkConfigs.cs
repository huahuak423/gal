using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework;
using UnityGameFramework.Editor.ResourceTools;
using System.IO;

namespace AVGGame.Editor
{
    public static class GameFrameworkConfigs
    {
        [ResourceEditorConfigPath]
        public static string ResourceEditorConfig = Utility.Path.GetRegularPath(Path.Combine(Application.dataPath, "GameMain/Configs/ResourceEditor.xml"));
    }
}