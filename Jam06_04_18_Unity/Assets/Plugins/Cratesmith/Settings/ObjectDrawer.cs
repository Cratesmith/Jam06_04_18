using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(UnityEngine.Object), true)]
public class ObjectDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		if (property.objectReferenceValue != null && !property.hasMultipleDifferentValues)
		{			
			var buttonRect = new Rect(position.x+(EditorGUI.indentLevel-1)*position.height+2, position.y, position.height-4, position.height);
			if (GUI.Button(buttonRect, "", "OL Plus"))
			{
				var windowRect = new Rect(GUIUtility.GUIToScreenPoint(position.position), new Vector2(500, 500));
				PopupEditorWindow.Create(property.objectReferenceValue, windowRect);
			}
		}
		EditorGUI.PropertyField(position, property, label);
	}
}
#endif