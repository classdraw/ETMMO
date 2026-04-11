using System;
using System.Collections.Generic;
using System.Text;
using ET;
using UnityEditor;
using UnityEngine;
//Object并非C#基础中的Object，而是 UnityEngine.Object
using Object = UnityEngine.Object;

//自定义ReferenceCollector类在界面中的显示与功能
[CustomEditor(typeof (ReferenceCollector))]
public class ReferenceCollectorEditor: Editor
{
    //输入在textfield中的字符串
    private string searchKey
	{
		get
		{
			return _searchKey;
		}
		set
		{
			if (_searchKey != value)
			{
				_searchKey = value;
				heroPrefab = referenceCollector.Get<Object>(searchKey);
			}
		}
	}

	private ReferenceCollector referenceCollector;

	private Object heroPrefab;

	private string _searchKey = "";

	private void DelNullReference()
	{
		var dataProperty = serializedObject.FindProperty("data");
		for (int i = dataProperty.arraySize - 1; i >= 0; i--)
		{
			var gameObjectProperty = dataProperty.GetArrayElementAtIndex(i).FindPropertyRelative("gameObject");
			if (gameObjectProperty.objectReferenceValue == null)
			{
				dataProperty.DeleteArrayElementAtIndex(i);
				EditorUtility.SetDirty(referenceCollector);
				serializedObject.ApplyModifiedProperties();
				serializedObject.UpdateIfRequiredOrScript();
			}
		}
	}

	private void OnEnable()
	{
        //将被选中的gameobject所挂载的ReferenceCollector赋值给编辑器类中的ReferenceCollector，方便操作
        referenceCollector = (ReferenceCollector) target;
	}

