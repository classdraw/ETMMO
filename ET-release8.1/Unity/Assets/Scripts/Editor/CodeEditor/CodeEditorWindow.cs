using System.IO;
using UnityEditor;
using UnityEngine;

namespace ET
{
    public enum CodeType
    {
        Simple,
        Normal,
    }

    public enum ServerClientType
    {
        Server,
        Client
    }

    public class CodeEditorWindow : EditorWindow
    {
        private string nameInput = "";
        private CodeType codeType = CodeType.Simple;
        private ServerClientType serverClientType = ServerClientType.Client;
        private bool isViewLayer = false;
        private bool createSystem = true;

        [MenuItem("Tools/Code/代码创建", false, 1)]
        public static void ShowWindow()
        {
            GetWindow<CodeEditorWindow>("Code Editor");
        }

        private void OnEnable()
        {
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("名称:", GUILayout.Width(80));
            this.nameInput = EditorGUILayout.TextField(this.nameInput, GUILayout.Width(200));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("代码类型:", GUILayout.Width(80));
            this.codeType = (CodeType)EditorGUILayout.EnumPopup(this.codeType, GUILayout.Width(150));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Server/Client:", GUILayout.Width(80));
            this.serverClientType = (ServerClientType)EditorGUILayout.EnumPopup(this.serverClientType, GUILayout.Width(150));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            this.isViewLayer = EditorGUILayout.Toggle("是否显示层:", this.isViewLayer, GUILayout.Width(150));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            this.createSystem = EditorGUILayout.Toggle("是否创建System:", this.createSystem, GUILayout.Width(150));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // 检查名称是否为空
            bool isNameValid = !string.IsNullOrWhiteSpace(this.nameInput);
            // 检查显示层是否选择了Server（显示层不支持Server）
            bool isViewLayerWithServer = this.isViewLayer && this.serverClientType == ServerClientType.Server;
            bool canCreate = isNameValid && !isViewLayerWithServer;
            
            EditorGUI.BeginDisabledGroup(!canCreate);
            if (GUILayout.Button("创建", GUILayout.Height(30)))
            {
                OnCreateButtonClicked();
            }
            EditorGUI.EndDisabledGroup();

            if (!isNameValid)
            {
                EditorGUILayout.HelpBox("名称不能为空", MessageType.Warning);
            }
            else if (isViewLayerWithServer)
            {
                EditorGUILayout.HelpBox("显示层不支持Server，请选择Client", MessageType.Warning);
            }

            EditorGUILayout.Space(10);
        }

        private void OnCreateButtonClicked()
        {
            // 双重验证：显示层不支持Server
            if (this.isViewLayer && this.serverClientType == ServerClientType.Server)
            {
                EditorUtility.DisplayDialog("错误", "显示层不支持Server，请选择Client", "确定");
                return;
            }

            try
            {
                string componentName = this.nameInput + "Component";
                string systemName = componentName + "System";
                
                // 确定路径
                string modelPath = GetModelPath();
                string hotfixPath = GetHotfixPath();
                
                string componentFilePath = Path.Combine(modelPath, $"{componentName}.cs");
                string systemFilePath = Path.Combine(hotfixPath, $"{systemName}.cs");

                if (File.Exists(componentFilePath))
                {
                    EditorUtility.DisplayDialog("错误", $"文件已存在:\n{componentFilePath}", "确定");
                    return;
                }

                if (this.createSystem && File.Exists(systemFilePath))
                {
                    EditorUtility.DisplayDialog("错误", $"文件已存在:\n{systemFilePath}", "确定");
                    return;
                }

                CreateComponentFile(componentFilePath, componentName);

                if (this.createSystem)
                {
                    CreateSystemFile(systemFilePath, componentName, systemName);
                }
                
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("成功", $"代码创建成功！\nComponent: {componentFilePath}\n" +
                    (this.createSystem ? $"System: {systemFilePath}" : ""), "确定");
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("错误", $"创建代码失败: {e.Message}", "确定");
                Debug.LogError(e);
            }
        }

