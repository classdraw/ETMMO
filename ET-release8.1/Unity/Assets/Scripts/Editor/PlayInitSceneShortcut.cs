using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace ET
{
	/// <summary>
	/// F1：保存当前场景提示后打开 Init 并进入 Play（可在 Edit → Shortcuts 中改键）。
	/// </summary>
	public static class PlayInitSceneShortcut
	{
		private const string InitScenePath = "Assets/Scenes/Init.unity";

		[Shortcut("ET/Play Init Scene", KeyCode.F1, ShortcutModifiers.None)]
		private static void PlayInitSceneShortcutAction()
		{
			StartPlayInitScene();
		}

		[MenuItem("ET/Play Init Scene", false, 200)]
		private static void PlayInitSceneMenu()
		{
			StartPlayInitScene();
		}

		private static void StartPlayInitScene()
		{
			if (EditorApplication.isPlayingOrWillChangePlaymode)
			{
				return;
			}

			if (!System.IO.File.Exists(InitScenePath))
			{
				Debug.LogError($"未找到场景: {InitScenePath}");
				return;
			}

			if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
			{
				return;
			}

			EditorSceneManager.OpenScene(InitScenePath, OpenSceneMode.Single);
			EditorApplication.isPlaying = true;
		}
	}
}
