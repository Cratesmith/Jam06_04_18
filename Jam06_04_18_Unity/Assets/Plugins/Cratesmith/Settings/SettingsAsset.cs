using System;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

/*
 Suggested usage:
 public class MySettings : SettingsAsset<MySettings>
 {
		public int		myIntParam = 3;
		public Actor	myActorPrefabParam;
		
		[System.Serializable]
		public class Reference : SettingsReference<MySettings> {}
 }
*/ 


/// <summary>
/// A base type for settings asset classes for use with SettingsReference objects
/// These are ScriptableObjects that create a default instance in the a Resources folder.
/// </summary>
/// 
public abstract class SettingsAsset<TSelfType> : SettingsAssetBase where TSelfType:SettingsAsset<TSelfType>
{
	private static TSelfType s_instance;
	
	public static TSelfType defaultInstance
	{
		get 
		{ 
			LoadAsset();
#if UNITY_EDITOR
			if (s_instance == null)
			{
				BuildDefaultAsset(typeof(TSelfType));
			}
#endif

			if (s_instance == null)
			{
				Debug.LogWarningFormat("Couldn't load asset for {0}, creating a temporary instance", typeof(TSelfType).Name);
				s_instance = CreateInstance<TSelfType>();
			}
			return s_instance; 
		} 
	}

	private static string GetAssetResourceName()
	{
		return GetAssetResourceName(typeof(TSelfType));
	}

	private static string GetAssetFileName()
	{
		return GetAssetFileName(typeof(TSelfType));
	}
	
	static void LoadAsset()
	{
		if(Application.isPlaying)
		{
			if(!s_instance)
			{				
				s_instance = Resources.Load(GetAssetResourceName()) as TSelfType;
                Assert.IsNotNull(s_instance, "Couldn't load default resource for type "+typeof(TSelfType).Name);
			}
		}

#if UNITY_EDITOR
		if(!s_instance) 
		{
			var temp = CreateInstance<TSelfType>();
			var monoscript  = MonoScript.FromScriptableObject(temp);
			DestroyImmediate(temp);
			if (monoscript != null)
			{
				var scriptPath  = AssetDatabase.GetAssetPath(monoscript);
				var assetDir    = Path.GetDirectoryName(scriptPath)+"/Resources/";
				Directory.CreateDirectory(assetDir);
				var assetPath   = assetDir+GetAssetFileName();
				s_instance = AssetDatabase.LoadAssetAtPath<TSelfType>(assetPath);				
			}
			else
			{
				Debug.LogWarningFormat("SettingsAsset: Could not find script for {0} (expecting to find it in a file named {0}.cs)", typeof(TSelfType).Name);
			}
		}
#endif
	}
}

public abstract class SettingsAssetBase : ScriptableObject
{
	protected static string GetAssetResourceName(System.Type type)
	{
		return type.Name + ".default";
	}

	protected static string GetAssetFileName(System.Type type)
	{
		return type.Name + ".default.asset";
	}

#if UNITY_EDITOR
	[UnityEditor.Callbacks.DidReloadScripts]
	public static void OnDidReloadScripts()
	{
		AssetDatabase.StartAssetEditing();
		foreach (var script in MonoImporter.GetAllRuntimeMonoScripts())
		{
			BuildDefaultAsset(script.GetClass());
		}
		AssetDatabase.StopAssetEditing();
		AssetDatabase.SaveAssets();
	}
	
	protected static void BuildDefaultAsset(Type scriptClass)
	{
		if (scriptClass == null || scriptClass.IsAbstract || !typeof(SettingsAssetBase).IsAssignableFrom(scriptClass))
		{
			return;
		}

		var temp = ScriptableObject.CreateInstance(scriptClass);
		var script = MonoScript.FromScriptableObject(temp);
		if (script == null)
		{
			Debug.LogWarningFormat("SettingsAsset: Could not find script for {0} (expecting to find it in a file named {0}.cs)", scriptClass.Name);
			return;
		}

		DestroyImmediate(temp);

		var scriptPath = AssetDatabase.GetAssetPath(script);
		var assetDir = Path.GetDirectoryName(scriptPath) + "/Resources/";
		Directory.CreateDirectory(assetDir);

		var assetPath = assetDir + GetAssetFileName(scriptClass);
		if (!File.Exists(assetPath))
		{
			var instance = CreateInstance(scriptClass);
			AssetDatabase.CreateAsset(instance, assetPath);
		}
	}

	public static Object GetDefaultAsset(Type scriptClass)
	{
		if (scriptClass == null || scriptClass.IsAbstract || !typeof(SettingsAssetBase).IsAssignableFrom(scriptClass))
		{
			return null;
		}

		var temp = ScriptableObject.CreateInstance(scriptClass);
		var script = MonoScript.FromScriptableObject(temp);
		if (script == null)
		{
			Debug.LogWarningFormat("SettingsAsset: Could not find script for {0} (expecting to find it in a file named {0}.cs)", scriptClass.Name);
			return null;
		}

		DestroyImmediate(temp);

		var scriptPath = AssetDatabase.GetAssetPath(script);
		var assetDir = Path.GetDirectoryName(scriptPath) + "/Resources/";
		var assetPath = assetDir + GetAssetFileName(scriptClass);

		return AssetDatabase.LoadAssetAtPath(assetPath, scriptClass);
	}
#endif
}