        private string GetModelPath()
        {
            string clientServer = this.serverClientType == ServerClientType.Client ? "Client" : "Server";
            if (this.isViewLayer)
            {
                return Path.Combine("Assets/Scripts/ModelView", clientServer, "StateSync", "UI", GetUiFolderName());
            }

            return Path.Combine("Assets/Scripts/Model", clientServer);
        }

        private string GetHotfixPath()
        {
            string clientServer = this.serverClientType == ServerClientType.Client ? "Client" : "Server";
            if (this.isViewLayer)
            {
                return Path.Combine("Assets/Scripts/HotfixView", clientServer, "StateSync", "UI", GetUiFolderName());
            }

            string basePath = "Assets/Scripts/Hotfix";
            return Path.Combine(basePath, clientServer);
        }

        private string GetUiFolderName()
        {
            return this.nameInput.Trim();
        }

        private void CreateComponentFile(string filePath, string componentName)
        {
            string namespaceName = this.serverClientType == ServerClientType.Client ? "ET.Client" : "ET.Server";

            string interfaces;
            if (this.isViewLayer)
            {
                interfaces = this.codeType == CodeType.Simple
                    ? "Entity, IAwake, IDestroy"
                    : "Entity, IAwake, IUpdate, IDestroy";
            }
            else if (this.codeType == CodeType.Simple)
            {
                interfaces = "Entity, IAwake";
            }
            else
            {
                interfaces = "Entity, IAwake, IUpdate, IDestroy";
            }

            string componentOf = this.isViewLayer
                ? $@"
    [ComponentOf(typeof(UI))]
    public class {componentName}: {interfaces}"
                : $@"
    public class {componentName}: {interfaces}";

            string componentUsings = this.isViewLayer
                ? @"using ET;
using GameLogic;
using UnityEngine;
using UnityEngine.UI;

"
                : "";

            string content = $@"{componentUsings}namespace {namespaceName}
{{{componentOf}
    {{
    }}
}}";

            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(filePath, content);
        }

        private void CreateSystemFile(string filePath, string componentName, string systemName)
        {
            string namespaceName = this.serverClientType == ServerClientType.Client ? "ET.Client" : "ET.Server";

            string awakeBody = this.isViewLayer
                ? $@"            UIBindComponent bind = self.GetParent<UI>().GameObject.GetComponent<UIBindComponent>();"
                : "";

            string methods;
            if (this.isViewLayer)
            {
                if (this.codeType == CodeType.Simple)
                {
                    methods = $@"        [EntitySystem]
        private static void Awake(this {componentName} self)
        {{
{awakeBody}
        }}

        [EntitySystem]
        private static void Destroy(this {componentName} self)
        {{
        }}";
                }
                else
                {
                    methods = $@"        [EntitySystem]
        private static void Awake(this {componentName} self)
        {{
{awakeBody}
        }}

        [EntitySystem]
        private static void Update(this {componentName} self)
        {{
        }}

        [EntitySystem]
        private static void Destroy(this {componentName} self)
        {{
        }}";
                }
            }
            else if (this.codeType == CodeType.Simple)
            {
                methods = $@"        [EntitySystem]
        private static void Awake(this {componentName} self)
        {{
        }}";
            }
            else
            {
                methods = $@"        [EntitySystem]
        private static void Awake(this {componentName} self)
        {{
        }}

        [EntitySystem]
        private static void Update(this {componentName} self)
        {{
        }}

        [EntitySystem]
        private static void Destroy(this {componentName} self)
        {{
        }}";
            }
            
            string usings = this.isViewLayer
                ? @"using ET;
using GameLogic;
using UnityEngine;
using UnityEngine.UI;
using XEngine.UI;

"
                : "";

            string content = $@"{usings}namespace {namespaceName}
{{
    [EntitySystemOf(typeof({componentName}))]
    [FriendOf(typeof({componentName}))]
    public static partial class {systemName}
    {{
{methods}
    }}
}}";

            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(filePath, content);
        }
    }
}
