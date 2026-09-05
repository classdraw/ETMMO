namespace ET
{
    /// <summary>
    /// 动画 Clip 切换时，按槽位映射刷新部位贴图。
    /// 由 RedressAvatar 等组件实现。
    /// </summary>
    public interface IFrameSheetClipTextureRouter
    {
        void ApplyClipTextureRouting(FrameSheetAnimClip clip, FrameSheetFacing facing);
    }
}
