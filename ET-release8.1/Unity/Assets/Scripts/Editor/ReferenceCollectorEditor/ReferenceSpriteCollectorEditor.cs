using System;
using System.Collections.Generic;
using System.Text;
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
		if (GUILayout.Button("角色绑点收集"))
		{
			BindSpriteRendererNodes();
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

	/// <summary>
	/// 收集：激活且可见、已赋 sprite 的 SpriteRenderer。
	/// 仅当物体名与白名单某 key 完全一致（Ordinal）时才写入该 key；引用为 SpriteRenderer 本身。
	/// </summary>
	private void BindSpriteRendererNodes()
	{
		HashSet<string> keyAllowlist = AvatarBindKeyAllowlistUtility.ParseToSet();
		if (keyAllowlist.Count == 0)
		{
			EditorUtility.DisplayDialog("ReferenceSpriteCollector",
				"请配置角色绑点 Key 白名单：在 Project 中创建或使用 " + AvatarBindKeyAllowlistUtility.DefaultAssetPath +
				"（菜单 Create → ET → 角色绑点 Key 白名单，或在「预制体处理工具」中点「创建资源」），填写 keys 后点「保存到资源」。",
				"确定");
			return;
		}

		Undo.RecordObject(referenceSpriteCollector, "ReferenceSpriteCollector 角色绑定");
		var spriteRenderers = referenceSpriteCollector.GetComponentsInChildren<SpriteRenderer>(false);
		var usedKeys = new HashSet<string>(StringComparer.Ordinal);
		foreach (var item in referenceSpriteCollector.data)
		{
			if (!string.IsNullOrEmpty(item.key))
			{
				usedKeys.Add(item.key);
			}
		}

		foreach (var sr in spriteRenderers)
		{
			if (sr == null || !IsCollectableSpriteRenderer(sr))
			{
				continue;
			}

			GameObject go = sr.gameObject;
			string resolvedKey = ResolveBindingKeyByExactName(go.name, keyAllowlist);
			if (string.IsNullOrEmpty(resolvedKey))
			{
				Debug.LogWarning($"[ReferenceSpriteCollector] 跳过「{go.name}」：物体名须与白名单某 key 完全一致。");
				continue;
			}

			if (usedKeys.Contains(resolvedKey))
			{
				Debug.LogWarning($"[ReferenceSpriteCollector] 跳过「{go.name}」：key「{resolvedKey}」已占用。");
				continue;
			}

			usedKeys.Add(resolvedKey);
			referenceSpriteCollector.Add(resolvedKey, sr);
		}

		referenceSpriteCollector.Sort();
		EditorUtility.SetDirty(referenceSpriteCollector);

		var keysSb = new StringBuilder();
		for (int i = 0; i < referenceSpriteCollector.data.Count; i++)
		{
			string k = referenceSpriteCollector.data[i]?.key;
			if (string.IsNullOrEmpty(k))
			{
				continue;
			}

			if (keysSb.Length > 0)
			{
				keysSb.Append(", ");
			}

			keysSb.Append(k);
		}

		Debug.Log(keysSb.ToString());
	}

	private static string ResolveBindingKeyByExactName(string gameObjectName, HashSet<string> allowlist)
	{
		if (string.IsNullOrEmpty(gameObjectName) || allowlist == null || allowlist.Count == 0)
		{
			return null;
		}

		return allowlist.Contains(gameObjectName) ? gameObjectName : null;
	}

	private static bool IsCollectableSpriteRenderer(SpriteRenderer sr)
	{
		return sr.enabled && sr.sprite != null;
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
