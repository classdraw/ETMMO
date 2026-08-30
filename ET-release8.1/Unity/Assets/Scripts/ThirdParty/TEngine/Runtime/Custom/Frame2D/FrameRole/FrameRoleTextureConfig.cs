using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    [Serializable]
    public class FrameRoleTextureEntry
    {
        [Tooltip("列表序号，0 起。参与 display 最后三位")]
        public int index;

        [Tooltip("自动生成：部位Key*10000000 + 种族Key*100000 + 性别Key*1000 + index")]
        public int displayId;

        [Tooltip("备注，方便对照 Excel / 资源名")]
        public string name;

        [Tooltip("描述，导出到 ExternalDisplayConfig 的 Desc 列")]
        public string desc;

        [Tooltip("贴图引用")]
        public Texture2D texture;
    }

    [Serializable]
    public class FrameRoleGenderGroup
    {
        [Tooltip("性别 Key，自行填写 int；0 表示所有性别均可")]
        public int genderKey;

        public List<FrameRoleTextureEntry> textures = new List<FrameRoleTextureEntry>();
    }

    [Serializable]
    public class FrameRoleRaceGroup
    {
        [Tooltip("种族 Key，自行填写 int；0 表示所有种族均可")]
        public int raceKey;

        public List<FrameRoleGenderGroup> genders = new List<FrameRoleGenderGroup>();
    }

    /// <summary>
    /// 单个部位的贴图分配表。body / head 等各建一份。
    /// Excel 配 display，运行时 <see cref="TryGetTexture"/> 直接取图。
    /// </summary>
    [CreateAssetMenu(fileName = "FrameRoleTexture_Body", menuName = "ET/Frame2D/Frame Role Texture Config", order = 210)]
    public class FrameRoleTextureConfig : ScriptableObject
    {
        [Tooltip("本文件对应的部位 Key，自行填写 int")]
        public int partKey;

        public List<FrameRoleRaceGroup> races = new List<FrameRoleRaceGroup>();

        [NonSerialized]
        private Dictionary<int, FrameRoleTextureEntry> lookup;

        public void RebuildDisplayIds()
        {
            for (int r = 0; r < races.Count; r++)
            {
                FrameRoleRaceGroup raceGroup = races[r];
                if (raceGroup == null || raceGroup.genders == null)
                {
                    continue;
                }

                for (int g = 0; g < raceGroup.genders.Count; g++)
                {
                    FrameRoleGenderGroup genderGroup = raceGroup.genders[g];
                    if (genderGroup == null || genderGroup.textures == null)
                    {
                        continue;
                    }

                    for (int i = 0; i < genderGroup.textures.Count; i++)
                    {
                        FrameRoleTextureEntry entry = genderGroup.textures[i];
                        if (entry == null)
                        {
                            continue;
                        }

                        entry.index = i;
                        if (string.IsNullOrEmpty(entry.name) && entry.texture != null)
                        {
                            entry.name = entry.texture.name;
                        }

                        entry.displayId = FrameRoleTextureDisplay.Encode(partKey, raceGroup.raceKey, genderGroup.genderKey, entry.index);
                    }
                }
            }

            lookup = null;
        }

        public void RebuildLookup()
        {
            if (lookup == null)
            {
                lookup = new Dictionary<int, FrameRoleTextureEntry>();
            }
            else
            {
                lookup.Clear();
            }

            for (int r = 0; r < races.Count; r++)
            {
                FrameRoleRaceGroup raceGroup = races[r];
                if (raceGroup?.genders == null)
                {
                    continue;
                }

                for (int g = 0; g < raceGroup.genders.Count; g++)
                {
                    FrameRoleGenderGroup genderGroup = raceGroup.genders[g];
                    if (genderGroup?.textures == null)
                    {
                        continue;
                    }

                    for (int i = 0; i < genderGroup.textures.Count; i++)
                    {
                        FrameRoleTextureEntry entry = genderGroup.textures[i];
                        if (entry == null)
                        {
                            continue;
                        }

                        lookup[entry.displayId] = entry;
                    }
                }
            }
        }

        public bool TryGetEntry(int displayId, out FrameRoleTextureEntry entry)
        {
            if (lookup == null)
            {
                RebuildLookup();
            }

            return lookup.TryGetValue(displayId, out entry);
        }

        public bool TryGetTexture(int displayId, out Texture2D texture)
        {
            if (TryGetEntry(displayId, out FrameRoleTextureEntry entry) && entry.texture != null)
            {
                texture = entry.texture;
                return true;
            }

            texture = null;
            return false;
        }

        public bool TryGetTexture(int raceKey, int genderKey, int index, out Texture2D texture)
        {
            return TryGetTexture(FrameRoleTextureDisplay.Encode(partKey, raceKey, genderKey, index), out texture);
        }

        public FrameRoleRaceGroup GetOrCreateRace(int raceKey)
        {
            for (int i = 0; i < races.Count; i++)
            {
                if (races[i] != null && races[i].raceKey == raceKey)
                {
                    return races[i];
                }
            }

            FrameRoleRaceGroup group = new FrameRoleRaceGroup { raceKey = raceKey };
            races.Add(group);
            return group;
        }

        public FrameRoleGenderGroup GetOrCreateGender(int raceKey, int genderKey)
        {
            FrameRoleRaceGroup raceGroup = GetOrCreateRace(raceKey);
            for (int i = 0; i < raceGroup.genders.Count; i++)
            {
                if (raceGroup.genders[i] != null && raceGroup.genders[i].genderKey == genderKey)
                {
                    return raceGroup.genders[i];
                }
            }

            FrameRoleGenderGroup group = new FrameRoleGenderGroup { genderKey = genderKey };
            raceGroup.genders.Add(group);
            return group;
        }

        private void OnEnable()
        {
            lookup = null;
        }

        private void OnValidate()
        {
            RebuildDisplayIds();
        }
    }
}
