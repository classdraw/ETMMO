namespace ET
{
    /// <summary>
    /// 怪物模型加载方式，对应 MonsterConfig.ModelType。
    /// </summary>
    public enum MonsterModelType
    {
        /// <summary>
        /// 与角色相同：加载 Monster.prefab 骨架，Model 为 Body,Head,Tail,Shirt,Pants 逗号分隔 DisplayId。
        /// </summary>
        PartAssembly = 0,

        /// <summary>
        /// Model 为 prefab 名称（不含路径与后缀），直接加载 Assets/Bundles/Unit/{Model}.prefab。
        /// 预制体已包含完整贴图/材质，与 PartAssembly 共用 FrameSheetAnimPlayer 等脚本，但不拼装部件。
        /// </summary>
        Prefab = 1,
    }
}
