using System;
using System.Collections.Generic;
using ET;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ReferenceSpriteCollector))]
public class ReferenceSpriteCollectorEditor : Editor
{
	private string searchKey
	{
		get => _searchKey;
		set
		{
			if (_searchKey != value)
			{
				_searchKey = value;
				previewSprite = referenceSpriteCollector.Get(value);
			}
		}
	}

	private ReferenceSpriteCollector referenceSpriteCollector;
	private SpriteRenderer previewSprite;
	private string _searchKey = string.Empty;

	private void DelNullReference()
	{
		var dataProperty = serializedObject.FindProperty("data");
		for (int i = dataProperty.arraySize - 1; i >= 0; i--)
		{
			var prop = dataProperty.GetArrayElementAtIndex(i).FindPropertyRelative("spriteRenderer");
			if (prop.objectReferenceValue == null)
			{
				dataProperty.DeleteArrayElementAtIndex(i);
				EditorUtility.SetDirty(referenceSpriteCollector);
				serializedObject.ApplyModifiedProperties();
				serializedObject.UpdateIfRequiredOrScript();
			}
		}
	}

	private void OnEnable()
	{
		referenceSpriteCollector = (ReferenceSpriteCollector)target;
	}

	public override void OnInspectorGUI()
	{
		Undo.RecordObject(referenceSpriteCollector, "ReferenceSpriteCollector");
		serializedObject.Update();
		var dataProperty = serializedObject.FindProperty("data");

		GUILayout.BeginHorizontal();
		if (GUILayout.Button("添加引用"))
		{
			AddReference(dataProperty, Guid.NewGuid().GetHashCode().ToString(), null);
		}

		if (GUILayout.Button("全部删除"))
		{
			referenceSpriteCollector.Clear();
		}

		if (GUILayout.Button("删除空引用"))
		{
			DelNullReference();
		}

		if (GUILayout.Button("排序"))
		{
			referenceSpriteCollector.Sort();
		}

		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		searchKey = EditorGUILayout.TextField(searchKey);
		EditorGUILayout.ObjectField(previewSprite, typeof(SpriteRenderer), false);
		if (GUILayout.Button("删除"))
		{
			referenceSpriteCollector.Remove(searchKey);
			previewSprite = null;
		}

		GUILayout.EndHorizontal();
		EditorGUILayout.Space();

		var delList = new List<int>();
		SerializedProperty property;
		for (int i = dataProperty.arraySize - 1; i >= 0; i--)
		{
			GUILayout.BeginHorizontal();
			property = dataProperty.GetArrayElementAtIndex(i).FindPropertyRelative("key");
			property.stringValue = EditorGUILayout.TextField(property.stringValue, GUILayout.Width(150));
			property = dataProperty.GetArrayElementAtIndex(i).FindPropertyRelative("spriteRenderer");
			property.objectReferenceValue = EditorGUILayout.ObjectField(property.objectReferenceValue, typeof(SpriteRenderer), true);
			if (GUILayout.Button("X"))
			{
				delList.Add(i);
			}

			GUILayout.EndHorizontal();
		}

		EventType eventType = Event.current.type;
		if (eventType == EventType.DragUpdated || eventType == EventType.DragPerform)
		{
			DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
			if (eventType == EventType.DragPerform)
			{
				DragAndDrop.AcceptDrag();
				foreach (UnityEngine.Object o in DragAndDrop.objectReferences)
				{
					SpriteRenderer sr = o as SpriteRenderer;
					if (sr == null && o is GameObject go)
					{
						sr = go.GetComponent<SpriteRenderer>();
					}

					if (sr != null)
					{
						AddReference(dataProperty, sr.gameObject.name, sr);
					}
				}
			}

			Event.current.Use();
		}

		foreach (int i in delList)
		{
			dataProperty.DeleteArrayElementAtIndex(i);
		}

		serializedObject.ApplyModifiedProperties();
		serializedObject.UpdateIfRequiredOrScript();
	}

	private void AddReference(SerializedProperty dataProperty, string key, SpriteRenderer spriteRenderer)
	{
		int index = dataProperty.arraySize;
		dataProperty.InsertArrayElementAtIndex(index);
		var element = dataProperty.GetArrayElementAtIndex(index);
		element.FindPropertyRelative("key").stringValue = key;
		element.FindPropertyRelative("spriteRenderer").objectReferenceValue = spriteRenderer;
	}
}
