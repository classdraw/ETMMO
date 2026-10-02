using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ET
{
    public enum DevelopMode
    {
        正式 = 0,
        开发 = 1,
        压测 = 2,
    }

    public class ServerCommandLineEditor: EditorWindow
    {
        [MenuItem("ET/ServerTools", false, ETMenuItemPriority.ServerTools)]
        public static void ShowWindow()
        {
            GetWindow<ServerCommandLineEditor>(DockDefine.Types);
        }

        private int selectStartConfigIndex = 1;
        private string[] startConfigs;
        private string startConfig;
        private DevelopMode developMode;

        public void OnEnable()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo("../Config/Excel/cs/StartConfig");
            this.startConfigs = directoryInfo.GetDirectories().Select(x => x.Name).ToArray();
        }

        public void OnGUI()
        {
            selectStartConfigIndex = EditorGUILayout.Popup(selectStartConfigIndex, this.startConfigs);
            this.startConfig = this.startConfigs[this.selectStartConfigIndex];
            this.developMode = (DevelopMode)EditorGUILayout.EnumPopup("起服模式：", this.developMode);

            if (GUILayout.Button("Start Server(Single Process)"))
            {
                string arguments = $"--Process=1 --StartConfig=StartConfig/{this.startConfig} --Console=1 --Develop={(int)this.developMode} --LogLevel=1";
#if UNITY_EDITOR_WIN
                ProcessHelper.Run("App.exe", arguments, "../Bin/");
#else
                ProcessHelper.Run("dotnet", $"App.dll {arguments}", "../Bin/");
#endif
            }

            if (GUILayout.Button("Start Watcher"))
            {
                string arguments = $"--AppType=Watcher --StartConfig=StartConfig/{this.startConfig} --Console=1 --Develop={(int)this.developMode} --LogLevel=1";
#if UNITY_EDITOR_WIN
                ProcessHelper.Run("App.exe", arguments, "../Bin/");
#else
                ProcessHelper.Run("dotnet", $"App.dll {arguments}", "../Bin/");
#endif
            }

            if (GUILayout.Button("Start Mongo"))
            {
                ProcessHelper.Run("mongod", @"--dbpath=db", "../Database/bin/");
            }

#if UNITY_EDITOR_WIN
            if (GUILayout.Button("启动机器人"))
            {
                StartRobotProcess();
            }
#endif
        }

#if UNITY_EDITOR_WIN
        private void StartRobotProcess()
        {
            string templatePath = Path.Combine(Application.dataPath, "Scripts/Editor/ServerCommandLineEditor/StartRobot.bat");
            // dataPath = .../Unity/Assets，Bin 在 .../ET-release8.1/Bin（与 ProcessHelper 的 ../Bin/ 一致）
            string binDir = Path.GetFullPath(Path.Combine(Application.dataPath, "../../Bin"));
            string batPath = Path.Combine(binDir, "StartRobot.bat");

            if (!File.Exists(templatePath))
            {
                EditorUtility.DisplayDialog("启动机器人", $"找不到模板: {templatePath}", "确定");
                return;
            }

            if (!Directory.Exists(binDir))
            {
                EditorUtility.DisplayDialog("启动机器人", $"请先编译服务端，目录不存在: {binDir}", "确定");
                return;
            }

            string batContent = File.ReadAllText(templatePath, Encoding.UTF8)
                    .Replace("__DEVELOP__", ((int)this.developMode).ToString())
                    .Replace("__STARTCONFIG__", $"StartConfig/{this.startConfig}");

            // CMD 默认 GBK，生成到 Bin 的 bat 必须用 GB936，否则中文 echo 乱码且可能被误解析成命令
            File.WriteAllText(batPath, batContent, Encoding.GetEncoding(936));

            // 用 cmd /k 打开独立控制台，避免 bat 双击/子进程时 stdin 异常
            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/k \"\"{batPath}\"\"",
                WorkingDirectory = binDir,
                UseShellExecute = true,
            });
        }
#endif
    }
}