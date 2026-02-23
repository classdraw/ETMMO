using System;
using UnityEngine;
using TEngine;

namespace ET
{
    /// <summary>
    /// 游戏全局配置类，模仿RootModule，只实现部分核心属性
    /// </summary>
    
    [EnableClass]
    [DisallowMultipleComponent]
    public class TEngineGlobal : MonoBehaviour
    {
        
        [StaticField]
        private static TEngineGlobal _instance = null;

        public static TEngineGlobal Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Utility.Unity.FindObjectOfType<TEngineGlobal>();
                }
                return _instance;
            }
        }

        [SerializeField]
        private string textHelperTypeName = "TEngine.DefaultTextHelper";

        [SerializeField]
        private string logHelperTypeName = "TEngine.DefaultLogHelper";

        [SerializeField]
        private string jsonHelperTypeName = "TEngine.DefaultJsonHelper";

        [SerializeField]
        private int frameRate = 120;

        [SerializeField]
        private float gameSpeed = 1f;

        [SerializeField]
        private bool runInBackground = true;

        [SerializeField]
        private bool neverSleep = true;

        /// <summary>
        /// 获取或设置文本辅助器类型名称。
        /// </summary>
        public string TextHelperTypeName
        {
            get => textHelperTypeName;
            set => textHelperTypeName = value;
        }

        /// <summary>
        /// 获取或设置日志辅助器类型名称。
        /// </summary>
        public string LogHelperTypeName
        {
            get => logHelperTypeName;
            set => logHelperTypeName = value;
        }

        /// <summary>
        /// 获取或设置JSON辅助器类型名称。
        /// </summary>
        public string JsonHelperTypeName
        {
            get => jsonHelperTypeName;
            set => jsonHelperTypeName = value;
        }

        /// <summary>
        /// 获取或设置游戏帧率。
        /// </summary>
        public int FrameRate
        {
            get => frameRate;
            set => Application.targetFrameRate = frameRate = value;
        }

        /// <summary>
        /// 获取或设置游戏速度。
        /// </summary>
        public float GameSpeed
        {
            get => gameSpeed;
            set => Time.timeScale = gameSpeed = value >= 0f ? value : 0f;
        }

        /// <summary>
        /// 获取或设置是否允许后台运行。
        /// </summary>
        public bool RunInBackground
        {
            get => runInBackground;
            set => Application.runInBackground = runInBackground = value;
        }

        /// <summary>
        /// 获取或设置是否禁止休眠。
        /// </summary>
        public bool NeverSleep
        {
            get => neverSleep;
            set
            {
                neverSleep = value;
                Screen.sleepTimeout = value ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;
            }
        }

        /// <summary>
        /// 游戏全局配置初始化。
        /// </summary>
        private void Awake()
        {
            
            _instance = this;
           
            InitTextHelper();
            InitLogHelper();
            InitJsonHelper();

            Application.targetFrameRate = frameRate;
            Time.timeScale = gameSpeed;
            Application.runInBackground = runInBackground;
            Screen.sleepTimeout = neverSleep ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;

            GameFrameworkLog.Info("TEngine Awake Success!!!");
        }

        private void InitTextHelper()
        {
            if (string.IsNullOrEmpty(textHelperTypeName))
            {
                return;
            }

            Type textHelperType = Utility.Assembly.GetType(textHelperTypeName);
            if (textHelperType == null)
            {
                //Log.Error("Can not find text helper type '{0}'.", textHelperTypeName);
                return;
            }

            Utility.Text.ITextHelper textHelper = (Utility.Text.ITextHelper)Activator.CreateInstance(textHelperType);
            if (textHelper == null)
            {
                //Log.Error("Can not create text helper instance '{0}'.", textHelperTypeName);
                return;
            }

            Utility.Text.SetTextHelper(textHelper);
        }

        private void InitLogHelper()
        {
            if (string.IsNullOrEmpty(logHelperTypeName))
            {
                return;
            }

            Type logHelperType = Utility.Assembly.GetType(logHelperTypeName);
            if (logHelperType == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find log helper type '{0}'.", logHelperTypeName));
            }

            GameFrameworkLog.ILogHelper logHelper = (GameFrameworkLog.ILogHelper)Activator.CreateInstance(logHelperType);
            if (logHelper == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not create log helper instance '{0}'.", logHelperTypeName));
            }

            GameFrameworkLog.SetLogHelper(logHelper);


        }

        private void InitJsonHelper()
        {
            if (string.IsNullOrEmpty(jsonHelperTypeName))
            {
                return;
            }

            Type jsonHelperType = Utility.Assembly.GetType(jsonHelperTypeName);
            if (jsonHelperType == null)
            {
                //Log.Error("Can not find JSON helper type '{0}'.", jsonHelperTypeName);
                return;
            }

            Utility.Json.IJsonHelper jsonHelper = (Utility.Json.IJsonHelper)Activator.CreateInstance(jsonHelperType);
            if (jsonHelper == null)
            {
                //Log.Error("Can not create JSON helper instance '{0}'.", jsonHelperTypeName);
                return;
            }

            Utility.Json.SetJsonHelper(jsonHelper);
        }

        private void Update()
        {

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.LogError("11111111111111");
                Debug.Log("2222222222222");
            }
        }
    }
}
