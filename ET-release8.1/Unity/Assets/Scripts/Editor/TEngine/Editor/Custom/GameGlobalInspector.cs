using System;
using TEngine;
using TEngine.Editor.Inspector;
using UnityEditor;
using UnityEngine;

namespace ET
{
    [CustomEditor(typeof(GameGlobal))]
    internal sealed class GameGlobalInspector : GameFrameworkInspector
    {
        private static readonly float[] GameSpeed = new float[] { 0f, 0.01f, 0.1f, 0.25f, 0.5f, 1f, 1.5f, 2f, 4f, 8f };
        private static readonly string[] GameSpeedForDisplay = new string[] { "0x", "0.01x", "0.1x", "0.25x", "0.5x", "1x", "1.5x", "2x", "4x", "8x" };

        private SerializedProperty _textHelperTypeName = null;
        private SerializedProperty _logHelperTypeName = null;
        private SerializedProperty _jsonHelperTypeName = null;
        private SerializedProperty _frameRate = null;
        private SerializedProperty _gameSpeed = null;
        private SerializedProperty _runInBackground = null;
        private SerializedProperty _neverSleep = null;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            GameGlobal t = (GameGlobal)target;

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                EditorGUILayout.BeginVertical("box");
                {
                    EditorGUILayout.LabelField("Global Helpers", EditorStyles.boldLabel);

                    EditorGUILayout.PropertyField(_textHelperTypeName, new GUIContent("Text Helper"));
                    EditorGUILayout.PropertyField(_logHelperTypeName, new GUIContent("Log Helper"));
                    EditorGUILayout.PropertyField(_jsonHelperTypeName, new GUIContent("JSON Helper"));
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUI.EndDisabledGroup();

            int frameRate = EditorGUILayout.IntSlider("Frame Rate", _frameRate.intValue, 1, 120);
            if (frameRate != _frameRate.intValue)
            {
                if (EditorApplication.isPlaying)
                {
                    t.FrameRate = frameRate;
                }
                else
                {
                    _frameRate.intValue = frameRate;
                }
            }

            EditorGUILayout.BeginVertical("box");
            {
                float gameSpeed = EditorGUILayout.Slider("Game Speed", _gameSpeed.floatValue, 0f, 8f);
                int selectedGameSpeed = GUILayout.SelectionGrid(GetSelectedGameSpeed(gameSpeed), GameSpeedForDisplay, 5);
                if (selectedGameSpeed >= 0)
                {
                    gameSpeed = GetGameSpeed(selectedGameSpeed);
                }

                if (Math.Abs(gameSpeed - _gameSpeed.floatValue) > 0.01f)
                {
                    if (EditorApplication.isPlaying)
                    {
                        t.GameSpeed = gameSpeed;
                    }
                    else
                    {
                        _gameSpeed.floatValue = gameSpeed;
                    }
                }
            }
            EditorGUILayout.EndVertical();

            bool runInBackground = EditorGUILayout.Toggle("Run in Background", _runInBackground.boolValue);
            if (runInBackground != _runInBackground.boolValue)
            {
                if (EditorApplication.isPlaying)
                {
                    t.RunInBackground = runInBackground;
                }
                else
                {
                    _runInBackground.boolValue = runInBackground;
                }
            }

            bool neverSleep = EditorGUILayout.Toggle("Never Sleep", _neverSleep.boolValue);
            if (neverSleep != _neverSleep.boolValue)
            {
                if (EditorApplication.isPlaying)
                {
                    t.NeverSleep = neverSleep;
                }
                else
                {
                    _neverSleep.boolValue = neverSleep;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            _textHelperTypeName = serializedObject.FindProperty("textHelperTypeName");
            _logHelperTypeName = serializedObject.FindProperty("logHelperTypeName"); 
            _jsonHelperTypeName = serializedObject.FindProperty("jsonHelperTypeName");
            _frameRate = serializedObject.FindProperty("frameRate");
            _gameSpeed = serializedObject.FindProperty("gameSpeed");
            _runInBackground = serializedObject.FindProperty("runInBackground");
            _neverSleep = serializedObject.FindProperty("neverSleep");
        }

        private float GetGameSpeed(int selectedGameSpeed)
        {
            if (selectedGameSpeed < 0)
            {
                return GameSpeed[0];
            }

            if (selectedGameSpeed >= GameSpeed.Length)
            {
                return GameSpeed[GameSpeed.Length - 1];
            }

            return GameSpeed[selectedGameSpeed];
        }

        private int GetSelectedGameSpeed(float gameSpeed)
        {
            for (int i = 0; i < GameSpeed.Length; i++)
            {
                if (Mathf.Approximately(gameSpeed, GameSpeed[i]))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
