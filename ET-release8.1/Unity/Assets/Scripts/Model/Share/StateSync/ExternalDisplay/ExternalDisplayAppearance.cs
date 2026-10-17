namespace ET
{
    /// <summary>
    /// 创角档案（Race/Gender/Job）与怪物部位拼装（Body/Head/…）共用结构。
    /// </summary>
    public struct ExternalDisplayAppearance
    {
        public int Race;
        public int Gender;
        public int Job;
        public int BodyDisplayId;
        public int HeadDisplayId;
        public int TailDisplayId;
        public int ShirtDisplayId;
        public int PantsDisplayId;
    }
}
