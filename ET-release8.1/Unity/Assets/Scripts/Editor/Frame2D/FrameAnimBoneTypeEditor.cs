using ET;
using UnityEditor;
using UnityEngine;

namespace ET.Editor.Frame2D
{
    /// <summary>
    /// 编辑器侧骨骼枚举别名，与运行时 <see cref="FrameAnimBindBoneType"/> / 业务 <see cref="BindBoneType"/> 数值一致。
    /// </summary>
    public enum BindBoneTypeEditor : byte
    {
        Body = 0,
        Head = 1,
        Foot = 2,
        LeftHand = 3,
        RightHand = 4,
    }

    public static class FrameAnimBoneTypeConverter
    {
        public static FrameAnimBindBoneType ToRuntime(BindBoneTypeEditor boneType)
        {
            return (FrameAnimBindBoneType)(byte)boneType;
        }

        public static BindBoneTypeEditor ToEditor(FrameAnimBindBoneType boneType)
        {
            return (BindBoneTypeEditor)(byte)boneType;
        }

        public static FrameAnimBindBoneType FromBindBoneType(BindBoneType boneType)
        {
            return (FrameAnimBindBoneType)(byte)boneType;
        }
    }
}
