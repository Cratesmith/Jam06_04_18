using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class LayerId
{
	public int value;
	
	public static implicit operator int(LayerId source)
	{
		return source.value;
	}
	
	public static implicit operator LayerId(int source)
	{
		return new LayerId() { value = source };
	}

#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(LayerId))]
	public class LayerIdDrawer : PropertyDrawer 
	{
		public override void OnGUI (Rect position, SerializedProperty prop, GUIContent label) 
		{
			EditorGUI.BeginProperty (position, label, prop);
		
			var valueProp = prop.FindPropertyRelative("value");
			valueProp.intValue = EditorGUI.LayerField(position, label, valueProp.intValue);
		
			EditorGUI.EndProperty();
		}
	}
#endif
}
