using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using Type = System.Type;
using UnityEngine.Assertions;

#if UNITY_EDITOR
using System.Linq;
#endif

/// <summary>
/// Cache for ComponentDependencyAttribute so GetAttributes doesn't need to be called on each type 
/// at runtime (potential GC Alloc and performance spikes)
/// </summary>
public class ScriptExecutionOrderCache : ResourceSingleton<ScriptExecutionOrderCache>
, ISerializationCallbackReceiver
{   
    [System.Serializable]
    public struct SerializedItem 
    {
        public string typeName;
        public int executionOrder;
    }  
    /// <summary>
    /// Serialized version of dependency table to be loaded at runtime.
    /// </summary>
    [SerializeField] List<SerializedItem> m_serializedItems = new List<SerializedItem>();

    /// <summary>
    /// Dependencies table for all types using ComponentDepenencyAttribute
    /// </summary>
    Dictionary<Type, int> m_executionOrder = new Dictionary<Type, int>();


    public static int GetExecutionOrder(Type forType)
    {
        int output = 0;
        instance.m_executionOrder.TryGetValue(forType, out output);
        return output;
    }
    
    #if UNITY_EDITOR
    [UnityEditor.Callbacks.DidReloadScripts(CallbackOrder.SCRIPT_EXECUTION_ORDER_CACHE)]
    static void ProcessScripts()
    {
        if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
        {
	        if (rawInstance != null)
	        {
		        ProcessDependencies();		        
	        }
	        else
	        {
		        Debug.LogWarning("ScrpitExecutionOrderCache delaying processing until unity allows existing assets to load");
		        UnityEditor.EditorApplication.delayCall += ProcessDependencies;
	        }
        }  
    }

    private static void ProcessDependencies()
    { 
        ResourceSingletonBuilder.BuildResourceSingletonsIfDirty();

        var so = new UnityEditor.SerializedObject(instance);

        var types = new string[] { ".cs", ".js" };

        var allScriptPaths = Directory.GetFiles("Assets", "*.*", SearchOption.AllDirectories)
            .Where(s => types.Any(x => s.EndsWith(x, System.StringComparison.CurrentCultureIgnoreCase)))
            .ToArray();

        instance.m_serializedItems.Clear();

        for (int i = 0; i < allScriptPaths.Length; ++i)
        {
            UnityEditor.MonoScript script = UnityEditor.AssetDatabase.LoadAssetAtPath(allScriptPaths[i], typeof(UnityEditor.MonoScript)) as UnityEditor.MonoScript;

            if (!script || script.GetClass() == null) continue;

            var type = script.GetClass();
            if (!typeof(Component).IsAssignableFrom(script.GetClass())
                && !typeof(ScriptableObject).IsAssignableFrom(script.GetClass()))
            {
                continue;
            }

            var typeExecutionOrder = UnityEditor.MonoImporter.GetExecutionOrder(script);
            if (typeExecutionOrder == 0)
            {
                continue;
            }

            instance.m_serializedItems.Add(new SerializedItem()
            {
                typeName = type.FullName,
                executionOrder = typeExecutionOrder,
            });
        }

        so.Update();
        instance.hideFlags = HideFlags.NotEditable;
        UnityEditor.EditorUtility.SetDirty(instance);
        UnityEditor.AssetDatabase.Refresh();      
    }
    #endif

    #region ISerializationCallbackReceiver implementation
    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
    }

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
        m_executionOrder.Clear();
        for(int i=0;i<m_serializedItems.Count;++i)
        {
            var item = m_serializedItems[i];
            if(string.IsNullOrEmpty(item.typeName)) continue;

            var forType = GetType(item.typeName);
            if(forType==null)
            {
                continue; 
            }

            m_executionOrder[forType] = item.executionOrder;
        }
    }    
#endregion
    static System.Type GetType(string name)
    { 
        System.Type type = null;
        var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
        for (int i = 0; i < assemblies.Length; i++)
        {
            type = assemblies[i].GetType(name);
            if (type != null) break;
        }
        return type;
    }
}
