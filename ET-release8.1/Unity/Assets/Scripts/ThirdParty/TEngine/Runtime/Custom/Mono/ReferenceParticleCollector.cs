using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 专用于特效等场景：按 key 缓存子层级中的 <see cref="ParticleSystem"/>，用法类似 <see cref="ReferenceCollector"/>。
/// </summary>
[Serializable]
public class ReferenceParticleCollectorData
{
	public string key;
	public ParticleSystem particleSystem;
}

public class ReferenceParticleCollectorDataComparer : IComparer<ReferenceParticleCollectorData>
{
	public int Compare(ReferenceParticleCollectorData x, ReferenceParticleCollectorData y)
	{
		return string.Compare(x.key, y.key, StringComparison.Ordinal);
	}
}

public class ReferenceParticleCollector : MonoBehaviour, ISerializationCallbackReceiver
{
	public List<ReferenceParticleCollectorData> data = new List<ReferenceParticleCollectorData>();
	private readonly Dictionary<string, ParticleSystem> dict = new Dictionary<string, ParticleSystem>();

#if UNITY_EDITOR
	public void Add(string key, ParticleSystem particleSystem)
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
			element.FindPropertyRelative("particleSystem").objectReferenceValue = particleSystem;
		}
		else
		{
			dataProperty.InsertArrayElementAtIndex(i);
			UnityEditor.SerializedProperty element = dataProperty.GetArrayElementAtIndex(i);
			element.FindPropertyRelative("key").stringValue = key;
			element.FindPropertyRelative("particleSystem").objectReferenceValue = particleSystem;
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
		data.Sort(new ReferenceParticleCollectorDataComparer());
		UnityEditor.EditorUtility.SetDirty(this);
		serializedObject.ApplyModifiedProperties();
		serializedObject.UpdateIfRequiredOrScript();
	}
#endif

	public ParticleSystem Get(string key)
	{
		return dict.TryGetValue(key, out ParticleSystem ps) ? ps : null;
	}

	public T Get<T>(string key) where T : class
	{
		return Get(key) as T;
	}

	public void PlayAll()
	{
		foreach (ParticleSystem ps in dict.Values)
		{
			if (ps == null)
			{
				continue;
			}

			ps.Play(true);
		}
	}

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
		dict.Clear();
		foreach (ReferenceParticleCollectorData row in data)
		{
			if (string.IsNullOrEmpty(row.key) || row.particleSystem == null)
			{
				continue;
			}

			if (!dict.ContainsKey(row.key))
			{
				dict.Add(row.key, row.particleSystem);
			}
		}
	}
}
