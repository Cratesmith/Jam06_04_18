#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using System.IO;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using Object = UnityEngine.Object;

public class AutoGizmo : UnityEditor.AssetPostprocessor
{
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        if (BuildPipeline.isBuildingPlayer)
        {
            return;
        }

        var allAssets = importedAssets
            .Concat(movedAssets)
            .Concat(deletedAssets)
            .Concat(movedFromAssetPaths)
            .Distinct()
            .ToArray();

        bool iconsChanged = allAssets.Any(x =>
            new DirectoryInfo(Path.GetDirectoryName(x)).Name.Equals("Gizmos", StringComparison.CurrentCultureIgnoreCase)
            && x.EndsWith(" Icon.png", StringComparison.CurrentCultureIgnoreCase));

        bool scriptsChanged = allAssets.Any(x => x.EndsWith(".cs", StringComparison.CurrentCultureIgnoreCase) ||
                                                  x.EndsWith(".dll", StringComparison.CurrentCultureIgnoreCase));

        if (iconsChanged || scriptsChanged)
        {
            AssignIcons();   
        }
    }

    public static Texture2D Editor_GetIcon(Object forObject)
    {
        var ty = typeof(EditorGUIUtility);
        var mi = ty.GetMethod("GetIconForObject", BindingFlags.NonPublic | BindingFlags.Static);
        return mi.Invoke(null, new object[] { forObject }) as Texture2D;
    }

    public static void Editor_SetIcon(Object forObject, Texture2D iconTexture)
    {
        var ty = typeof(EditorGUIUtility);
        var mi2 = ty.GetMethod("SetIconForObject", BindingFlags.NonPublic | BindingFlags.Static);
        mi2.Invoke(null, new object[] { forObject, iconTexture });               
    }

    private static void AssignIcons()
    {
        var lookup = new Dictionary<string,string>();
        var suffix = " Icon.png";
        foreach (var file in Directory.GetFiles("Assets", "*"+suffix, SearchOption.AllDirectories))
        {
            var di = new DirectoryInfo(Path.GetDirectoryName(file));
            if (!di.Name.Equals("Gizmos", StringComparison.CurrentCultureIgnoreCase))
            {
                continue;
            }

            var filename = Path.GetFileName(file);
            var name = filename.Substring(0, filename.Length - suffix.Length).ToLower();
            lookup[name] = file;
        }
        
        var allScripts = Resources.FindObjectsOfTypeAll<MonoScript>();
        foreach (var script in allScripts.Where(x=>x.GetClass()!=null && typeof(UnityEngine.Object).IsAssignableFrom(x.GetClass())))
        {
            var type = script.GetClass();
            if (type == null) break;
            do
            {
                string iconPath = "";
                
                if (!lookup.TryGetValue( TrimGenericsFromType(type.FullName.ToLower()), out iconPath) &&
                    !lookup.TryGetValue( TrimGenericsFromType(type.Name.ToLower()), out iconPath))
                {
                    type = type.BaseType;
                    continue;
                }

                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);
                if (texture == null)
                {
                    type = type.BaseType;
                    continue;
                }



                Texture2D prevIcon = Editor_GetIcon(script);            
                if (texture != prevIcon)
                {
                    Editor_SetIcon(script,texture);
                }
                break;
            } while (type!=null);

            if (type == null)
            {
                Editor_SetIcon(script, null);
            }
        }
    }    

    static string TrimGenericsFromType(string name)
    {
        int index = name.IndexOf('`');
        if (index == -1)
        {
            return name;
        }
        return name.Substring(0, index);
    }

    [UnityEditor.InitializeOnLoad]
    class HeirachyDrawer
    {
        private static HeirachyDrawer m_instance;

        static HeirachyDrawer()
        {
            m_instance = new HeirachyDrawer();
            EditorApplication.hierarchyWindowItemOnGUI += m_instance.HeirachyWindowItemOnGUI;
            EditorApplication.projectWindowItemOnGUI += m_instance.ProjectWindowItemOnGUI;
            EditorApplication.RepaintHierarchyWindow();
        }

        private void HeirachyWindowItemOnGUI(int instanceid, Rect rect)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            var obj = EditorUtility.InstanceIDToObject(instanceid);
            if (obj == null)
            {
                return;
            }                               

            var go = obj as GameObject;
            if (go == null)
            {
                return;
            }
                        
            var texture = AutoGizmo.Editor_GetIcon(go);
            if(texture==null)
            {
                return;
            }
                
            Rect iconRect = new Rect(rect.x+(EditorGUI.indentLevel-1)*35, rect.y, 30, rect.height);
            GUI.DrawTexture(iconRect, texture, ScaleMode.ScaleToFit, true, (float)texture.width/texture.height);
        }

        private void ProjectWindowItemOnGUI(string guid, Rect rect)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            var assetName = AssetDatabase.GUIDToAssetPath(guid);
            ProjectWindowItemOnGUI_Scripts(rect, assetName);
            ProjectWindowItemOnGUI_Prefabs(rect, assetName);
        }

        private static void ProjectWindowItemOnGUI_Scripts(Rect rect, string assetName)
        {
            if (!assetName.EndsWith(".cs", StringComparison.CurrentCultureIgnoreCase))
            {
                return;
            }

            var script = AssetDatabase.LoadAssetAtPath<MonoScript>(assetName);
            if (script == null)
            {
                return;
            }

            var scriptIcon = EditorGUIUtility.ObjectContent(null, typeof(MonoScript)).image;
            var icon = AutoGizmo.Editor_GetIcon(script);
            if (icon == null || scriptIcon == icon || scriptIcon == null)
            {
                return;
            }

            Rect iconRect = new Rect(rect.x - 5, rect.y + 5, 32, rect.height - 5);
            GUI.DrawTexture(iconRect, scriptIcon, ScaleMode.ScaleToFit, true, (float)scriptIcon.width / scriptIcon.height);
        }

        private static void ProjectWindowItemOnGUI_Prefabs(Rect rect, string assetName)
        {
            if (!assetName.EndsWith(".prefab", StringComparison.CurrentCultureIgnoreCase))
            {
                return;
            }

            var go = AssetDatabase.LoadAssetAtPath<GameObject>(assetName);
            if (go == null)
            {
                return;
            }

            var texture = AutoGizmo.Editor_GetIcon(go);
            if (texture == null)
            {
                return;
            }

            Rect iconRect = new Rect(rect.x-5, rect.y+5, 30, rect.height-5);
            GUI.DrawTexture(iconRect, texture, ScaleMode.ScaleToFit, true, (float)texture.width/texture.height);
        }
    }
}
#endif