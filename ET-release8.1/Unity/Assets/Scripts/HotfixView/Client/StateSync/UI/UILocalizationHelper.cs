using System;
using System.Collections.Generic;
using System.Reflection;
using TEngine;
using TEngine.Localization;
using UnityEngine;
using YooAsset;
#if UNITY_EDITOR
using UnityEditor;
#endif
using I2 = TEngine.Localization.LocalizationManager;

namespace ET.Client
{
    /// <summary>客户端 UI：读取 I2 语言包词条。</summary>
    public static class UILocalizationHelper
    {
        public const string EquipmentLevelTerm = "ui_EquipmentLevel";
        public const string EquipmentLastPointsTerm = "ui_EquipmentLastPoints";

        /// <summary>手动调试：Console 搜 UIL10n。</summary>
        public static void LogTermDiagnostics(string term, params object[] formatArgs)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                Log.Warning("[UIL10n] LogTermDiagnostics: term 为空");
                return;
            }

            EnsureLocalizationReady(out _, out _);

            string key = term.Trim();
            int sourceCount = I2.Sources?.Count ?? 0;
            string lang = I2.CurrentLanguage ?? "(null)";

            I2.TryGetTranslation(key, out string rawFromI2);
            string final = GetTerm(key, $"<!missing:{key}>", formatArgs);

            Log.Info(
                $"[UIL10n] term={key} | lang={lang} | sources={sourceCount} | " +
                $"raw=\"{rawFromI2 ?? "(null)"}\" | result=\"{final}\"");
        }

        public static bool IsTermResolvable(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return false;
            }

            EnsureLocalizationReady(out _, out _);

            string expectedI2 = ClientViewDebugSettings.GetExpectedI2LanguageName();
            if (!string.IsNullOrEmpty(expectedI2)
                && !string.Equals(I2.CurrentLanguage, expectedI2, StringComparison.Ordinal))
            {
                return false;
            }

            return TryGetTranslationText(term.Trim(), out _);
        }

        public static string GetTerm(string term, string fallback = null, params object[] args)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return ApplyParameters(fallback ?? string.Empty, args);
            }

            EnsureLocalizationReady(out _, out _);

            string key = term.Trim();
            bool resolvedFromTable = TryGetTranslationText(key, out string tableText);
            string text = resolvedFromTable ? tableText : (fallback ?? key);
            return ApplyParameters(text, args);
        }

        public static string GetTerm(string term, string fallback, IReadOnlyDictionary<string, object> namedParameters)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return fallback ?? string.Empty;
            }

            EnsureLocalizationReady(out _, out _);

            string key = term.Trim();
            string text = TryResolveTranslation(key);
            if (string.IsNullOrEmpty(text))
            {
                text = fallback ?? key;
            }

            if (namedParameters == null || namedParameters.Count == 0)
            {
                return text;
            }

            var dict = namedParameters as Dictionary<string, object> ?? new Dictionary<string, object>(namedParameters);
            I2.ApplyLocalizationParams(ref text, dict);
            return text;
        }

        public static void EnsureLocalizationReadyForInit(out bool changed, out string note)
        {
            EnsureLocalizationReady(out changed, out note);
        }

        private static void EnsureLocalizationReady(out bool changed, out string note)
        {
            changed = false;
            note = "unchanged";
            int sourcesBefore = I2.Sources?.Count ?? 0;

            if (sourcesBefore > 0)
            {
                ClientViewDebugSettings.TryForceI2CurrentLanguage();
            }

            if (TryRepairEmptyI2LanguageTable())
            {
                changed = true;
                note = "RepairI2Csv";
            }

            I2.InitializeIfNeeded();

#if UNITY_EDITOR
            if (Application.isPlaying && sourcesBefore == 0)
            {
                I2.RegisterSourceInEditor();
                if ((I2.Sources?.Count ?? 0) > sourcesBefore)
                {
                    changed = true;
                    note = "RegisterSourceInEditor";
                }
            }
#endif

            if (ClientViewDebugSettings.RuntimeLanguage == Language.Unspecified
                && (I2.Sources?.Count ?? 0) > 0
                && string.IsNullOrEmpty(I2.CurrentLanguage))
            {
                List<string> languages = I2.GetAllLanguages();
                if (languages != null && languages.Count > 0)
                {
                    string preferred = languages.Find(l => l.StartsWith("Chinese", StringComparison.Ordinal))
                                        ?? languages[0];
                    I2.CurrentLanguage = preferred;
                    changed = true;
                    note = "SetCurrentLanguage";
                }
            }
            else if (ClientViewDebugSettings.TryForceI2CurrentLanguage())
            {
                changed = true;
                note = "SyncInitLang";
            }

            if (changed)
            {
                I2.InitializeIfNeeded();
            }
        }

        private static bool TryRepairEmptyI2LanguageTable()
        {
            int sourceCount = I2.Sources?.Count ?? 0;
            if (sourceCount == 0)
            {
                return false;
            }

            List<string> languages = I2.GetAllLanguages();
            if (languages != null && languages.Count > 0)
            {
                return false;
            }

            TextAsset csv = ResolveInnerLocalizationCsvAsset();
            if (csv == null || string.IsNullOrEmpty(csv.text))
            {
                Log.Warning(
                    $"[UIL10n] 语言源无列数据且无法加载 CSV：{ClientViewDebugSettings.RuntimeInnerCsvLocation}，请做多语言同步并构建 DefaultPackage。");
                return false;
            }

            LanguageSourceData sourceData = I2.Sources[0];
            sourceData.Import_I2CSV(string.Empty, csv.text, eSpreadsheetUpdateMode.Merge);
            I2.UpdateSources();
            ClientViewDebugSettings.TryForceI2CurrentLanguage();
            return true;
        }

        private static TextAsset ResolveInnerLocalizationCsvAsset()
        {
            TEngine.LocalizationManager teLoc = UnityEngine.Object.FindObjectOfType<TEngine.LocalizationManager>();
            if (teLoc != null)
            {
                FieldInfo field = typeof(TEngine.LocalizationManager).GetField(
                    "innerLocalizationCsv",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                if (field?.GetValue(teLoc) is TextAsset fromComponent && fromComponent.text.Length > 0)
                {
                    return fromComponent;
                }
            }

#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                TextAsset editorPath = AssetDatabase.LoadAssetAtPath<TextAsset>(ClientViewDebugSettings.RuntimeInnerCsvLocation);
                if (editorPath != null)
                {
                    return editorPath;
                }
            }
#endif

            ResourcePackage package = YooAssets.GetPackage("DefaultPackage");
            if (package == null)
            {
                return null;
            }

            AssetHandle handle = package.LoadAssetSync<TextAsset>(ClientViewDebugSettings.RuntimeInnerCsvLocation);
            return handle.AssetObject as TextAsset;
        }

        private static string TryResolveTranslation(string key)
        {
            EnsureLocalizationReady(out _, out _);

            if (TryGetTranslationText(key, out string translation))
            {
                return translation;
            }

            return null;
        }

        private static bool TryGetTranslationText(string key, out string translation)
        {
            translation = null;
            if (!I2.TryGetTranslation(key, out string text) || string.IsNullOrEmpty(text))
            {
                return false;
            }

            if (string.Equals(text, key, StringComparison.Ordinal))
            {
                return false;
            }

            translation = text;
            return true;
        }

        private static string ApplyParameters(string text, object[] args)
        {
            if (string.IsNullOrEmpty(text) || args == null || args.Length == 0)
            {
                return text ?? string.Empty;
            }

            try
            {
                return string.Format(text, args);
            }
            catch (FormatException)
            {
                return text;
            }
        }
    }
}