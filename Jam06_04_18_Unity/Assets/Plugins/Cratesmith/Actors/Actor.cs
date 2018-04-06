
using System;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// A base type for scripts that are unique entities and own all the gameObjects and components beneath them
/// </summary>
[SelectionBase]
public class Actor : MonoBehaviour
{
    protected virtual void OnDrawGizmos()
    {
        #if UNITY_EDITOR
        Editor_Apply_Icon();
        #endif
    }

    #if UNITY_EDITOR
    public void Editor_Apply_Icon()
    {
        Texture2D icon = Editor_GetIcon(this);
        Texture2D objectIcon = Editor_GetIcon(gameObject);

        if (objectIcon != icon)
        {
            Editor_SetIcon(gameObject, icon);
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
    #endif

    #if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
    class HeirachyDrawer
    {
        private static HeirachyDrawer m_instance;

        static HeirachyDrawer()
        {
            m_instance = new HeirachyDrawer();
            EditorApplication.hierarchyWindowItemOnGUI += m_instance.HierarchyWindowListElementOnGUI;
            EditorApplication.projectWindowItemOnGUI += m_instance.ProjectWindowItemOnGUI;
            EditorApplication.RepaintHierarchyWindow();
        }

        private void ProjectWindowItemOnGUI(string guid, Rect rect)
        {                
            var assetName = AssetDatabase.GUIDToAssetPath(guid);
            if (!assetName.EndsWith(".prefab", StringComparison.CurrentCultureIgnoreCase))
            {
                return;
            }

            var go = AssetDatabase.LoadAssetAtPath<GameObject>(assetName);
            if (go == null)
            {
                return;
            }

            var actor = go.GetComponent<Actor>();
            if (actor == null)
            {
                return;
            }

            actor.Editor_Apply_Icon();                
        }

        private void HierarchyWindowListElementOnGUI(int instanceid, Rect rect)
        {          
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

            var actor = go.GetComponent<Actor>();
            if (actor == null)
            {
                return;
            }

            actor.Editor_Apply_Icon();                
        }
    }
    #endif
}