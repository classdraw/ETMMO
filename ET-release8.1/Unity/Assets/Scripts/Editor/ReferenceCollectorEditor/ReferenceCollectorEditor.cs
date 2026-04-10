using System;
using System.Collections.Generic;
using System.Text;
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
	/// 收集：激活且可见（组件启用）、已赋 sprite 的 SpriteRenderer；key 由节点名规范为大写英文标识（英文字母开头）。
	/// </summary>
	private void BindSpriteRendererNodes()
	{
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
			string baseKey = NodeNameToPascalEnglishKey(go.name);
			string key = baseKey;
			int n = 2;
			while (usedKeys.Contains(key))
			{
				key = $"{baseKey}_{n}";
				n++;
			}

			usedKeys.Add(key);
			referenceCollector.Add(key, go);
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

	/// <summary>激活、Renderer 启用、且已指定 Sprite。</summary>
	private static bool IsCollectableSpriteRenderer(SpriteRenderer sr)
	{
		return sr.enabled && sr.sprite != null;
	}

	/// <summary>
	/// 仅保留英文字母与数字，并转为 PascalCase（每个单词首字母大写，其余小写）。
	/// 必须以英文字母开头（否则前缀 R）。
	/// </summary>
	private static string NodeNameToPascalEnglishKey(string rawName)
	{
		if (string.IsNullOrEmpty(rawName))
		{
			return "R";
		}

		var sb = new StringBuilder();
		bool newWord = true;
		bool prevWasLower = false;

		foreach (char c in rawName)
		{
			bool isLetter = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
			bool isDigit = (c >= '0' && c <= '9');

			if (!isLetter && !isDigit)
			{
				newWord = true;
				prevWasLower = false;
				continue;
			}

			if (isDigit)
			{
				sb.Append(c);
				// 数字后如果跟字母，视为新单词（例如 2d -> 2D）
				newWord = true;
				prevWasLower = false;
				continue;
			}

			// 处理大小写：新单词首字母大写，其余小写；并将 camelCase 的大写也视作新单词
			bool isUpper = (c >= 'A' && c <= 'Z');
			bool isLower = (c >= 'a' && c <= 'z');
			bool upperStartsNewWord = isUpper && prevWasLower;

			if (newWord || upperStartsNewWord)
			{
				char upper = isLower ? (char)(c - 32) : c;
				sb.Append(upper);
				newWord = false;
			}
			else
			{
				char lower = isUpper ? (char)(c + 32) : c;
				sb.Append(lower);
			}

			prevWasLower = isLower;
		}

		string s = sb.ToString();
		if (s.Length == 0)
		{
			return "R";
		}

		char first = s[0];
		bool startsWithLetter = (first >= 'A' && first <= 'Z') || (first >= 'a' && first <= 'z');
		if (!startsWithLetter)
		{
			return "R" + s;
		}

		// 确保首字母大写
		if (first >= 'a' && first <= 'z')
		{
			s = (char)(first - 32) + s.Substring(1);
		}

		return s;
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