	public override void OnInspectorGUI()
	{
        //使ReferenceCollector支持撤销操作，还有Redo，不过没有在这里使用
        Undo.RecordObject(referenceCollector, "Changed Settings");
		serializedObject.Update();
		var dataProperty = serializedObject.FindProperty("data");
        //开始水平布局，如果是比较新版本学习U3D的，可能不知道这东西，这个是老GUI系统的知识，除了用在编辑器里，还可以用在生成的游戏中
		GUILayout.BeginHorizontal();
        //下面几个if都是点击按钮就会返回true调用里面的东西
		if (GUILayout.Button("添加引用"))
		{
            //添加新的元素，具体的函数注释
            // Guid.NewGuid().GetHashCode().ToString() 就是新建后默认的key
            AddReference(dataProperty, Guid.NewGuid().GetHashCode().ToString(), null);
		}
		if (GUILayout.Button("全部删除"))
		{
			referenceCollector.Clear();
		}
		if (GUILayout.Button("删除空引用"))
		{
			DelNullReference();
		}
		if (GUILayout.Button("排序"))
		{
			referenceCollector.Sort();
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("角色绑点收集"))
		{
			BindSpriteRendererNodes();
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
        //可以在编辑器中对searchKey进行赋值，只要输入对应的Key值，就可以点后面的删除按钮删除相对应的元素
        searchKey = EditorGUILayout.TextField(searchKey);
        //添加的可以用于选中Object的框，这里的object也是(UnityEngine.Object
        //第三个参数为是否只能引用scene中的Object
        EditorGUILayout.ObjectField(heroPrefab, typeof (Object), false);
		if (GUILayout.Button("删除"))
		{
			referenceCollector.Remove(searchKey);
			heroPrefab = null;
		}
		GUILayout.EndHorizontal();
		EditorGUILayout.Space();

		var delList = new List<int>();
        SerializedProperty property;
        // 必须以 dataProperty.arraySize 为准，避免与 referenceCollector.data.Count 不同步导致 GetArrayElementAtIndex 越界
        for (int i = dataProperty.arraySize - 1; i >= 0; i--)
		{
			GUILayout.BeginHorizontal();
            //这里的知识点在ReferenceCollector中有说
            property = dataProperty.GetArrayElementAtIndex(i).FindPropertyRelative("key");
            property.stringValue = EditorGUILayout.TextField(property.stringValue, GUILayout.Width(150));
            property = dataProperty.GetArrayElementAtIndex(i).FindPropertyRelative("gameObject");
            property.objectReferenceValue = EditorGUILayout.ObjectField(property.objectReferenceValue, typeof(Object), true);
			if (GUILayout.Button("X"))
			{
                //将元素添加进删除list
				delList.Add(i);
			}
			GUILayout.EndHorizontal();
		}
		var eventType = Event.current.type;
        //在Inspector 窗口上创建区域，向区域拖拽资源对象，获取到拖拽到区域的对象
        if (eventType == EventType.DragUpdated || eventType == EventType.DragPerform)
		{
			// Show a copy icon on the drag
			DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

			if (eventType == EventType.DragPerform)
			{
				DragAndDrop.AcceptDrag();
				foreach (var o in DragAndDrop.objectReferences)
				{
					AddReference(dataProperty, o.name, o);
				}
			}

			Event.current.Use();
		}

        //遍历删除list，将其删除掉
		foreach (var i in delList)
		{
			dataProperty.DeleteArrayElementAtIndex(i);
		}
		serializedObject.ApplyModifiedProperties();
		serializedObject.UpdateIfRequiredOrScript();
	}

	/// <summary>
	/// 收集：激活且可见、已赋 sprite 的 SpriteRenderer。
	/// 仅当物体名与白名单某 key 完全一致（Ordinal）时才写入该 key。
	/// </summary>
	private void BindSpriteRendererNodes()
	{
		HashSet<string> keyAllowlist = AvatarBindKeyAllowlistUtility.ParseToSet();
		if (keyAllowlist.Count == 0)
		{
			EditorUtility.DisplayDialog("ReferenceCollector",
				"请配置角色绑点 Key 白名单：在 Project 中创建或使用 " + AvatarBindKeyAllowlistUtility.DefaultAssetPath +
				"（菜单 Create → ET → 角色绑点 Key 白名单，或在「预制体处理工具」中点「创建资源」），填写 keys 后点「保存到资源」。",
				"确定");
			return;
		}

		Undo.RecordObject(referenceCollector, "ReferenceCollector 角色绑定");
		var spriteRenderers = referenceCollector.GetComponentsInChildren<SpriteRenderer>(false);
		var usedKeys = new HashSet<string>(StringComparer.Ordinal);
		foreach (var item in referenceCollector.data)
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
				Debug.LogWarning($"[ReferenceCollector] 跳过「{go.name}」：物体名须与白名单某 key 完全一致。");
				continue;
			}

			if (usedKeys.Contains(resolvedKey))
			{
				Debug.LogWarning($"[ReferenceCollector] 跳过「{go.name}」：key「{resolvedKey}」已占用。");
				continue;
			}

			usedKeys.Add(resolvedKey);
			referenceCollector.Add(resolvedKey, go);
		}

		referenceCollector.Sort();
		EditorUtility.SetDirty(referenceCollector);

		// 打印当前所有 key，英文逗号分割，便于复制
		var keysSb = new StringBuilder();
		for (int i = 0; i < referenceCollector.data.Count; i++)
		{
			string k = referenceCollector.data[i]?.key;
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

	/// <summary>激活、Renderer 启用、且已指定 Sprite。</summary>
	private static bool IsCollectableSpriteRenderer(SpriteRenderer sr)
	{
		return sr.enabled && sr.sprite != null;
	}

    //添加元素，具体知识点在ReferenceCollector中说了
    private void AddReference(SerializedProperty dataProperty, string key, Object obj)
	{
		int index = dataProperty.arraySize;
		dataProperty.InsertArrayElementAtIndex(index);
		var element = dataProperty.GetArrayElementAtIndex(index);
		element.FindPropertyRelative("key").stringValue = key;
		element.FindPropertyRelative("gameObject").objectReferenceValue = obj;
	}
}
