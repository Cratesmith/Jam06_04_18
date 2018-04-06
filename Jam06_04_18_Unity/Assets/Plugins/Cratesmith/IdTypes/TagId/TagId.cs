// Cratesmith 2017
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

[System.Serializable]
public class TagId
{
	public string value;

	public override string ToString()
	{
		return value;
	}

	public static implicit operator string(TagId source)
	{
		return source.ToString();
	}

	public static implicit operator TagId(string source)
	{
		return new TagId {value = source};
	}

#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(TagId))]
	public class Drawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, prop);

			var valueProp = prop.FindPropertyRelative("value");

			valueProp.stringValue = EditorGUI.TagField(position, label, valueProp.stringValue);

			EditorGUI.EndProperty();
		}
	}
#endif
}