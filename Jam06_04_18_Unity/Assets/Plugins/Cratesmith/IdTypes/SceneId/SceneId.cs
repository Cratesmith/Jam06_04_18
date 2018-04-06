// Cratesmith 2017
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class SceneId 
#if UNITY_EDITOR
: ISerializationCallbackReceiver
#endif
{
	public Scene scene { get { return SceneManager.GetSceneByPath(path); }}
	public string path { get { return m_scenePath; } } 
	public string fullName { get { return m_sceneFullName; } }
	public string name { get { return m_sceneName; } }
	
	[SerializeField] Object editorSceneObject;
	[SerializeField] string m_sceneFullName;
	[SerializeField] string m_scenePath;
	[SerializeField] string m_sceneName;

	public override string ToString()
	{
		return fullName;
	}
		
	public static implicit operator string(SceneId source)
	{
		return source.ToString();
	}

#if UNITY_EDITOR
	public void OnBeforeSerialize()
	{		
		if (editorSceneObject != null)
		{
			var assetPath = AssetDatabase.GetAssetPath(editorSceneObject).Substring("Assets/".Length);	
			m_sceneFullName = Path.GetDirectoryName(assetPath) + "/" + Path.GetFileNameWithoutExtension(assetPath);
			m_sceneName = editorSceneObject.name;
			m_scenePath = "Assets/" + m_sceneFullName + ".unity";
		}
		else
		{
			m_sceneFullName = m_scenePath = m_sceneName = "";
		}
	}

	public void OnAfterDeserialize()
	{
	}

	[CustomPropertyDrawer(typeof(SceneId))]
	[CanEditMultipleObjects]
	public class Drawer : UnityEditor.PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var objectProp = property.FindPropertyRelative("editorSceneObject");
			EditorGUI.BeginChangeCheck();
			UnityEditor.EditorGUI.ObjectField(position, objectProp, typeof(SceneAsset), label);
			if (EditorGUI.EndChangeCheck())
			{
				var toPath = AssetDatabase.GetAssetPath(objectProp.objectReferenceValue);
				foreach (var target in property.serializedObject.targetObjects)
				{
					var fromPath = AssetDatabase.GetAssetPath(target);
					LinkedAssetTable.AddLink(fromPath,toPath,false);                    
				}               
				LinkedAssetTable.SaveToDisk();
			}
		}
	}
#endif
}
