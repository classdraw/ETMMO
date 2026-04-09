namespace ET.Client
{
    /// <summary>
    /// 网络 Cache 数据缓存（账号、Token、区服列表等与网络会话相关的客户端缓存），挂在 Main Scene，访问方式与 <see cref="PlayerComponent"/> 相同。
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class NetworkCacheComponent : Entity, IAwake, IDestroy
    {
        #region 登录相关缓存数据
        public string Account { get; set; }

        public string Token { get; set; }

        /// <summary>最近一次拉取的区服列表响应，业务侧按需赋值。</summary>
        public R2C_GetServerInfos LastServerListResponse { get; set; }

        public int ServerId { get; set; }
        //最后一次获取的角色数据
        public R2C_GetRoles LastRoleListResponse { get; set; }

        public int RoleId { get; set; }

        /// <summary><see cref="NetClient2Main_LoginGame.PlayerId"/>（进图时的 MyUnitId），用于视图层识别本单位。</summary>
        public long LoginGamePlayerId { get; set; }

        #endregion

    }
}
