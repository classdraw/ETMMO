namespace ET.Client
{
    public static class RelationshipHelper
    {
        public static async ETTask<int> GMCreateTeam(Scene root,string teamName)
        {
            C2G_CreateTeam c2GCreateTeam = C2G_CreateTeam.Create();
            c2GCreateTeam.TeamName = teamName;
            G2C_CreateTeam g2CCreateTeam=await root.GetComponent<ClientSenderComponent>().Call(c2GCreateTeam) as G2C_CreateTeam;
            if (g2CCreateTeam.Error==ErrorCode.ERR_Success)
            {
                Log.Info($"创建队伍成功!!!");
            }
            else
            {
                Log.Info($"创建队伍失败!!!"+g2CCreateTeam.Error);
            }

            return g2CCreateTeam.Error;
        }
        
        public static async ETTask<int> GMLeaveTeam(Scene root,bool dissolve)
        {
            C2G_LeaveTeam c2GLeaveTeam = C2G_LeaveTeam.Create();
            c2GLeaveTeam.Dissolve = dissolve?1:0;
            G2C_LeaveTeam g2CLeaveTeam=await root.GetComponent<ClientSenderComponent>().Call(c2GLeaveTeam) as G2C_LeaveTeam;
            if (g2CLeaveTeam.Error==ErrorCode.ERR_Success)
            {
                if (dissolve)
                {
                    Log.Info($"解散队伍成功!!!");
                }
                else
                {
                    Log.Info($"离开队伍成功!!!");
                }
                
            }
            else
            {
                if (dissolve)
                {
                    Log.Info($"解散队伍失败!!!"+g2CLeaveTeam.Error);
                }
                else
                {
                    Log.Info($"离开队伍失败!!!"+g2CLeaveTeam.Error);
                }
            }

            return g2CLeaveTeam.Error;
        }
    }
}

