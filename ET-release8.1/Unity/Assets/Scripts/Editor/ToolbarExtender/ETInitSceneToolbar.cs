using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityToolbarExtender;

namespace ET
{
	/// <summary>
	/// 在工具栏播放按钮左侧增加入口场景按钮（实现方式同 GridWar 的 Launcher：Unity Toolbar Extender）。
	/// </summary>
	[InitializeOnLoad]
	public static class ETInitSceneToolbar
	{
		private const string PreviousSceneKey = "ET_PreviousScenePath";
		private const string IsInitToolbarKey = "ET_PlayFromInitToolbar";
		private const string InitScenePath = "Assets/Scenes/Init.unity";

		private static readonly string ButtonStyleName = "Tab middle";
		private static GUIStyle _buttonGuiStyle;

		static ETInitSceneToolbar()
		{
			ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
			EditorApplication.quitting += OnEditorQuit;
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
			if (GUILayout.Button(
					new GUIContent("Init", EditorGUIUtility.FindTexture("PlayButton"),
						"从入口场景播放（与 ET/Play Init Scene、快捷键 F1 相同）；停止运行后可恢复之前打开的场景。"),
					_buttonGuiStyle))
			{
				SceneHelper.StartInitScene();
			}
		}

		private static void OnPlayModeStateChanged(PlayModeStateChange state)
		{
			if (state == PlayModeStateChange.EnteredEditMode)
			{
				var previousScenePath = EditorPrefs.GetString(PreviousSceneKey, string.Empty);
				if (!string.IsNullOrEmpty(previousScenePath) && EditorPrefs.GetBool(IsInitToolbarKey))
				{
					EditorApplication.delayCall += () =>
					{
						if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
						{
							EditorSceneManager.OpenScene(previousScenePath);
						}
					};
				}

				EditorPrefs.SetBool(IsInitToolbarKey, false);
			}
		}

		private static void OnEditorQuit()
		{
			EditorPrefs.SetString(PreviousSceneKey, string.Empty);
			EditorPrefs.SetBool(IsInitToolbarKey, false);
		}

		private static class SceneHelper
		{
			private static string _pendingOpenPath;

			public static void StartInitScene()
			{
				if (EditorApplication.isPlaying)
				{
					EditorApplication.isPlaying = false;
				}

				if (!File.Exists(InitScenePath))
				{
					Debug.LogError($"未找到入口场景: {InitScenePath}");
					return;
				}

				var activeScene = SceneManager.GetActiveScene();
				if (activeScene.isLoaded && activeScene.path != InitScenePath)
				{
					EditorPrefs.SetString(PreviousSceneKey, activeScene.path);
					EditorPrefs.SetBool(IsInitToolbarKey, true);
				}

				_pendingOpenPath = InitScenePath;
				EditorApplication.update += OnUpdate;
			}

			private static void OnUpdate()
			{
				if (string.IsNullOrEmpty(_pendingOpenPath) ||
				    EditorApplication.isPlaying || EditorApplication.isPaused ||
				    EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)
				{
					return;
				}

				EditorApplication.update -= OnUpdate;

				if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
				{
					EditorSceneManager.OpenScene(_pendingOpenPath, OpenSceneMode.Single);
					EditorApplication.isPlaying = true;
				}

				_pendingOpenPath = null;
			}
		}
	}
}
