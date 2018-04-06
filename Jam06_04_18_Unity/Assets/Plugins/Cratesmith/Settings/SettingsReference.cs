using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
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
/// A base for parameter types that reference SettingsReference assets. It provides the SettingsAsset's default instance if no override has been set.
/// Note: You must create a subclass of SettingsReference with the [System.Serializable] attribute for it to appear in the Unity Inspector. (see suggested usage)
/// </summary>
public abstract class SettingsReference<TSettingsType> : SettingsReferenceBase where TSettingsType : SettingsAsset<TSettingsType>
{
	public TSettingsType overrideValue;

	public TSettingsType defaultValue { get {return SettingsAsset<TSettingsType>.defaultInstance; }}

	public TSettingsType value
	{
		get
		{
			if (overrideValue != null)
			{
				return overrideValue;
			}

			return defaultValue;
		}
	}
}

public abstract class SettingsReferenceBase
{	
#if UNITY_EDITOR
	public static Object GetDefaultAssetForReferenceType(System.Type type)
	{
		if (type.IsAbstract)
		{
			return null;
		}

		if (typeof(SettingsReferenceBase).IsAssignableFrom(type))
		{
			while (type != null)
			{
				if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(SettingsReference<>))
				{
					var assetType = type.GetGenericArguments()[0]; // will be derived from SettingsAsset<T> 
					var property = assetType.GetProperty("defaultInstance",
						BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
					return property.GetValue(null, null) as Object;
				}
				type = type.BaseType;			
			}
		}
		
		return null;
	}
#endif	
}
