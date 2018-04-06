using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

public class PopupEditorWindow : EditorWindow
{
	[SerializeField] private Editor m_editor;
	[SerializeField] private List<Editor> m_subEditors = new List<Editor>();
	[SerializeField] private Vector2 m_scrollPosition = Vector2.zero;

	private const string MENUITEM_STRING = "Window/Create Popup Inspector...";

	[MenuItem(MENUITEM_STRING, true)]
	public static bool _PopupEditorWindowMenuItem()
	{
		return Selection.activeObject != null;
	}

	[MenuItem(MENUITEM_STRING)]
	public static void PopupEditorWindowMenuItem()
	{
		Create(Selection.activeObject, new Rect(50, 50, 600, 500));
	}

	public static PopupEditorWindow Create(Object obj, Rect rect)
	{
		var window = CreateInstance<PopupEditorWindow>();
		window.Init(obj, rect);
		return window;
	}

	private void Init(Object obj, Rect rect)
	{
		m_editor = Editor.CreateEditor(obj);

		var gameObject = obj as GameObject;
		if (gameObject)
		{
			foreach (var component in gameObject.GetComponents<Component>())
			{
				m_subEditors.Add(Editor.CreateEditor(component));
			}
		}

		titleContent = new GUIContent(obj.name);
		Show();
		position	= rect;
		minSize		= rect.size;
		Focus();
	}

	void OnGUI()
	{
		if (m_editor==null)
		{
			return;		
		}

		m_scrollPosition = GUILayout.BeginScrollView(m_scrollPosition, "button");
		OnGUI_DrawEditor(m_editor, true, false);
		foreach (var editor in m_subEditors)
		{
			OnGUI_DrawEditor(editor, false, true);
		}
		GUILayout.EndScrollView();
        

		if (GUI.Button(new Rect(0, 0, 27, 27), "", "TL SelectionBarCloseButton"))
		{
			Close();
		}

	}


	private void OnGUI_DrawEditor(Editor editor, bool drawHeader, bool isExpandable)
	{
		if (editor.targets.Length == 0)
		{
			return;		
		}

		bool wideMode	= EditorGUIUtility.wideMode;
		var labelWidth	= EditorGUIUtility.labelWidth;
		var fieldWidth	= EditorGUIUtility.fieldWidth;

		EditorGUIUtility.wideMode = true;
		EditorGUIUtility.labelWidth = 0;
		EditorGUIUtility.fieldWidth = 0;

		if (drawHeader)
		{
			editor.DrawHeader();
		}

		bool drawEditor = !isExpandable;
		if (isExpandable)
		{
			var prevExpanded = false;
			foreach (var target in editor.targets)
			{
				if (UnityEditorInternal.InternalEditorUtility.GetIsInspectorExpanded(target))
				{
					prevExpanded = true;
					break;
				}
			}

			var expanded = EditorGUILayout.InspectorTitlebar(prevExpanded, editor.targets);		
			if (expanded != prevExpanded)
			{
				foreach (var target in editor.targets)
				{
					UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(target, expanded);
				}
			}

			drawEditor = expanded;
		}

		if (drawEditor)
		{
			editor.OnInspectorGUI();            
		}
		
		EditorGUIUtility.labelWidth = labelWidth;
		EditorGUIUtility.fieldWidth = fieldWidth;
		EditorGUIUtility.wideMode	= wideMode;
	}
}
#endif