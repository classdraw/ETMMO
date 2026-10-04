namespace ET
{
    /// <summary>
    /// Init 场景 Inspector 调试项，启动时写入；HotfixView 只读。
    /// </summary>
    public static class ClientViewDebugSettings
    {
        /// <summary>头顶 HUD 是否用阵营 Id 代替角色名。</summary>
        public static bool ReplaceUnitNameWithFaction { get; set; }
    }
}
