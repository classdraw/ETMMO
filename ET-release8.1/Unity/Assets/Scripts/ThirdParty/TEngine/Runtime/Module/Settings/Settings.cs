using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TEngine
{
    public enum ResolutionOption
    {
        Lowest,
        Low,
        Medium,
        High,
        Maximum,
    }
    
    public class Settings : MonoBehaviour
    {
        private static Settings _instance;

        public static Settings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Utility.Unity.FindObjectOfType<Settings>();

                    if (_instance != null)
                    {
                        return _instance;
                    }
                }

                return _instance;
            }
        }

        [SerializeField]
        private AudioSetting audioSetting;

        [SerializeField]
        private ProcedureSetting procedureSetting;


        public static AudioSetting AudioSetting => Instance.audioSetting;

        public static ProcedureSetting ProcedureSetting => Instance.procedureSetting;


        #region 用户设置的一些值

        protected virtual List<string> GetScreenResolutions() =>
                Screen.resolutions.Select(resolution => resolution.ToString()).ToList();

        protected virtual List<string> GetRenderingResolutions() =>
                System.Enum.GetNames(typeof(ResolutionOption)).ToList();
        

        #endregion

    }
}