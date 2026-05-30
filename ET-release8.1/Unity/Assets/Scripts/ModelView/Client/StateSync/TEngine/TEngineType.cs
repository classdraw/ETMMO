using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    public enum Setting_Key_Enum
    {
        PostProcessingKey = 0,
        MusicVolumeKey = 1,
        EffectsVolumeKey = 2,
        Count
    }

    public enum SettingValueType
    {
        Bool = 0,
        Int = 1,
        Float = 2,
    }

    /// <summary>
    /// 单项设置配置：值类型 + 对应类型的默认值。
    /// </summary>
    public readonly struct SettingConfig
    {
        public SettingValueType ValueType { get; }
        public bool DefaultBool { get; }
        public int DefaultInt { get; }
        public float DefaultFloat { get; }

        private SettingConfig(SettingValueType valueType, bool defaultBool, int defaultInt, float defaultFloat)
        {
            ValueType = valueType;
            DefaultBool = defaultBool;
            DefaultInt = defaultInt;
            DefaultFloat = defaultFloat;
        }

        public static SettingConfig CreateBool(bool defaultValue = false)
        {
            return new SettingConfig(SettingValueType.Bool, defaultValue, 0, 0f);
        }

        public static SettingConfig CreateInt(int defaultValue = 0)
        {
            return new SettingConfig(SettingValueType.Int, false, defaultValue, 0f);
        }

        public static SettingConfig CreateFloat(float defaultValue = 0f)
        {
            return new SettingConfig(SettingValueType.Float, false, 0, defaultFloat: defaultValue);
        }
    }

    public static class SettingHelper
    {
        [StaticField]
        private static readonly Dictionary<Setting_Key_Enum, SettingConfig> ConfigTable = new Dictionary<Setting_Key_Enum, SettingConfig>
        {
            { Setting_Key_Enum.PostProcessingKey, SettingConfig.CreateBool(true) },
            { Setting_Key_Enum.MusicVolumeKey, SettingConfig.CreateFloat(1f) },
            { Setting_Key_Enum.EffectsVolumeKey, SettingConfig.CreateFloat(1f) },
        };

        public static bool IsSettingKey(Setting_Key_Enum key)
        {
            return key >= Setting_Key_Enum.PostProcessingKey && key < Setting_Key_Enum.Count;
        }

        public static IEnumerable<Setting_Key_Enum> GetAllKeys()
        {
            for (Setting_Key_Enum key = Setting_Key_Enum.PostProcessingKey; key < Setting_Key_Enum.Count; key++)
            {
                yield return key;
            }
        }

        public static bool TryGetConfig(Setting_Key_Enum key, out SettingConfig config)
        {
            if (!IsSettingKey(key))
            {
                config = default;
                return false;
            }

            return ConfigTable.TryGetValue(key, out config);
        }

        public static SettingConfig GetConfig(Setting_Key_Enum key)
        {
            if (!TryGetConfig(key, out SettingConfig config))
            {
                throw new KeyNotFoundException($"Setting config not found: {key}");
            }

            return config;
        }

        public static string GetPrefsKey(Setting_Key_Enum key)
        {
            return key.ToString();
        }

        public static bool LoadBool(Setting_Key_Enum key)
        {
            SettingConfig config = GetConfig(key);
            if (config.ValueType != SettingValueType.Bool)
            {
                Log.Error($"Setting {key} is not bool, type={config.ValueType}");
                return config.DefaultBool;
            }

            return PlayerPrefs.GetInt(GetPrefsKey(key), config.DefaultBool ? 1 : 0) != 0;
        }

        public static int LoadInt(Setting_Key_Enum key)
        {
            SettingConfig config = GetConfig(key);
            if (config.ValueType != SettingValueType.Int)
            {
                Log.Error($"Setting {key} is not int, type={config.ValueType}");
                return config.DefaultInt;
            }

            return PlayerPrefs.GetInt(GetPrefsKey(key), config.DefaultInt);
        }

        public static float LoadFloat(Setting_Key_Enum key)
        {
            SettingConfig config = GetConfig(key);
            switch (config.ValueType)
            {
                case SettingValueType.Bool:
                    return LoadBool(key) ? 1f : 0f;
                case SettingValueType.Int:
                    return LoadInt(key);
                case SettingValueType.Float:
                    return PlayerPrefs.GetFloat(GetPrefsKey(key), config.DefaultFloat);
                default:
                    return 0f;
            }
        }

        public static void SaveBool(Setting_Key_Enum key, bool value)
        {
            SettingConfig config = GetConfig(key);
            if (config.ValueType != SettingValueType.Bool)
            {
                Log.Error($"Setting {key} is not bool, type={config.ValueType}");
                return;
            }

            PlayerPrefs.SetInt(GetPrefsKey(key), value ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static void SaveInt(Setting_Key_Enum key, int value)
        {
            SettingConfig config = GetConfig(key);
            if (config.ValueType != SettingValueType.Int)
            {
                Log.Error($"Setting {key} is not int, type={config.ValueType}");
                return;
            }

            PlayerPrefs.SetInt(GetPrefsKey(key), value);
            PlayerPrefs.Save();
        }

        public static void SaveFloat(Setting_Key_Enum key, float value)
        {
            SettingConfig config = GetConfig(key);
            if (config.ValueType != SettingValueType.Float)
            {
                Log.Error($"Setting {key} is not float, type={config.ValueType}");
                return;
            }

            PlayerPrefs.SetFloat(GetPrefsKey(key), value);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 按配置表类型写入 PlayerPrefs。
        /// </summary>
        public static void Save(Setting_Key_Enum key, float value)
        {
            SettingConfig config = GetConfig(key);
            switch (config.ValueType)
            {
                case SettingValueType.Bool:
                    SaveBool(key, value >= 0.5f);
                    break;
                case SettingValueType.Int:
                    SaveInt(key, Mathf.RoundToInt(value));
                    break;
                case SettingValueType.Float:
                    SaveFloat(key, value);
                    break;
            }
        }

        /// <summary>
        /// 按配置表类型保存设置，并返回写入 SettingValues 用的 float 缓存值。
        /// </summary>
        public static bool TrySave(Setting_Key_Enum key, object value, out float cacheValue)
        {
            cacheValue = 0f;
            if (!TryGetConfig(key, out SettingConfig config))
            {
                Log.Error($"Setting config not found: {key}");
                return false;
            }

            if (value == null)
            {
                Log.Error($"Setting {key} save value is null");
                return false;
            }

            switch (config.ValueType)
            {
                case SettingValueType.Bool:
                {
                    if (!TryConvertToBool(value, out bool boolValue))
                    {
                        Log.Error($"Setting {key} expects bool, value type={value.GetType().Name}");
                        return false;
                    }

                    SaveBool(key, boolValue);
                    cacheValue = boolValue ? 1f : 0f;
                    return true;
                }
                case SettingValueType.Int:
                {
                    if (!TryConvertToInt(value, out int intValue))
                    {
                        Log.Error($"Setting {key} expects int, value type={value.GetType().Name}");
                        return false;
                    }

                    SaveInt(key, intValue);
                    cacheValue = intValue;
                    return true;
                }
                case SettingValueType.Float:
                {
                    if (!TryConvertToFloat(value, out float floatValue))
                    {
                        Log.Error($"Setting {key} expects float, value type={value.GetType().Name}");
                        return false;
                    }

                    SaveFloat(key, floatValue);
                    cacheValue = floatValue;
                    return true;
                }
                default:
                    return false;
            }
        }

        private static bool TryConvertToBool(object value, out bool result)
        {
            switch (value)
            {
                case bool boolValue:
                    result = boolValue;
                    return true;
                case int intValue:
                    result = intValue != 0;
                    return true;
                case float floatValue:
                    result = floatValue >= 0.5f;
                    return true;
                case double doubleValue:
                    result = doubleValue >= 0.5d;
                    return true;
                default:
                    result = false;
                    return false;
            }
        }

        private static bool TryConvertToInt(object value, out int result)
        {
            switch (value)
            {
                case int intValue:
                    result = intValue;
                    return true;
                case float floatValue:
                    result = Mathf.RoundToInt(floatValue);
                    return true;
                case double doubleValue:
                    result = (int)System.Math.Round(doubleValue);
                    return true;
                case bool boolValue:
                    result = boolValue ? 1 : 0;
                    return true;
                default:
                    result = 0;
                    return false;
            }
        }

        private static bool TryConvertToFloat(object value, out float result)
        {
            switch (value)
            {
                case float floatValue:
                    result = floatValue;
                    return true;
                case double doubleValue:
                    result = (float)doubleValue;
                    return true;
                case int intValue:
                    result = intValue;
                    return true;
                case bool boolValue:
                    result = boolValue ? 1f : 0f;
                    return true;
                default:
                    result = 0f;
                    return false;
            }
        }

        /// <summary>
        /// 若本地无记录则写入默认值。
        /// </summary>
        public static void EnsureDefault(Setting_Key_Enum key)
        {
            if (PlayerPrefs.HasKey(GetPrefsKey(key)))
            {
                return;
            }

            SettingConfig config = GetConfig(key);
            switch (config.ValueType)
            {
                case SettingValueType.Bool:
                    SaveBool(key, config.DefaultBool);
                    break;
                case SettingValueType.Int:
                    SaveInt(key, config.DefaultInt);
                    break;
                case SettingValueType.Float:
                    SaveFloat(key, config.DefaultFloat);
                    break;
            }
        }
    }
}
