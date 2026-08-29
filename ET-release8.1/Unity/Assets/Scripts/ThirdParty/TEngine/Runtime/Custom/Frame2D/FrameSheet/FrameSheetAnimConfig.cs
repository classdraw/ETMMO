using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    [Serializable]
    public class FrameSheetAnimClip
    {
        [Tooltip("动画名")]
        public FrameSheetAnimType animType;

        [Header("Rows By Facing 0=Bottom")]
        [Tooltip("朝下")]
        public int rowDown;

        [Tooltip("朝左")]
        public int rowLeft;

        [Tooltip("朝右")]
        public int rowRight;

        [Tooltip("朝上")]
        public int rowUp;

        [Header("Columns Shared By All Facings")]
        [Tooltip("起始列，0=最左第一帧")]
        public int startColumn;

        [Tooltip("结束列，0=最左；与 Shader End Column 一致")]
        public int endColumn;

        public bool loop = true;

        [Min(0.0001f)]
        public float interval = 0.1f;

        public int GetRow(FrameSheetFacing facing)
        {
            switch (facing)
            {
                case FrameSheetFacing.Left: return rowLeft;
                case FrameSheetFacing.Right: return rowRight;
                case FrameSheetFacing.Up: return rowUp;
                default: return rowDown;
            }
        }

        public void SetRow(FrameSheetFacing facing, int row)
        {
            switch (facing)
            {
                case FrameSheetFacing.Left: rowLeft = row; break;
                case FrameSheetFacing.Right: rowRight = row; break;
                case FrameSheetFacing.Up: rowUp = row; break;
                default: rowDown = row; break;
            }
        }
    }

    [CreateAssetMenu(fileName = "FrameSheetAnimConfig", menuName = "ET/Tools/FrameSheet Anim Config", order = 200)]
    public class FrameSheetAnimConfig : ScriptableObject
    {
        [Header("Grid Shared")]
        public int gridRows = 21;
        public int gridColumns = 13;

        [Header("Clips")]
        public List<FrameSheetAnimClip> clips = new List<FrameSheetAnimClip>();

        public bool TryGetClip(FrameSheetAnimType animType, out FrameSheetAnimClip clip)
        {
            for (int i = 0; i < clips.Count; i++)
            {
                if (clips[i].animType == animType)
                {
                    clip = clips[i];
                    return true;
                }
            }

            clip = null;
            return false;
        }

        public void ApplyClip(Material material, FrameSheetAnimType animType, FrameSheetFacing facing)
        {
            if (material == null || !TryGetClip(animType, out FrameSheetAnimClip clip))
            {
                return;
            }

            ApplyGrid(material);
            ApplyClip(material, clip, facing);
        }

        public void ApplyClip(MaterialPropertyBlock block, FrameSheetAnimType animType, FrameSheetFacing facing)
        {
            if (block == null || !TryGetClip(animType, out FrameSheetAnimClip clip))
            {
                return;
            }

            ApplyGrid(block);
            ApplyClip(block, clip, facing);
        }

        public void ApplyGrid(Material material)
        {
            material.SetFloat(FrameSheetAnimShaderIds.GridRows, gridRows);
            material.SetFloat(FrameSheetAnimShaderIds.GridColumns, gridColumns);
        }

        public void ApplyGrid(MaterialPropertyBlock block)
        {
            block.SetFloat(FrameSheetAnimShaderIds.GridRows, gridRows);
            block.SetFloat(FrameSheetAnimShaderIds.GridColumns, gridColumns);
        }

        public static void ApplyClip(Material material, FrameSheetAnimClip clip, FrameSheetFacing facing)
        {
            material.SetFloat(FrameSheetAnimShaderIds.Row, clip.GetRow(facing));
            material.SetFloat(FrameSheetAnimShaderIds.StartColumn, clip.startColumn);
            material.SetFloat(FrameSheetAnimShaderIds.EndColumn, clip.endColumn);
            material.SetFloat(FrameSheetAnimShaderIds.Loop, clip.loop ? 1f : 0f);
            material.SetFloat(FrameSheetAnimShaderIds.Interval, clip.interval);
        }

        public static void ApplyClip(MaterialPropertyBlock block, FrameSheetAnimClip clip, FrameSheetFacing facing)
        {
            block.SetFloat(FrameSheetAnimShaderIds.Row, clip.GetRow(facing));
            block.SetFloat(FrameSheetAnimShaderIds.StartColumn, clip.startColumn);
            block.SetFloat(FrameSheetAnimShaderIds.EndColumn, clip.endColumn);
            block.SetFloat(FrameSheetAnimShaderIds.Loop, clip.loop ? 1f : 0f);
            block.SetFloat(FrameSheetAnimShaderIds.Interval, clip.interval);
        }

        private void OnValidate()
        {
            gridRows = Mathf.Max(1, gridRows);
            gridColumns = Mathf.Max(1, gridColumns);

            for (int i = 0; i < clips.Count; i++)
            {
                FrameSheetAnimClip clip = clips[i];
                clip.rowDown = Mathf.Clamp(clip.rowDown, 0, gridRows - 1);
                clip.rowLeft = Mathf.Clamp(clip.rowLeft, 0, gridRows - 1);
                clip.rowRight = Mathf.Clamp(clip.rowRight, 0, gridRows - 1);
                clip.rowUp = Mathf.Clamp(clip.rowUp, 0, gridRows - 1);
                clip.startColumn = Mathf.Clamp(clip.startColumn, 0, gridColumns - 1);
                clip.endColumn = Mathf.Clamp(clip.endColumn, 0, gridColumns - 1);
                clip.interval = Mathf.Max(0.0001f, clip.interval);
            }
        }
    }

    public static class FrameSheetAnimShaderIds
    {
        public static readonly int GridRows = Shader.PropertyToID("_GridRows");
        public static readonly int GridColumns = Shader.PropertyToID("_GridColumns");
        public static readonly int Row = Shader.PropertyToID("_Row");
        public static readonly int StartColumn = Shader.PropertyToID("_StartColumn");
        public static readonly int EndColumn = Shader.PropertyToID("_EndColumn");
        public static readonly int Loop = Shader.PropertyToID("_Loop");
        public static readonly int Interval = Shader.PropertyToID("_Interval");
    }
}
