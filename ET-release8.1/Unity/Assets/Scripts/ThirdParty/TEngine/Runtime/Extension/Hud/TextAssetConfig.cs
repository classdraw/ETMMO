using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using XEngine.Utilities;
using TEngine;

[CreateAssetMenu(fileName = "TextAssetConfig", menuName = "Scriptable Objects/TextAssetConfig", order = 7)]
public class TextAssetConfig : ScriptableObject
{
    public List<TextAssetData> m_kTextAssets = new List<TextAssetData>();
    public Dictionary<string, TextAssetData> m_kDatas = new Dictionary<string, TextAssetData>();

    public void Init()
    {
        m_kDatas.Clear();
        for (int i = 0; i < m_kTextAssets.Count; i++)
        {
            var config = m_kTextAssets[i];
            if (m_kDatas.ContainsKey(config.m_Name))
            {
                m_kDatas[config.m_Name] = config;
            }
            else
            {
                m_kDatas.Add(config.m_Name, config);
            }
        }
        Log.Debug("AudioClipConfig Config:" + m_kDatas.Count);
    }

    public TextAssetData GetDataByKey(string key){
        if(m_kDatas.ContainsKey(key)){
            return m_kDatas[key];
        }
        Log.Error("TextAssetConfig GetDataByKey Error!!!"+key);
        return null;
    }
}


[Serializable]
public class TextAssetData
{
    public string m_Name;
    public TextAsset m_kTextAsset;
}
