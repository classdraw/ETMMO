using System.Collections.Generic;

namespace ET.Client
{
    [EntitySystemOf(typeof(NetworkCacheComponent))]
    [FriendOf(typeof(NetworkCacheComponent))]
    public static partial class NetworkCacheComponentSystem
    {
        [EntitySystem]
        private static void Awake(this NetworkCacheComponent self)
        {
            self.ClearCache();
        }

        [EntitySystem]
        private static void Destroy(this NetworkCacheComponent self)
        {
            self.ClearCache();
        }

        /// <summary>清空网络相关缓存（登出、回登录可调用）。</summary>
        public static void ClearCache(this NetworkCacheComponent self)
        {
            self.Account = string.Empty;
            self.Token = string.Empty;
            self.LastServerListResponse = null;
            self.LoginGamePlayerId = 0;
            self.ServerId = -1;
            self.LastRoleListResponse = null;
        }

        /// <summary>一次写入登录拉服后的账号、Token 与区服列表响应。</summary>
        public static void SetLoginServerListData(this NetworkCacheComponent self, string account, string token,
            R2C_GetServerInfos serverListResponse)
        {
            self.Account = account ?? string.Empty;
            self.Token = token ?? string.Empty;
            self.LastServerListResponse = serverListResponse;
        }

        public static void SetLoginRoleListData(this NetworkCacheComponent self,R2C_GetRoles roles)
        {
            self.LastRoleListResponse = roles;
        }

        /// <summary>
        /// 将创建角色返回的 <see cref="RoleInfoProto"/> 合并进 <see cref="NetworkCacheComponent.LastRoleListResponse"/>，
        /// 按角色 Id 升序排序；若尚未拉取过角色列表则新建容器。
        /// （区服列表见 <see cref="NetworkCacheComponent.LastServerListResponse"/>，与角色列表分离。）
        /// </summary>
        public static void MergeCreatedRoleIntoRoleListCache(this NetworkCacheComponent self, RoleInfoProto roleInfo)
        {
            if (roleInfo == null)
            {
                return;
            }

            R2C_GetRoles roles = self.LastRoleListResponse;
            if (roles == null)
            {
                roles = R2C_GetRoles.Create();
                roles.Error = ErrorCode.ERR_Success;
                self.LastRoleListResponse = roles;
            }

            List<RoleInfoProto> list = roles.RoleInfoList;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i].Id == roleInfo.Id)
                {
                    list.RemoveAt(i);
                }
            }

            list.Add(roleInfo);
            list.Sort(static (a, b) => a.Id.CompareTo(b.Id));
        }

        /// <summary>从 <see cref="NetworkCacheComponent.LastRoleListResponse"/> 中移除指定角色 Id（若有）。</summary>
        public static void RemoveRoleFromRoleListCache(this NetworkCacheComponent self, long roleInfoId)
        {
            if (roleInfoId == 0)
            {
                return;
            }

            R2C_GetRoles roles = self.LastRoleListResponse;
            if (roles?.RoleInfoList == null)
            {
                return;
            }

            List<RoleInfoProto> list = roles.RoleInfoList;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i].Id == roleInfoId)
                {
                    list.RemoveAt(i);
                }
            }
        }
    }
}
