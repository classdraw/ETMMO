using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityToolbarExtender;

namespace ET
{
	/// <summary>
	/// 在工具栏播放按钮左侧增加场景下拉切换（不含 Assets/Art 目录），仅打开场景不运行。
	/// F1 / ET/Play Init Scene 仍由 <see cref="PlayInitSceneShortcut"/> 负责打开 Init 并播放。
	/// </summary>
	[InitializeOnLoad]
	public static class ETInitSceneToolbar
	{
		private const string ArtFolderToken = "/Art/";

		private static readonly string ButtonStyleName = "Tab middle";
		private static GUIStyle _buttonGuiStyle;
		private static string _pendingOpenPath;

		static ETInitSceneToolbar()
		{
			ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
		}

		private static void OnToolbarGUI()
		{
			_buttonGuiStyle ??= new GUIStyle(ButtonStyleName)
			{
				padding = new RectOffset(2, 8, 2, 2),
				alignment = TextAnchor.MiddleCenter,
				fontStyle = FontStyle.Bold
			};

			GUILayout.FlexibleSpace();

			string label = GetButtonLabel();
			var content = new GUIContent(label, "切换工程场景（不含 Art 目录），不自动运行");
			Rect buttonRect = GUILayoutUtility.GetRect(content, _buttonGuiStyle, GUILayout.MinWidth(72));

			if (EditorGUI.DropdownButton(buttonRect, content, FocusType.Passive, _buttonGuiStyle))
			{
				ShowSceneMenu(buttonRect);
			}
		}

		private static void ShowSceneMenu(Rect buttonRect)
		{
			string[] scenePaths = GetProjectScenes();
			var menu = new GenericMenu();

			if (scenePaths.Length == 0)
			{
				menu.AddDisabledItem(new GUIContent("未找到场景"));
			}
			else
			{
				string activePath = SceneManager.GetActiveScene().path;
				foreach (string scenePath in scenePaths)
				{
					string menuPath = GetMenuPath(scenePath);
					bool isActive = string.Equals(activePath, scenePath, StringComparison.OrdinalIgnoreCase);
					string path = scenePath;
					menu.AddItem(new GUIContent(menuPath), isActive, () => OpenScene(path));
				}
			}

			menu.DropDown(buttonRect);
		}

		private static string GetButtonLabel()
		{
			string activePath = SceneManager.GetActiveScene().path;
			if (!string.IsNullOrEmpty(activePath))
			{
				return Path.GetFileNameWithoutExtension(activePath);
			}

			return "Scenes";
		}

		private static string[] GetProjectScenes()
		{
			var scenes = new List<string>();
			string[] guids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });

			foreach (string guid in guids)
			{
				string path = AssetDatabase.GUIDToAssetPath(guid).Replace('\\', '/');
				if (path.Contains(ArtFolderToken, StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}

				scenes.Add(path);
			}

			scenes.Sort(StringComparer.OrdinalIgnoreCase);
			return scenes.ToArray();
		}

		private static string GetMenuPath(string scenePath)
		{
			string path = scenePath.Replace('\\', '/');
			const string prefix = "Assets/";
			if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
			{
				path = path.Substring(prefix.Length);
			}

			return Path.ChangeExtension(path, null);
		}

		private static void OpenScene(string scenePath)
		{
			if (!File.Exists(scenePath))
			{
				Debug.LogError($"未找到场景: {scenePath}");
				return;
			}

			if (EditorApplication.isPlaying)
			{
				EditorApplication.isPlaying = false;
				_pendingOpenPath = scenePath;
				EditorApplication.update -= OnPendingOpenUpdate;
				EditorApplication.update += OnPendingOpenUpdate;
				return;
			}

			TryOpenScene(scenePath);
		}

		private static void OnPendingOpenUpdate()
		{
			if (EditorApplication.isPlaying || EditorApplication.isPaused ||
			    EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)
			{
				return;
			}

			EditorApplication.update -= OnPendingOpenUpdate;

			if (string.IsNullOrEmpty(_pendingOpenPath))
			{
				return;
			}

			string path = _pendingOpenPath;
			_pendingOpenPath = null;
			TryOpenScene(path);
		}

		private static void TryOpenScene(string scenePath)
		{
			if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
			{
				return;
			}

			EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
		}
	}
}
