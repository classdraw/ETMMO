using System;
using System.Collections.Generic;
using TEngine;
using TEngine.Localization;
using UnityEngine;
using I2 = TEngine.Localization.LocalizationManager;

namespace ET
{
    /// <summary>
    /// Init 场景 Inspector 调试项，启动时写入；HotfixView 在语言表加载后再应用。
    /// </summary>
    public static class ClientViewDebugSettings
    {
        /// <summary>I2 上次退出时记住的语言（优先级高于 Init 时会把界面拉回英文）。</summary>
        public const string I2SavedLanguageKey = "I2 Language";

        /// <summary>运行时内置语言表（YooAsset 地址，与 GameEntry Inner CSV 一致）。</summary>
        public const string RuntimeInnerCsvLocation = "Assets/Bundles/Tools/I2Localization/I2Languages.csv";

        /// <summary>头顶 HUD 是否用阵营 Id 代替角色名。</summary>
        public static bool ReplaceUnitNameWithFaction { get; set; }

        /// <summary>Init 场景指定的运行语言；Unspecified 时沿用 GameEntry RootModule / 系统语言 / I2 存档。</summary>
        public static Language RuntimeLanguage { get; set; } = Language.Unspecified;

        /// <summary>Init 指定语言时清除 I2 本地「I2 Language」，避免下次 Initialize 自动选英文。</summary>
        public static void ClearI2SavedLanguageWhenInitOverrides()
        {
            if (RuntimeLanguage == Language.Unspecified)
            {
                return;
            }

            PersistentStorage.DeleteSetting(I2SavedLanguageKey);
        }

        /// <summary>根据 Init 语言解析 I2 列名，如 Chinese → Chinese (Simplified)。</summary>
        public static string GetExpectedI2LanguageName()
        {
            if (RuntimeLanguage == Language.Unspecified)
            {
                return null;
            }

            string teName = LocalizationUtility.GetLanguageStr(RuntimeLanguage);
            return ResolveI2LanguageName(teName, RuntimeLanguage);
        }

        /// <summary>语言源就绪后强制当前 I2 语言（不被 SelectStartupLanguage / 存档盖掉）。</summary>
        public static bool TryForceI2CurrentLanguage()
        {
            if (RuntimeLanguage == Language.Unspecified)
            {
                return false;
            }

            if ((I2.Sources?.Count ?? 0) == 0)
            {
                return false;
            }

            string i2Name = GetExpectedI2LanguageName();
            if (string.IsNullOrEmpty(i2Name))
            {
                return false;
            }

            if (!I2.HasLanguage(i2Name, Initialize: true, SkipDisabled: true)
                && !I2.HasLanguage(i2Name, Initialize: false, SkipDisabled: true))
            {
                return false;
            }

            string languageCode = I2.GetLanguageCode(i2Name);
            I2.SetLanguageAndCode(i2Name, languageCode, RememberLanguage: false, Force: true);
            return string.Equals(I2.CurrentLanguage, i2Name, StringComparison.Ordinal);
        }

        /// <summary>GameEntry 实例化后、Localization Start 前写入 RootModule。</summary>
        public static void TryApplyRootModuleLanguage(GameObject gameEntry)
        {
            if (RuntimeLanguage == Language.Unspecified || gameEntry == null)
            {
                return;
            }

            RootModule rootModule = gameEntry.GetComponentInChildren<RootModule>(true);
            if (rootModule == null)
            {
                Log.Warning("Init 运行语言未应用：GameEntry 上未找到 RootModule。");
                return;
            }

            rootModule.EditorLanguage = RuntimeLanguage;
        }

        /// <summary>语言源已导入后切换 TEngine 组件（勿在模块注册前调用 ModuleSystem.GetModule）。</summary>
        public static bool TryApplyOnLocalizationManager(TEngine.LocalizationManager localizationManager, bool load = false)
        {
            if (RuntimeLanguage == Language.Unspecified || localizationManager == null)
            {
                return false;
            }

            return localizationManager.SetLanguage(RuntimeLanguage, load);
        }

        private static string ResolveI2LanguageName(string teName, Language runtimeLanguage)
        {
            if (string.IsNullOrEmpty(teName))
            {
                return null;
            }

            List<string> languages = I2.GetAllLanguages();
            if (languages != null && languages.Count > 0)
            {
                if (runtimeLanguage == Language.ChineseSimplified)
                {
                    string simplified = languages.Find(l =>
                        l.IndexOf("Simplified", StringComparison.OrdinalIgnoreCase) >= 0);
                    if (!string.IsNullOrEmpty(simplified))
                    {
                        return simplified;
                    }
                }

                if (runtimeLanguage == Language.ChineseTraditional)
                {
                    string traditional = languages.Find(l =>
                        l.IndexOf("Traditional", StringComparison.OrdinalIgnoreCase) >= 0);
                    if (!string.IsNullOrEmpty(traditional))
                    {
                        return traditional;
                    }
                }
            }

            string supported = I2.GetSupportedLanguage(teName);
            if (!string.IsNullOrEmpty(supported))
            {
                return supported;
            }

            if (languages != null && languages.Count > 0)
            {
                foreach (string name in languages)
                {
                    if (name.StartsWith(teName, StringComparison.OrdinalIgnoreCase)
                        || name.IndexOf(teName, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return name;
                    }
                }
            }

            // 语言表尚未导入时 I2.GetAllLanguages 为空，仍可按 Init 枚举给出 CSV 列名
            return GetStaticI2LanguageName(runtimeLanguage);
        }

        private static string GetStaticI2LanguageName(Language runtimeLanguage)
        {
            switch (runtimeLanguage)
            {
                case Language.ChineseSimplified:
                    return "Chinese (Simplified)";
                case Language.ChineseTraditional:
                    return "Chinese (Traditional)";
                case Language.English:
                    return "English";
                case Language.Japanese:
                    return "Japanese";
                case Language.Korean:
                    return "Korean";
                default:
                    return LocalizationUtility.GetLanguageStr(runtimeLanguage);
            }
        }
    }
}