using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 专用于角色等场景：按 key 缓存子层级中的 <see cref="SpriteRenderer"/>，用法类似 <see cref="ReferenceCollector"/>。
/// </summary>
[Serializable]
public class ReferenceSpriteCollectorData
{
	public string key;
	public SpriteRenderer spriteRenderer;
}

public class ReferenceSpriteCollectorDataComparer : IComparer<ReferenceSpriteCollectorData>
{
	public int Compare(ReferenceSpriteCollectorData x, ReferenceSpriteCollectorData y)
	{
		return string.Compare(x.key, y.key, StringComparison.Ordinal);
	}
}

public class ReferenceSpriteCollector : MonoBehaviour, ISerializationCallbackReceiver
{
	public List<ReferenceSpriteCollectorData> data = new List<ReferenceSpriteCollectorData>();
	private readonly Dictionary<string, SpriteRenderer> dict = new Dictionary<string, SpriteRenderer>();

#if UNITY_EDITOR
	public void Add(string key, SpriteRenderer spriteRenderer)
	{
		UnityEditor.SerializedObject serializedObject = new UnityEditor.SerializedObject(this);
		UnityEditor.SerializedProperty dataProperty = serializedObject.FindProperty("data");
		int i;
		for (i = 0; i < data.Count; i++)
		{
			if (data[i].key == key)
			{
				break;
			}
		}

		if (i != data.Count)
		{
			UnityEditor.SerializedProperty element = dataProperty.GetArrayElementAtIndex(i);
			element.FindPropertyRelative("spriteRenderer").objectReferenceValue = spriteRenderer;
		}
		else
		{
			dataProperty.InsertArrayElementAtIndex(i);
			UnityEditor.SerializedProperty element = dataProperty.GetArrayElementAtIndex(i);
			element.FindPropertyRelative("key").stringValue = key;
			element.FindPropertyRelative("spriteRenderer").objectReferenceValue = spriteRenderer;
		}

		UnityEditor.EditorUtility.SetDirty(this);
		serializedObject.ApplyModifiedProperties();
		serializedObject.UpdateIfRequiredOrScript();
	}

	public void Remove(string key)
	{
		UnityEditor.SerializedObject serializedObject = new UnityEditor.SerializedObject(this);
		UnityEditor.SerializedProperty dataProperty = serializedObject.FindProperty("data");
		int i;
		for (i = 0; i < data.Count; i++)
		{
			if (data[i].key == key)
			{
				break;
			}
		}

		if (i != data.Count)
		{
			dataProperty.DeleteArrayElementAtIndex(i);
		}

		UnityEditor.EditorUtility.SetDirty(this);
		serializedObject.ApplyModifiedProperties();
		serializedObject.UpdateIfRequiredOrScript();
	}

	public void Clear()
	{
		UnityEditor.SerializedObject serializedObject = new UnityEditor.SerializedObject(this);
		var dataProperty = serializedObject.FindProperty("data");
		dataProperty.ClearArray();
		UnityEditor.EditorUtility.SetDirty(this);
		serializedObject.ApplyModifiedProperties();
		serializedObject.UpdateIfRequiredOrScript();
	}

	public void Sort()
	{
		UnityEditor.SerializedObject serializedObject = new UnityEditor.SerializedObject(this);
		data.Sort(new ReferenceSpriteCollectorDataComparer());
		UnityEditor.EditorUtility.SetDirty(this);
		serializedObject.ApplyModifiedProperties();
		serializedObject.UpdateIfRequiredOrScript();
	}
#endif

	public SpriteRenderer Get(string key)
	{
		return dict.TryGetValue(key, out SpriteRenderer sr) ? sr : null;
	}

	public T Get<T>(string key) where T : class
	{
		return Get(key) as T;
	}

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
		dict.Clear();
		foreach (ReferenceSpriteCollectorData row in data)
		{
			if (string.IsNullOrEmpty(row.key) || row.spriteRenderer == null)
			{
				continue;
			}

			if (!dict.ContainsKey(row.key))
			{
				dict.Add(row.key, row.spriteRenderer);
			}
		}
	}
}
