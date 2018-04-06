#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using Object = UnityEngine.Object;

using ListType = System.Collections.Generic.HashSet<string>;
using TableType = System.Collections.Generic.Dictionary<string, System.Collections.Generic.HashSet<string>>;

[InitializeOnLoad]
public static class LinkedAssetTable  
{
	static TableType table = new TableType();

	public static string tablePath { get { return "Assets/Plugins/Cratesmith/IdTypes/Editor/LinkedAssetTable.json"; } }

	static LinkedAssetTable()
	{
		LoadFromDisk();
	}

	public static void AddLink(string fromAssetPath, string toAssetPath, bool saveToDisk=true)
	{
		if (string.IsNullOrEmpty(fromAssetPath) || string.IsNullOrEmpty(toAssetPath))
		{
			return;
		}

		var fromGuid = AssetDatabase.AssetPathToGUID(fromAssetPath);
		var toGuid = AssetDatabase.AssetPathToGUID(toAssetPath);
        if(fromGuid==toGuid)
        {
            return; // we don't store cyclic links. There's no point
        }

		ListType list = null;
		if (!table.TryGetValue(toGuid, out list))
		{
			list = table[toGuid] = new ListType();
		}
		list.Add(fromGuid);

		if(saveToDisk) SaveToDisk();
	}

	public static void RemoveLink(string fromAssetPath, string toAssetPath, bool saveToDisk=true)
	{
		if (string.IsNullOrEmpty(fromAssetPath) || string.IsNullOrEmpty(toAssetPath))
		{
			return;
		}

		var fromGuid = AssetDatabase.AssetPathToGUID(fromAssetPath);
		var toGuid = AssetDatabase.AssetPathToGUID(toAssetPath);

		ListType list = null;
		if (table.TryGetValue(toGuid, out list))
		{
			list.Remove(fromGuid);
		}

		if(saveToDisk) SaveToDisk();
	}

	public static void RemoveAllLinksTo(string toAssetPath, bool saveToDisk=true)
	{
		if (string.IsNullOrEmpty(toAssetPath))
		{
			return;
		}

		var toGuid = AssetDatabase.AssetPathToGUID(toAssetPath);
		table.Remove(toGuid);

		if(saveToDisk) SaveToDisk();
	}

	public static void SaveToDisk()
	{
		var dirPath = Path.GetDirectoryName(tablePath);
		if (!Directory.Exists(dirPath))
		{
			Directory.CreateDirectory(dirPath);
		}

		try
		{
			File.WriteAllText(tablePath, JsonConvert.SerializeObject(table));
		}
		catch (Exception e)
		{
			Debug.LogError("Couldn't save LinkedAssetTable. "+e);
		}
	}

	private static void LoadFromDisk()
	{
		if (!File.Exists(tablePath))
		{
			return;
		}

		try
		{
			table = JsonConvert.DeserializeObject<TableType>(File.ReadAllText(tablePath));
		}
		catch (Exception e)
		{
			Debug.LogError("Couldn't load LinkedAssetTable. "+e);
		}
	}

	private static void MarkLinkedAssetsDirty(string fromAssetPath)
	{
		if (string.IsNullOrEmpty(fromAssetPath))
		{
			return;
		}

		var fromGuid = AssetDatabase.AssetPathToGUID(fromAssetPath);
		
		ListType list = null;
		if (!table.TryGetValue(fromGuid, out list))
		{
			return;
		}

		foreach (var toGuid in list)
		{
            if (toGuid == fromGuid) 
            { 
                continue; 
            }

			var toAssetPath = AssetDatabase.GUIDToAssetPath(toGuid);
			foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(toAssetPath))
			{					
				EditorUtility.SetDirty(asset);
			}				
			AssetDatabase.SaveAssets();
		}
	}

	public class Processor : UnityEditor.AssetPostprocessor
	{
		public override int GetPostprocessOrder()
		{
			return int.MaxValue;
		}

		static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
			string[] movedFromAssetPaths)
		{
			foreach (var movedAsset in movedAssets)
			{
				MarkLinkedAssetsDirty(movedAsset);
			}	
			
			foreach (var deletedAsset in deletedAssets)
			{
				MarkLinkedAssetsDirty(deletedAsset);
				RemoveAllLinksTo(deletedAsset);
			}
			SaveToDisk();
		}	
	}	
}
#endif