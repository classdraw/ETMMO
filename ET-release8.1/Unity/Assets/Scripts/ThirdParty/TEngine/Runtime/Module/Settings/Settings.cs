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

        [SerializeField]
        private UpdateSetting updateSetting;

        public static AudioSetting AudioSetting => Instance.audioSetting;

        public static ProcedureSetting ProcedureSetting => Instance.procedureSetting;

        public static UpdateSetting UpdateSetting
        {
            get
            {
#if UNITY_EDITOR
                if (Instance != null && Instance.updateSetting != null)
                {
                    return Instance.updateSetting;
                }

                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:UpdateSetting");
                if (guids.Length >= 1)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                    return UnityEditor.AssetDatabase.LoadAssetAtPath<UpdateSetting>(path);
                }

                return null;
#else
                return Instance != null ? Instance.updateSetting : null;
#endif
            }
        }


        #region 用户设置的一些值

        protected virtual List<string> GetScreenResolutions() =>
                Screen.resolutions.Select(resolution => resolution.ToString()).ToList();

        protected virtual List<string> GetRenderingResolutions() =>
                System.Enum.GetNames(typeof(ResolutionOption)).ToList();
        

        #endregion

    }
}