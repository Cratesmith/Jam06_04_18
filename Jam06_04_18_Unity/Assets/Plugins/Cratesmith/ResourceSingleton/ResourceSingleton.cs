using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
#endif

[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public class ResourceFilenameAttribute : Attribute
{
    private string filename;
    private bool isSuffix;
    static Dictionary<Type,string> s_cache = new Dictionary<Type, string>();

    public ResourceFilenameAttribute(string filename, bool isSuffix=false)
    {
        this.filename = filename;
        this.isSuffix = isSuffix;
    } 

    public static string Get<T>()
    {        
        return Get(typeof(T));
    }

    public static string Get(Type type)
    {
        var attribs = (ResourceFilenameAttribute[])type.GetCustomAttributes(typeof(ResourceFilenameAttribute), true);
        string output;
        if (!s_cache.TryGetValue(type, out output))
        {
            if (attribs.Length > 0)
            {
                var attrib = attribs[0];
                output = !attrib.isSuffix
                    ? attrib.filename
                    : type.Name + attrib.filename;
            }
            else
            {
                output = type.Name;
            }

            s_cache[type] = output;
        }
              
        return output;
    }
}

public abstract class ResourceSingleton<T> : ScriptableObject where T:ScriptableObject
{
    static T s_instance;

    protected static T instance
    {
        get 
        { 
            LoadAsset();

            if(s_instance==null)
            {
                throw new ArgumentNullException("Couldn't load asset for ResourceSingleton "+typeof(T).Name);
            }

            return s_instance; 
        } 
    }

#if UNITY_EDITOR
	protected static T rawInstance
	{
		get 
		{ 
			LoadAsset();
			return s_instance; 
		} 
	}
#endif

	static void LoadAsset()
    {
        if(Application.isPlaying)
        {
            if(!s_instance)
            {
                s_instance = Resources.Load(ResourceFilenameAttribute.Get<T>()) as T;
            }
        }

        #if UNITY_EDITOR
        if(!s_instance) 
        {
            ResourceSingletonBuilder.BuildResourceSingletonsIfDirty(); // ensure that singletons were built

            var temp = CreateInstance<T>();
            var monoscript  = MonoScript.FromScriptableObject(temp);
            DestroyImmediate(temp);
            var scriptPath  = AssetDatabase.GetAssetPath(monoscript);
            var assetDir    = Path.GetDirectoryName(scriptPath).Replace("\\","/")+"/Resources/";
            Directory.CreateDirectory(assetDir);
            var assetPath   = assetDir+ResourceFilenameAttribute.Get<T>()+".asset";
            s_instance = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            
        }
        #endif
    }

    #if UNITY_EDITOR
    public virtual void OnRebuildInEditor()
    {                
    }
    #endif
 }

#region internal
#if UNITY_EDITOR
public class ResourceSingletonBuilder
{
    static bool s_hasRun;
   
    [DidReloadScripts(CallbackOrder.RESOURCE_SINGLETON)]
    public static void BuildResourceSingletonsIfDirty()
    {
        if(s_hasRun)
        {
            return; 
        } 
         
        BuildResourceSingletons();
    } 
        
    public static void BuildResourceSingletons()
    {
        var result = System.AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x=>x.GetTypes())
            .Where(t => !t.IsAbstract && GetBaseType(t, typeof(ResourceSingleton<>)));

        var method = typeof(ResourceSingletonBuilder).GetMethod("BuildOrMoveAsset", BindingFlags.NonPublic | BindingFlags.Static);
        if(method == null)
        {
            EditorApplication.delayCall += BuildResourceSingletons;
            return;
        }

        foreach(var i in result)
        {  
            var generic = method.MakeGenericMethod(i);
            generic.Invoke(null, new object[0]);
        }

        s_hasRun = true;
    }

    static bool GetBaseType(Type type, Type baseType)
    {
        if (type == null || baseType == null || type == baseType)
            return false;

        if (baseType.IsGenericType == false)
        {
            if (type.IsGenericType == false)
                return type.IsSubclassOf(baseType);
        }
        else
        {
            baseType = baseType.GetGenericTypeDefinition();
        }

        type = type.BaseType;
        Type objectType = typeof(object);
        while (type != objectType && type != null)
        {
            Type curentType = type.IsGenericType ?
                type.GetGenericTypeDefinition() : type;
            if (curentType == baseType)
                return true;

            type = type.BaseType;
        }
              
        return false;
    }

    static void BuildOrMoveAsset<T>() where T:ResourceSingleton<T>
    {
        var editorPrefsKey = "ResourceSingleton.PrevFilename." + typeof(T).FullName;
        var resourceFilename = ResourceFilenameAttribute.Get<T>();
        var instance = Resources.Load(resourceFilename) as T;
        var prevFilename = EditorPrefs.GetString(editorPrefsKey);
        if (instance == null && !string.IsNullOrEmpty(prevFilename))
        {
            instance = Resources.Load(prevFilename) as T;
        }
        EditorPrefs.SetString("ResourceSingleton.PrevFilename."+typeof(T).FullName, resourceFilename);

        var temp = ScriptableObject.CreateInstance<T>();
        var monoscript = MonoScript.FromScriptableObject(temp);
        ScriptableObject.DestroyImmediate(temp);
        if(monoscript==null)
        {
            Debug.LogError("Couldn't find script named "+typeof(T).Name+".cs (monoscripts must be in a file named the same as thier class)");
            return;
        }

        var scriptPath = AssetDatabase.GetAssetPath(monoscript);

        var assetDir = (Path.GetDirectoryName(scriptPath)+"/Resources/").Replace("\\","/");
        var assetPath  = assetDir+resourceFilename+".asset";
        Directory.CreateDirectory(assetDir);

        if(instance && AssetDatabase.GetAssetPath(instance)!=assetPath)
        {
            if (!File.Exists(assetPath))
            {
                Debug.Log("ResourceSingleton: Moving asset: " + typeof(T).Name + " from " +
                          AssetDatabase.GetAssetPath(instance) + " to " + assetPath);
                FileUtil.MoveFileOrDirectory(AssetDatabase.GetAssetPath(instance), assetPath);
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                AssetDatabase.ImportAsset(assetPath,ImportAssetOptions.ForceSynchronousImport);
                instance = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            }
            else
            {
                Debug.LogWarning("ResourceSingleton: Didn't move asset: " + typeof(T).Name + " from " +
                                 AssetDatabase.GetAssetPath(instance) + " to " + assetPath+ " as a file already exists there!");                    
            }
        }

        if(!instance && !File.Exists(assetPath))
        {
            Debug.Log("ResourceSingleton: Creating asset: " + typeof(T).Name + " at " + assetPath);
            instance = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(instance, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(assetPath,ImportAssetOptions.ForceSynchronousImport);
            instance = AssetDatabase.LoadAssetAtPath<T>(assetPath);
        }

        if (instance)
        {
            instance.OnRebuildInEditor();            
        }
    }
}
#endif
#endregion


