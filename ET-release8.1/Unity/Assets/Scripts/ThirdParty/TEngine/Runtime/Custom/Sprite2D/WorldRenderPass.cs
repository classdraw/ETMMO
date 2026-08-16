/// <summary>
/// Broad render passes. Configure identically named Sorting Layers in Unity (bottom to top):
/// Ground -> GroundFront -> World -> WorldFront -> SceneVfx.
/// </summary>
public enum WorldRenderPass
{
    Ground,
    GroundFront,
    World,
    WorldFront,
    SceneVfx
}

public static class WorldRenderPassUtility
{
    public static string GetSortingLayerName(WorldRenderPass pass)
    {
        return pass.ToString();
    }
}
