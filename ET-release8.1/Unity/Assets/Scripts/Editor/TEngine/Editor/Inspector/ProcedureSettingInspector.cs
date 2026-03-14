using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TEngine.Editor.Inspector
{
    [CustomEditor(typeof(ProcedureSetting))]
    internal sealed class ProcedureSettingInspector : GameFrameworkInspector
    {
        private SerializedProperty _availableProcedureTypeNames = null;
        private SerializedProperty _entranceProcedureTypeName = null;

        private string[] _procedureTypeNames = null;
        private List<string> _currentAvailableProcedureTypeNames = null;
        private int _entranceProcedureIndex = -1;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            ProcedureSetting t = (ProcedureSetting)target;

            if (_entranceProcedureTypeName != null && string.IsNullOrEmpty(_entranceProcedureTypeName.stringValue))
            {
                EditorGUILayout.HelpBox("Entrance procedure is invalid.", MessageType.Error);
            }

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                GUILayout.Label("Available Procedures", EditorStyles.boldLabel);
                if (_procedureTypeNames != null && _procedureTypeNames.Length > 0)
                {
                    EditorGUILayout.BeginVertical("box");
                    {
                        foreach (string procedureTypeName in _procedureTypeNames)
                        {
                            bool selected = _currentAvailableProcedureTypeNames != null && _currentAvailableProcedureTypeNames.Contains(procedureTypeName);
                            if (selected != EditorGUILayout.ToggleLeft(procedureTypeName, selected))
                            {
                                if (!selected)
                                {
                                    if (_currentAvailableProcedureTypeNames == null)
                                    {
                                        _currentAvailableProcedureTypeNames = new List<string>();
                                    }
                                    _currentAvailableProcedureTypeNames.Add(procedureTypeName);
                                    WriteAvailableProcedureTypeNames();
                                }
                                else if (_entranceProcedureTypeName != null && procedureTypeName != _entranceProcedureTypeName.stringValue)
                                {
                                    if (_currentAvailableProcedureTypeNames != null)
                                    {
                                        _currentAvailableProcedureTypeNames.Remove(procedureTypeName);
                                        WriteAvailableProcedureTypeNames();
                                    }
                                }
                            }
                        }
                    }
                    EditorGUILayout.EndVertical();
                }
                else
                {
                    EditorGUILayout.HelpBox("There is no available procedure.", MessageType.Warning);
                }

                if (_currentAvailableProcedureTypeNames != null && _currentAvailableProcedureTypeNames.Count > 0)
                {
                    EditorGUILayout.Separator();

                    int selectedIndex = EditorGUILayout.Popup("Entrance Procedure", _entranceProcedureIndex, _currentAvailableProcedureTypeNames.ToArray());
                    if (selectedIndex != _entranceProcedureIndex)
                    {
                        _entranceProcedureIndex = selectedIndex;
                        _entranceProcedureTypeName.stringValue = _currentAvailableProcedureTypeNames[selectedIndex];
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("Select available procedures first.", MessageType.Info);
                }
            }
            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();

            Repaint();
        }

        protected override void OnCompileComplete()
        {
            base.OnCompileComplete();

            RefreshTypeNames();
        }

        private void OnEnable()
        {
            _availableProcedureTypeNames = serializedObject.FindProperty("availableProcedureTypeNames");
            _entranceProcedureTypeName = serializedObject.FindProperty("entranceProcedureTypeName");

            RefreshTypeNames();
        }

        private void RefreshTypeNames()
        {
            if (_availableProcedureTypeNames == null || _entranceProcedureTypeName == null)
            {
                return;
            }

            _procedureTypeNames = Type.GetRuntimeTypeNames(typeof(ProcedureBase));
            if (_procedureTypeNames == null)
            {
                _procedureTypeNames = new string[0];
            }

            ReadAvailableProcedureTypeNames();
            if (_currentAvailableProcedureTypeNames == null)
            {
                _currentAvailableProcedureTypeNames = new List<string>();
            }

            int oldCount = _currentAvailableProcedureTypeNames.Count;
            _currentAvailableProcedureTypeNames = _currentAvailableProcedureTypeNames.Where(x => _procedureTypeNames.Contains(x)).ToList();
            if (_currentAvailableProcedureTypeNames.Count != oldCount)
            {
                WriteAvailableProcedureTypeNames();
            }
            else if (!string.IsNullOrEmpty(_entranceProcedureTypeName.stringValue))
            {
                _entranceProcedureIndex = _currentAvailableProcedureTypeNames.IndexOf(_entranceProcedureTypeName.stringValue);
                if (_entranceProcedureIndex < 0)
                {
                    _entranceProcedureTypeName.stringValue = null;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void ReadAvailableProcedureTypeNames()
        {
            _currentAvailableProcedureTypeNames = new List<string>();
            if (_availableProcedureTypeNames == null)
            {
                return;
            }

            int count = _availableProcedureTypeNames.arraySize;
            for (int i = 0; i < count; i++)
            {
                _currentAvailableProcedureTypeNames.Add(_availableProcedureTypeNames.GetArrayElementAtIndex(i).stringValue);
            }
        }

        private void WriteAvailableProcedureTypeNames()
        {
            if (_availableProcedureTypeNames == null)
            {
                return;
            }

            _availableProcedureTypeNames.ClearArray();
            if (_currentAvailableProcedureTypeNames == null)
            {
                return;
            }

            _currentAvailableProcedureTypeNames.Sort();
            int count = _currentAvailableProcedureTypeNames.Count;
            for (int i = 0; i < count; i++)
            {
                _availableProcedureTypeNames.InsertArrayElementAtIndex(i);
                _availableProcedureTypeNames.GetArrayElementAtIndex(i).stringValue = _currentAvailableProcedureTypeNames[i];
            }

            if (_entranceProcedureTypeName != null && !string.IsNullOrEmpty(_entranceProcedureTypeName.stringValue))
            {
                _entranceProcedureIndex = _currentAvailableProcedureTypeNames.IndexOf(_entranceProcedureTypeName.stringValue);
                if (_entranceProcedureIndex < 0)
                {
                    _entranceProcedureTypeName.stringValue = null;
                }
            }
        }
    }
}