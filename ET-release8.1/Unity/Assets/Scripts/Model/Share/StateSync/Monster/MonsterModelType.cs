namespace ET
{
    /// <summary>
    /// 怪物模型加载方式，对应 MonsterConfig.ModelType（仅整模 prefab）。
    /// </summary>
    public enum MonsterModelType
    {
        /// <summary>
        /// Model 为 prefab 名称（不含路径与后缀），加载 Assets/Bundles/Unit/{Model}.prefab。
        /// </summary>
        Prefab = 1,
    }
}
