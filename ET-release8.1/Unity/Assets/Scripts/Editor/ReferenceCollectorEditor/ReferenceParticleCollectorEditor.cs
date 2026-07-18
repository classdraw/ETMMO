using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ReferenceParticleCollector))]
public class ReferenceParticleCollectorEditor : Editor
{
	private string searchKey
	{
		get => _searchKey;
		set
		{
			if (_searchKey != value)
			{
				_searchKey = value;
				previewParticleSystem = referenceParticleCollector.Get(value);
			}
		}
	}

	private ReferenceParticleCollector referenceParticleCollector;
	private ParticleSystem previewParticleSystem;
	private string _searchKey = string.Empty;

	private void DelNullReference()
	{
		var dataProperty = serializedObject.FindProperty("data");
		for (int i = dataProperty.arraySize - 1; i >= 0; i--)
		{
			var prop = dataProperty.GetArrayElementAtIndex(i).FindPropertyRelative("particleSystem");
			if (prop.objectReferenceValue == null)
			{
				dataProperty.DeleteArrayElementAtIndex(i);
				EditorUtility.SetDirty(referenceParticleCollector);
				serializedObject.ApplyModifiedProperties();
				serializedObject.UpdateIfRequiredOrScript();
			}
		}
	}

	private void OnEnable()
	{
		referenceParticleCollector = (ReferenceParticleCollector)target;
	}

	public override void OnInspectorGUI()
	{
		Undo.RecordObject(referenceParticleCollector, "ReferenceParticleCollector");
		serializedObject.Update();
		var dataProperty = serializedObject.FindProperty("data");

		GUILayout.BeginHorizontal();
		if (GUILayout.Button("添加引用"))
		{
			AddReference(dataProperty, Guid.NewGuid().GetHashCode().ToString(), null);
		}

		if (GUILayout.Button("全部删除"))
		{
			referenceParticleCollector.Clear();
		}

		if (GUILayout.Button("删除空引用"))
		{
			DelNullReference();
		}

		if (GUILayout.Button("排序"))
		{
			referenceParticleCollector.Sort();
		}

		if (GUILayout.Button("收集子节点粒子"))
		{
			CollectChildParticleSystems();
			serializedObject.Update();
			dataProperty = serializedObject.FindProperty("data");
		}

		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		searchKey = EditorGUILayout.TextField(searchKey);
		EditorGUILayout.ObjectField(previewParticleSystem, typeof(ParticleSystem), true);
		if (GUILayout.Button("删除"))
		{
			referenceParticleCollector.Remove(searchKey);
			previewParticleSystem = null;
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
			property = dataProperty.GetArrayElementAtIndex(i).FindPropertyRelative("particleSystem");
			property.objectReferenceValue = EditorGUILayout.ObjectField(property.objectReferenceValue, typeof(ParticleSystem), true);
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
					ParticleSystem ps = o as ParticleSystem;
					if (ps == null && o is GameObject go)
					{
						ps = go.GetComponent<ParticleSystem>();
					}

					if (ps != null)
					{
						AddReference(dataProperty, ps.gameObject.name, ps);
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

	private void AddReference(SerializedProperty dataProperty, string key, ParticleSystem particleSystem)
	{
		int index = dataProperty.arraySize;
		dataProperty.InsertArrayElementAtIndex(index);
		var element = dataProperty.GetArrayElementAtIndex(index);
		element.FindPropertyRelative("key").stringValue = key;
		element.FindPropertyRelative("particleSystem").objectReferenceValue = particleSystem;
	}

	private void CollectChildParticleSystems()
	{
		referenceParticleCollector.Clear();

		ParticleSystem[] particleSystems = referenceParticleCollector.GetComponentsInChildren<ParticleSystem>(true);
		for (int i = 0; i < particleSystems.Length; i++)
		{
			referenceParticleCollector.Add($"Particle{i + 1}", particleSystems[i]);
		}
	}
}
