namespace ET
{
    /// <summary>
    /// 部位 Key 参考值。ScriptableObject 中请自行填写 int，不必强制使用本枚举。
    /// display 编码：partKey * 10000000。
    /// </summary>
    public enum FrameRolePartType:byte
    {
        None = 0,
        Body = 1,
        Head = 2,
        Tail = 3,
        Shirt = 4,
        Pants = 5,
        Count=6
    }

    /// <summary>
    /// 种族 Key 参考值。ScriptableObject 中请自行填写 int；0 表示所有种族均可。
    /// display 编码：raceKey * 100000。
    /// </summary>
    public enum FrameRoleRaceType:byte
    {
        None = 0,
        Human = 1,
        Orc = 2,
        DarkElf = 3,
        Wolf = 4,
    }

    /// <summary>
    /// 性别 Key 参考值。ScriptableObject 中请自行填写 int；0 表示所有性别均可。
    /// display 编码：genderKey * 1000。
    /// </summary>
    public enum FrameRoleGenderType:byte
    {
        None = 0,
        Male = 1,
        Female = 2,
    }
}
