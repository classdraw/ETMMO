using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    /// <summary>
    /// 与业务 BindBoneType 数值一致，供帧动画骨骼偏移配置使用（Unity.TEngine 程序集内）。
    /// </summary>
    public enum FrameAnimBindBoneType : byte
    {
        Body = 0,
        Head = 1,
        Foot = 2,
        LeftHand = 3,
        RightHand = 4,
    }

    /// <summary>
    /// 单帧下单个骨骼 Transform.localPosition（仅使用 X/Z，Y 恒为 0）。
    /// </summary>
    [Serializable]
    public struct FrameAnimBoneFrameData
    {
        [Tooltip("骨骼 localPosition（Unity 单位，Y 不使用）")]
        public Vector3 localPosition;
    }

    /// <summary>
    /// 单帧下 Graphics 显示 mesh 节点的 Transform（与骨骼同级，独立存储）。
    /// </summary>
    [Serializable]
    public struct FrameAnimMeshFrameData
    {
        [Tooltip("Graphics.localPosition.x / localPosition.z")]
        public Vector2 localPositionXZ;

        [Tooltip("Graphics.localScale.x / localScale.z")]
        public Vector2 localScaleXZ;

        public Vector3 ToLocalPosition()
        {
            return new Vector3(localPositionXZ.x, 0f, localPositionXZ.y);
        }

        public Vector3 ToLocalScale()
        {
            float sx = localScaleXZ.x <= 0f ? 1f : localScaleXZ.x;
            float sz = localScaleXZ.y <= 0f ? 1f : localScaleXZ.y;
            return new Vector3(sx, 1f, sz);
        }

        public static FrameAnimMeshFrameData Default => new FrameAnimMeshFrameData
        {
            localPositionXZ = Vector2.zero,
            localScaleXZ = Vector2.one,
        };
    }

    /// <summary>
    /// 单个骨骼在整段动画各帧上的位置轨迹。
    /// </summary>
    [Serializable]
    public class FrameAnimBoneTrack
    {
        public FrameAnimBindBoneType boneType = FrameAnimBindBoneType.Head;

        [Tooltip("每一帧的骨骼 localPosition（Unity 单位，与 Graphics 同级，不含 mesh 整体变换）")]
        public List<FrameAnimBoneFrameData> framePositions = new List<FrameAnimBoneFrameData>();
    }

    /// <summary>
    /// 帧动画骨骼 + Graphics mesh 配置：骨骼与显示 mesh 为同级节点，各自独立逐帧 Transform。
    /// </summary>
    [CreateAssetMenu(fileName = "FrameAnimBoneConfig", menuName = "Tools/Frame2D/Frame Anim Bone Config", order = 201)]
    public class FrameAnimBoneConfig : ScriptableObject
    {
        [Min(1)]
        [Tooltip("动画帧数")]
        public int frameCount = 4;

        [Tooltip("每帧 Graphics mesh 节点的 localPosition(X/Z) 与 localScale(X/Z)，与 boneTracks 同帧序")]
        public List<FrameAnimMeshFrameData> frameMeshFrames = new List<FrameAnimMeshFrameData>();

        [Tooltip("各骨骼的逐帧 localPosition")]
        public List<FrameAnimBoneTrack> boneTracks = new List<FrameAnimBoneTrack>();

        public bool TryGetTrack(FrameAnimBindBoneType boneType, out FrameAnimBoneTrack track)
        {
            for (int i = 0; i < boneTracks.Count; i++)
            {
                if (boneTracks[i].boneType == boneType)
                {
                    track = boneTracks[i];
                    return true;
                }
            }

            track = null;
            return false;
        }

        public bool TryGetFramePosition(FrameAnimBindBoneType boneType, int frameIndex, out Vector3 localPosition)
        {
            localPosition = Vector3.zero;
            if (frameIndex < 0 || frameIndex >= frameCount || !TryGetTrack(boneType, out FrameAnimBoneTrack track))
            {
                return false;
            }

            if (frameIndex >= track.framePositions.Count)
            {
                return false;
            }

            localPosition = track.framePositions[frameIndex].localPosition;
            return true;
        }

        public bool TryGetFrameMeshTransform(int frameIndex, out FrameAnimMeshFrameData meshFrame)
        {
            meshFrame = FrameAnimMeshFrameData.Default;
            if (frameIndex < 0 || frameIndex >= frameCount)
            {
                return false;
            }

            if (frameMeshFrames == null || frameIndex >= frameMeshFrames.Count)
            {
                return true;
            }

            meshFrame = frameMeshFrames[frameIndex];
            return true;
        }

        public void EnsureTrackFrameCount()
        {
            frameCount = Mathf.Max(1, frameCount);

            if (frameMeshFrames == null)
            {
                frameMeshFrames = new List<FrameAnimMeshFrameData>();
            }

            while (frameMeshFrames.Count < frameCount)
            {
                frameMeshFrames.Add(FrameAnimMeshFrameData.Default);
            }

            while (frameMeshFrames.Count > frameCount)
            {
                frameMeshFrames.RemoveAt(frameMeshFrames.Count - 1);
            }

            for (int i = 0; i < boneTracks.Count; i++)
            {
                FrameAnimBoneTrack track = boneTracks[i];
                if (track.framePositions == null)
                {
                    track.framePositions = new List<FrameAnimBoneFrameData>();
                }

                while (track.framePositions.Count < frameCount)
                {
                    track.framePositions.Add(new FrameAnimBoneFrameData());
                }

                while (track.framePositions.Count > frameCount)
                {
                    track.framePositions.RemoveAt(track.framePositions.Count - 1);
                }
            }
        }

        public FrameAnimBoneTrack GetOrCreateTrack(FrameAnimBindBoneType boneType)
        {
            if (TryGetTrack(boneType, out FrameAnimBoneTrack existing))
            {
                return existing;
            }

            FrameAnimBoneTrack track = new FrameAnimBoneTrack
            {
                boneType = boneType,
            };
            boneTracks.Add(track);
            EnsureTrackFrameCount();
            return track;
        }

        private void OnValidate()
        {
            EnsureTrackFrameCount();
        }
    }
}
