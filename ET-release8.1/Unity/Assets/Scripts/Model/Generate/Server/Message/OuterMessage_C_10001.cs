using MemoryPack;
using System.Collections.Generic;

namespace ET
{
    [MemoryPackable]
    [Message(OuterMessage.HttpGetRouterResponse)]
    public partial class HttpGetRouterResponse : MessageObject
    {
        public static HttpGetRouterResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(HttpGetRouterResponse), isFromPool) as HttpGetRouterResponse;
        }

        [MemoryPackOrder(0)]
        public List<string> Realms { get; set; } = new();

        [MemoryPackOrder(1)]
        public List<string> Routers { get; set; } = new();

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.Realms.Clear();
            this.Routers.Clear();

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.RouterSync)]
    public partial class RouterSync : MessageObject
    {
        public static RouterSync Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(RouterSync), isFromPool) as RouterSync;
        }

        [MemoryPackOrder(0)]
        public uint ConnectId { get; set; }

        [MemoryPackOrder(1)]
        public string Address { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.ConnectId = default;
            this.Address = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.C2M_TestRequest)]
    [ResponseType(nameof(M2C_TestResponse))]
    public partial class C2M_TestRequest : MessageObject, ILocationRequest
    {
        public static C2M_TestRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(C2M_TestRequest), isFromPool) as C2M_TestRequest;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public string request { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.request = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.M2C_TestResponse)]
    public partial class M2C_TestResponse : MessageObject, IResponse
    {
        public static M2C_TestResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(M2C_TestResponse), isFromPool) as M2C_TestResponse;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        [MemoryPackOrder(3)]
        public string response { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.response = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.C2G_EnterMap)]
    [ResponseType(nameof(G2C_EnterMap))]
    public partial class C2G_EnterMap : MessageObject, ISessionRequest
    {
        public static C2G_EnterMap Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(C2G_EnterMap), isFromPool) as C2G_EnterMap;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.G2C_EnterMap)]
    public partial class G2C_EnterMap : MessageObject, ISessionResponse
    {
        public static G2C_EnterMap Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(G2C_EnterMap), isFromPool) as G2C_EnterMap;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        /// <summary>
        /// 自己的UnitId
        /// </summary>
        [MemoryPackOrder(3)]
        public long MyId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.MyId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.MoveInfo)]
    public partial class MoveInfo : MessageObject
    {
        public static MoveInfo Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(MoveInfo), isFromPool) as MoveInfo;
        }

        [MemoryPackOrder(0)]
        public List<Unity.Mathematics.float3> Points { get; set; } = new();

        [MemoryPackOrder(1)]
        public Unity.Mathematics.quaternion Rotation { get; set; }

        [MemoryPackOrder(2)]
        public int TurnSpeed { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.Points.Clear();
            this.Rotation = default;
            this.TurnSpeed = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.UnitInfo)]
    public partial class UnitInfo : MessageObject
    {
        public static UnitInfo Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(UnitInfo), isFromPool) as UnitInfo;
        }

        [MemoryPackOrder(0)]
        public long UnitId { get; set; }

        [MemoryPackOrder(1)]
        public string Name { get; set; }

        [MemoryPackOrder(2)]
        public int ConfigId { get; set; }

        [MemoryPackOrder(3)]
        public int Type { get; set; }

        [MemoryPackOrder(4)]
        public int CampType { get; set; }

        [MemoryPackOrder(5)]
        public long OwnerId { get; set; }

        [MemoryPackOrder(6)]
        public long TeamId { get; set; }

        [MemoryPackOrder(7)]
        public Unity.Mathematics.float3 Position { get; set; }

        [MemoryPackOrder(8)]
        public Unity.Mathematics.float3 Forward { get; set; }

        [MongoDB.Bson.Serialization.Attributes.BsonDictionaryOptions(MongoDB.Bson.Serialization.Options.DictionaryRepresentation.ArrayOfArrays)]
        [MemoryPackOrder(9)]
        public Dictionary<int, long> KV { get; set; } = new();
        [MemoryPackOrder(10)]
        public MoveInfo MoveInfo { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.UnitId = default;
            this.Name = default;
            this.ConfigId = default;
            this.Type = default;
            this.CampType = default;
            this.OwnerId = default;
            this.TeamId = default;
            this.Position = default;
            this.Forward = default;
            this.KV.Clear();
            this.MoveInfo = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.M2C_CreateUnits)]
    public partial class M2C_CreateUnits : MessageObject, IMessage
    {
        public static M2C_CreateUnits Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(M2C_CreateUnits), isFromPool) as M2C_CreateUnits;
        }

        [MemoryPackOrder(0)]
        public List<UnitInfo> Units { get; set; } = new();

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.Units.Clear();

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.M2C_CreateMyUnit)]
    public partial class M2C_CreateMyUnit : MessageObject, IMessage
    {
        public static M2C_CreateMyUnit Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(M2C_CreateMyUnit), isFromPool) as M2C_CreateMyUnit;
        }

        [MemoryPackOrder(0)]
        public UnitInfo Unit { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.Unit = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.M2C_StartSceneChange)]
    public partial class M2C_StartSceneChange : MessageObject, IMessage
    {
        public static M2C_StartSceneChange Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(M2C_StartSceneChange), isFromPool) as M2C_StartSceneChange;
        }

        [MemoryPackOrder(0)]
        public long SceneInstanceId { get; set; }

        [MemoryPackOrder(1)]
        public string SceneName { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.SceneInstanceId = default;
            this.SceneName = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.M2C_RemoveUnits)]
    public partial class M2C_RemoveUnits : MessageObject, IMessage
    {
        public static M2C_RemoveUnits Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(M2C_RemoveUnits), isFromPool) as M2C_RemoveUnits;
        }

        [MemoryPackOrder(0)]
        public List<long> Units { get; set; } = new();

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.Units.Clear();

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.C2M_PathfindingResult)]
    public partial class C2M_PathfindingResult : MessageObject, ILocationMessage
    {
        public static C2M_PathfindingResult Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(C2M_PathfindingResult), isFromPool) as C2M_PathfindingResult;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public Unity.Mathematics.float3 Position { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Position = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.C2M_Stop)]
    public partial class C2M_Stop : MessageObject, ILocationMessage
    {
        public static C2M_Stop Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(C2M_Stop), isFromPool) as C2M_Stop;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.M2C_PathfindingResult)]
    public partial class M2C_PathfindingResult : MessageObject, IMessage
    {
        public static M2C_PathfindingResult Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(M2C_PathfindingResult), isFromPool) as M2C_PathfindingResult;
        }

        [MemoryPackOrder(0)]
        public long Id { get; set; }

        [MemoryPackOrder(1)]
        public Unity.Mathematics.float3 Position { get; set; }

        [MemoryPackOrder(2)]
        public List<Unity.Mathematics.float3> Points { get; set; } = new();

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.Id = default;
            this.Position = default;
            this.Points.Clear();

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.M2C_Stop)]
    public partial class M2C_Stop : MessageObject, IMessage
    {
        public static M2C_Stop Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(M2C_Stop), isFromPool) as M2C_Stop;
        }

        [MemoryPackOrder(0)]
        public int Error { get; set; }

        [MemoryPackOrder(1)]
        public long Id { get; set; }

        [MemoryPackOrder(2)]
        public Unity.Mathematics.float3 Position { get; set; }

        [MemoryPackOrder(3)]
        public Unity.Mathematics.quaternion Rotation { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.Error = default;
            this.Id = default;
            this.Position = default;
            this.Rotation = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.C2G_Ping)]
    [ResponseType(nameof(G2C_Ping))]
    public partial class C2G_Ping : MessageObject, ISessionRequest
    {
        public static C2G_Ping Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(C2G_Ping), isFromPool) as C2G_Ping;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.G2C_Ping)]
    public partial class G2C_Ping : MessageObject, ISessionResponse
    {
        public static G2C_Ping Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(G2C_Ping), isFromPool) as G2C_Ping;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        [MemoryPackOrder(3)]
        public long Time { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Time = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.G2C_Test)]
    public partial class G2C_Test : MessageObject, ISessionMessage
    {
        public static G2C_Test Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(G2C_Test), isFromPool) as G2C_Test;
        }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            
            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.C2M_Reload)]
    [ResponseType(nameof(M2C_Reload))]
    public partial class C2M_Reload : MessageObject, ISessionRequest
    {
        public static C2M_Reload Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(C2M_Reload), isFromPool) as C2M_Reload;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public string Account { get; set; }

        [MemoryPackOrder(2)]
        public string Password { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Account = default;
            this.Password = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.M2C_Reload)]
    public partial class M2C_Reload : MessageObject, ISessionResponse
    {
        public static M2C_Reload Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(M2C_Reload), isFromPool) as M2C_Reload;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.C2R_Login)]
    [ResponseType(nameof(R2C_Login))]
    public partial class C2R_Login : MessageObject, ISessionRequest
    {
        public static C2R_Login Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(C2R_Login), isFromPool) as C2R_Login;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        /// <summary>
        /// 帐号
        /// </summary>
        [MemoryPackOrder(1)]
        public string Account { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [MemoryPackOrder(2)]
        public string Password { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Account = default;
            this.Password = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.R2C_Login)]
    public partial class R2C_Login : MessageObject, ISessionResponse
    {
        public static R2C_Login Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(R2C_Login), isFromPool) as R2C_Login;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        [MemoryPackOrder(3)]
        public string Address { get; set; }

        [MemoryPackOrder(4)]
        public long Key { get; set; }

        [MemoryPackOrder(5)]
        public long GateId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Address = default;
            this.Key = default;
            this.GateId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.C2G_LoginGate)]
    [ResponseType(nameof(G2C_LoginGate))]
    public partial class C2G_LoginGate : MessageObject, ISessionRequest
    {
        public static C2G_LoginGate Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(C2G_LoginGate), isFromPool) as C2G_LoginGate;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        /// <summary>
        /// 帐号
        /// </summary>
        [MemoryPackOrder(1)]
        public long Key { get; set; }

        [MemoryPackOrder(2)]
        public long GateId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Key = default;
            this.GateId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.G2C_LoginGate)]
    public partial class G2C_LoginGate : MessageObject, ISessionResponse
    {
        public static G2C_LoginGate Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(G2C_LoginGate), isFromPool) as G2C_LoginGate;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        [MemoryPackOrder(3)]
        public long PlayerId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.PlayerId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.G2C_TestHotfixMessage)]
    public partial class G2C_TestHotfixMessage : MessageObject, ISessionMessage
    {
        public static G2C_TestHotfixMessage Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(G2C_TestHotfixMessage), isFromPool) as G2C_TestHotfixMessage;
        }

        [MemoryPackOrder(0)]
        public string Info { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.Info = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.C2M_TestRobotCase)]
    [ResponseType(nameof(M2C_TestRobotCase))]
    public partial class C2M_TestRobotCase : MessageObject, ILocationRequest
    {
        public static C2M_TestRobotCase Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(C2M_TestRobotCase), isFromPool) as C2M_TestRobotCase;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int N { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.N = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.M2C_TestRobotCase)]
    public partial class M2C_TestRobotCase : MessageObject, ILocationResponse
    {
        public static M2C_TestRobotCase Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(M2C_TestRobotCase), isFromPool) as M2C_TestRobotCase;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        [MemoryPackOrder(3)]
        public int N { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.N = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.C2M_TestRobotCase2)]
    public partial class C2M_TestRobotCase2 : MessageObject, ILocationMessage
    {
        public static C2M_TestRobotCase2 Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(C2M_TestRobotCase2), isFromPool) as C2M_TestRobotCase2;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int N { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.N = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.M2C_TestRobotCase2)]
    public partial class M2C_TestRobotCase2 : MessageObject, ILocationMessage
    {
        public static M2C_TestRobotCase2 Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(M2C_TestRobotCase2), isFromPool) as M2C_TestRobotCase2;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int N { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.N = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.C2M_TransferMap)]
    [ResponseType(nameof(M2C_TransferMap))]
    public partial class C2M_TransferMap : MessageObject, ILocationRequest
    {
        public static C2M_TransferMap Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(C2M_TransferMap), isFromPool) as C2M_TransferMap;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(0)]
        public int MapConfigId { get; set; }

        /// <summary>
        /// 地图唯一ID(可选)
        /// </summary>
        [MemoryPackOrder(1)]
        public long MapFiberId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.MapConfigId = default;
            this.MapFiberId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.M2C_TransferMap)]
    public partial class M2C_TransferMap : MessageObject, ILocationResponse
    {
        public static M2C_TransferMap Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(M2C_TransferMap), isFromPool) as M2C_TransferMap;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.C2G_Benchmark)]
    [ResponseType(nameof(G2C_Benchmark))]
    public partial class C2G_Benchmark : MessageObject, ISessionRequest
    {
        public static C2G_Benchmark Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(C2G_Benchmark), isFromPool) as C2G_Benchmark;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.G2C_Benchmark)]
    public partial class G2C_Benchmark : MessageObject, ISessionResponse
    {
        public static G2C_Benchmark Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(G2C_Benchmark), isFromPool) as G2C_Benchmark;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    /// <summary>
    /// 角色相关////////////////////////////////////////
    /// </summary>
    [MemoryPackable]
    [Message(OuterMessage.RoleInfoProto)]
    public partial class RoleInfoProto : MessageObject
    {
        public static RoleInfoProto Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(RoleInfoProto), isFromPool) as RoleInfoProto;
        }

        [MemoryPackOrder(0)]
        public long Id { get; set; }

        [MemoryPackOrder(1)]
        public string Name { get; set; }

        [MemoryPackOrder(2)]
        public int State { get; set; }

        [MemoryPackOrder(3)]
        public string AccountName { get; set; }

        [MemoryPackOrder(4)]
        public long LastLoginTime { get; set; }

        [MemoryPackOrder(5)]
        public long CreateTime { get; set; }

        [MemoryPackOrder(6)]
        public int ServerId { get; set; }

        [MemoryPackOrder(7)]
        public int ConfigId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.Id = default;
            this.Name = default;
            this.State = default;
            this.AccountName = default;
            this.LastLoginTime = default;
            this.CreateTime = default;
            this.ServerId = default;
            this.ConfigId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.C2R_GetRoles)]
    [ResponseType(nameof(R2C_GetRoles))]
    public partial class C2R_GetRoles : MessageObject, ISessionRequest
    {
        public static C2R_GetRoles Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(C2R_GetRoles), isFromPool) as C2R_GetRoles;
        }

        /// <summary>
        /// 获得某个服务器的角色列表
        /// </summary>
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public string Token { get; set; }

        [MemoryPackOrder(2)]
        public string AccountName { get; set; }

        [MemoryPackOrder(3)]
        public int ServerId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Token = default;
            this.AccountName = default;
            this.ServerId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.R2C_GetRoles)]
    public partial class R2C_GetRoles : MessageObject, ISessionResponse
    {
        public static R2C_GetRoles Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(R2C_GetRoles), isFromPool) as R2C_GetRoles;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        [MemoryPackOrder(3)]
        public List<RoleInfoProto> RoleInfoList { get; set; } = new();

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.RoleInfoList.Clear();

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.C2R_CreateRole)]
    [ResponseType(nameof(R2C_CreateRole))]
    public partial class C2R_CreateRole : MessageObject, ISessionRequest
    {
        public static C2R_CreateRole Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(C2R_CreateRole), isFromPool) as C2R_CreateRole;
        }

        /// <summary>
        /// 根据服务器id创建角色
        /// </summary>
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public string Token { get; set; }

        [MemoryPackOrder(2)]
        public string AccountName { get; set; }

        [MemoryPackOrder(3)]
        public string Name { get; set; }

        [MemoryPackOrder(4)]
        public int ServerId { get; set; }

        [MemoryPackOrder(5)]
        public int ConfigId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Token = default;
            this.AccountName = default;
            this.Name = default;
            this.ServerId = default;
            this.ConfigId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.R2C_CreateRole)]
    public partial class R2C_CreateRole : MessageObject, ISessionResponse
    {
        public static R2C_CreateRole Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(R2C_CreateRole), isFromPool) as R2C_CreateRole;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        [MemoryPackOrder(3)]
        public RoleInfoProto RoleInfo { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.RoleInfo = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.C2R_DeleteRole)]
    [ResponseType(nameof(R2C_DeleteRole))]
    public partial class C2R_DeleteRole : MessageObject, ISessionRequest
    {
        public static C2R_DeleteRole Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(C2R_DeleteRole), isFromPool) as C2R_DeleteRole;
        }

        /// <summary>
        /// 根据服务器id删除角色
        /// </summary>
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public string Token { get; set; }

        [MemoryPackOrder(2)]
        public string AccountName { get; set; }

        [MemoryPackOrder(3)]
        public long RoleInfoId { get; set; }

        [MemoryPackOrder(4)]
        public int ServerId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Token = default;
            this.AccountName = default;
            this.RoleInfoId = default;
            this.ServerId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.R2C_DeleteRole)]
    public partial class R2C_DeleteRole : MessageObject, ISessionResponse
    {
        public static R2C_DeleteRole Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(R2C_DeleteRole), isFromPool) as R2C_DeleteRole;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        [MemoryPackOrder(3)]
        public long DeleteRoleInfoId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.DeleteRoleInfoId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    /// <summary>
    /// 角色相关////////////////////////////////////////
    /// </summary>
    /// <summary>
    /// 登陆相关////////////////////////////////////////
    /// </summary>
    [MemoryPackable]
    [Message(OuterMessage.C2R_LoginAccount)]
    [ResponseType(nameof(R2C_LoginAccount))]
    public partial class C2R_LoginAccount : MessageObject, ISessionRequest
    {
        public static C2R_LoginAccount Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(C2R_LoginAccount), isFromPool) as C2R_LoginAccount;
        }

        /// <summary>
        /// 登录并且获取token
        /// </summary>
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public string AccountName { get; set; }

        [MemoryPackOrder(2)]
        public string Password { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.AccountName = default;
            this.Password = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.R2C_LoginAccount)]
    public partial class R2C_LoginAccount : MessageObject, ISessionResponse
    {
        public static R2C_LoginAccount Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(R2C_LoginAccount), isFromPool) as R2C_LoginAccount;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        [MemoryPackOrder(3)]
        public string Token { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Token = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.A2C_Disconnet)]
    public partial class A2C_Disconnet : MessageObject, ISessionMessage
    {
        public static A2C_Disconnet Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(A2C_Disconnet), isFromPool) as A2C_Disconnet;
        }

        /// <summary>
        /// 强制客户端session离线 0踢人 1超时
        /// </summary>
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.ServerInfoProto)]
    public partial class ServerInfoProto : MessageObject
    {
        public static ServerInfoProto Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(ServerInfoProto), isFromPool) as ServerInfoProto;
        }

        /// <summary>
        /// 服务器列表数据
        /// </summary>
        [MemoryPackOrder(0)]
        public int Id { get; set; }

        [MemoryPackOrder(1)]
        public int Status { get; set; }

        [MemoryPackOrder(2)]
        public string ServerName { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.Id = default;
            this.Status = default;
            this.ServerName = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.C2R_GetServerInfos)]
    [ResponseType(nameof(R2C_GetServerInfos))]
    public partial class C2R_GetServerInfos : MessageObject, ISessionRequest
    {
        public static C2R_GetServerInfos Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(C2R_GetServerInfos), isFromPool) as C2R_GetServerInfos;
        }

        /// <summary>
        /// 获得服务器列表
        /// </summary>
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public string Token { get; set; }

        [MemoryPackOrder(2)]
        public string AccountName { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Token = default;
            this.AccountName = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.R2C_GetServerInfos)]
    public partial class R2C_GetServerInfos : MessageObject, ISessionResponse
    {
        public static R2C_GetServerInfos Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(R2C_GetServerInfos), isFromPool) as R2C_GetServerInfos;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        [MemoryPackOrder(3)]
        public List<ServerInfoProto> ServerInfoList { get; set; } = new();

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.ServerInfoList.Clear();

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.C2R_GetRealmKey)]
    [ResponseType(nameof(R2C_GetRealmKey))]
    public partial class C2R_GetRealmKey : MessageObject, ISessionRequest
    {
        public static C2R_GetRealmKey Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(C2R_GetRealmKey), isFromPool) as C2R_GetRealmKey;
        }

        /// <summary>
        /// 获得realm服务器的key
        /// </summary>
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public string Token { get; set; }

        [MemoryPackOrder(2)]
        public string AccountName { get; set; }

        [MemoryPackOrder(3)]
        public int ServerId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Token = default;
            this.AccountName = default;
            this.ServerId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.R2C_GetRealmKey)]
    public partial class R2C_GetRealmKey : MessageObject, ISessionResponse
    {
        public static R2C_GetRealmKey Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(R2C_GetRealmKey), isFromPool) as R2C_GetRealmKey;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        [MemoryPackOrder(3)]
        public string Address { get; set; }

        [MemoryPackOrder(4)]
        public long Key { get; set; }

        [MemoryPackOrder(5)]
        public long GateId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Address = default;
            this.Key = default;
            this.GateId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.C2G_LoginGameGate)]
    [ResponseType(nameof(G2C_LoginGameGate))]
    public partial class C2G_LoginGameGate : MessageObject, ISessionRequest
    {
        public static C2G_LoginGameGate Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(C2G_LoginGameGate), isFromPool) as C2G_LoginGameGate;
        }

        /// <summary>
        /// 账号登录GameGate
        /// </summary>
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        /// <summary>
        /// realmkey
        /// </summary>
        [MemoryPackOrder(1)]
        public long Key { get; set; }

        /// <summary>
        /// 帐号
        /// </summary>
        [MemoryPackOrder(2)]
        public string AccountName { get; set; }

        /// <summary>
        /// 角色id
        /// </summary>
        [MemoryPackOrder(3)]
        public long RoleId { get; set; }

        /// <summary>
        /// 初始角色配置id
        /// </summary>
        [MemoryPackOrder(4)]
        public int ConfigId { get; set; }

        /// <summary>
        /// 角色名字
        /// </summary>
        [MemoryPackOrder(5)]
        public string Name { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Key = default;
            this.AccountName = default;
            this.RoleId = default;
            this.ConfigId = default;
            this.Name = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.G2C_LoginGameGate)]
    public partial class G2C_LoginGameGate : MessageObject, ISessionResponse
    {
        public static G2C_LoginGameGate Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(G2C_LoginGameGate), isFromPool) as G2C_LoginGameGate;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        [MemoryPackOrder(3)]
        public long PlayerId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.PlayerId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.C2G_EnterGame)]
    [ResponseType(nameof(G2C_EnterGame))]
    public partial class C2G_EnterGame : MessageObject, ISessionRequest
    {
        public static C2G_EnterGame Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(C2G_EnterGame), isFromPool) as C2G_EnterGame;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.G2C_EnterGame)]
    public partial class G2C_EnterGame : MessageObject, ISessionResponse
    {
        public static G2C_EnterGame Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(G2C_EnterGame), isFromPool) as G2C_EnterGame;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        /// <summary>
        /// 自己的UnitId
        /// </summary>
        [MemoryPackOrder(3)]
        public long MyUnitId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.MyUnitId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    /// <summary>
    /// 登陆相关////////////////////////////////////////
    /// </summary>
    /// <summary>
    /// 数值///////////////////////////////////////////
    /// </summary>
    [MemoryPackable]
    [Message(OuterMessage.C2M_TestNumericValue)]
    [ResponseType(nameof(M2C_TestNumericValue))]
    public partial class C2M_TestNumericValue : MessageObject, ILocationRequest
    {
        public static C2M_TestNumericValue Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(C2M_TestNumericValue), isFromPool) as C2M_TestNumericValue;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.M2C_TestNumericValue)]
    public partial class M2C_TestNumericValue : MessageObject, ILocationResponse
    {
        public static M2C_TestNumericValue Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(M2C_TestNumericValue), isFromPool) as M2C_TestNumericValue;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        [MemoryPackOrder(3)]
        public string response { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.response = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.M2C_NoticeUnitNumeric)]
    public partial class M2C_NoticeUnitNumeric : MessageObject, IMessage
    {
        public static M2C_NoticeUnitNumeric Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(M2C_NoticeUnitNumeric), isFromPool) as M2C_NoticeUnitNumeric;
        }

        [MemoryPackOrder(0)]
        public long UnitId { get; set; }

        [MemoryPackOrder(1)]
        public int NumericType { get; set; }

        [MemoryPackOrder(2)]
        public long NewValue { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.UnitId = default;
            this.NumericType = default;
            this.NewValue = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.M2C_NoticeNumericMsg)]
    public partial class M2C_NoticeNumericMsg : MessageObject, IMessage
    {
        public static M2C_NoticeNumericMsg Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(M2C_NoticeNumericMsg), isFromPool) as M2C_NoticeNumericMsg;
        }

        [MemoryPackOrder(0)]
        public int NumericType { get; set; }

        [MemoryPackOrder(1)]
        public long NewValue { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.NumericType = default;
            this.NewValue = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.M2C_NoticeUnitNumericList)]
    public partial class M2C_NoticeUnitNumericList : MessageObject, IMessage
    {
        public static M2C_NoticeUnitNumericList Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(M2C_NoticeUnitNumericList), isFromPool) as M2C_NoticeUnitNumericList;
        }

        [MemoryPackOrder(0)]
        public long UnitId { get; set; }

        [MemoryPackOrder(1)]
        public List<int> NumericTypeList { get; set; } = new();

        [MemoryPackOrder(2)]
        public List<long> NewValueList { get; set; } = new();

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.UnitId = default;
            this.NumericTypeList.Clear();
            this.NewValueList.Clear();

            ObjectPool.Instance.Recycle(this);
        }
    }

    /// <summary>
    /// 数值///////////////////////////////////////////
    /// </summary>
    /// <summary>
    /// 背包///////////////////////////////////////////
    /// </summary>
    [MemoryPackable]
    [Message(OuterMessage.ItemProto)]
    public partial class ItemProto : MessageObject
    {
        public static ItemProto Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(ItemProto), isFromPool) as ItemProto;
        }

        [MemoryPackOrder(0)]
        public int ConfigId { get; set; }

        [MemoryPackOrder(1)]
        public int ContainerType { get; set; }

        [MemoryPackOrder(2)]
        public long Id { get; set; }

        [MemoryPackOrder(3)]
        public int Count { get; set; }

        [MemoryPackOrder(4)]
        public int ItemQuality { get; set; }

        [MemoryPackOrder(5)]
        public EquipInfoProto EquipInfo { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.ConfigId = default;
            this.ContainerType = default;
            this.Id = default;
            this.Count = default;
            this.ItemQuality = default;
            this.EquipInfo = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.M2C_UpdateItemInfo)]
    public partial class M2C_UpdateItemInfo : MessageObject, IMessage
    {
        public static M2C_UpdateItemInfo Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(M2C_UpdateItemInfo), isFromPool) as M2C_UpdateItemInfo;
        }

        [MemoryPackOrder(0)]
        public int Op { get; set; }

        [MemoryPackOrder(1)]
        public ItemProto ItemInfo { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.Op = default;
            this.ItemInfo = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.C2M_GetAllKnapsack)]
    [ResponseType(nameof(M2C_GetAllKnapsack))]
    public partial class C2M_GetAllKnapsack : MessageObject, ILocationRequest
    {
        public static C2M_GetAllKnapsack Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(C2M_GetAllKnapsack), isFromPool) as C2M_GetAllKnapsack;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.M2C_GetAllKnapsack)]
    public partial class M2C_GetAllKnapsack : MessageObject, ILocationResponse
    {
        public static M2C_GetAllKnapsack Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(M2C_GetAllKnapsack), isFromPool) as M2C_GetAllKnapsack;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        [MemoryPackOrder(3)]
        public List<ItemProto> ItemList { get; set; } = new();

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.ItemList.Clear();

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.C2M_AddKnapsackItem)]
    [ResponseType(nameof(M2C_AddKnapsackItem))]
    public partial class C2M_AddKnapsackItem : MessageObject, ILocationRequest
    {
        public static C2M_AddKnapsackItem Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(C2M_AddKnapsackItem), isFromPool) as C2M_AddKnapsackItem;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int ContainerType { get; set; }

        [MemoryPackOrder(2)]
        public int ConfigId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.ContainerType = default;
            this.ConfigId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.M2C_AddKnapsackItem)]
    public partial class M2C_AddKnapsackItem : MessageObject, ILocationResponse
    {
        public static M2C_AddKnapsackItem Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(M2C_AddKnapsackItem), isFromPool) as M2C_AddKnapsackItem;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.C2M_RemoveKnapsackItem)]
    [ResponseType(nameof(M2C_RemoveKnapsackItem))]
    public partial class C2M_RemoveKnapsackItem : MessageObject, ILocationRequest
    {
        public static C2M_RemoveKnapsackItem Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(C2M_RemoveKnapsackItem), isFromPool) as C2M_RemoveKnapsackItem;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int ContainerType { get; set; }

        [MemoryPackOrder(2)]
        public long ItemId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.ContainerType = default;
            this.ItemId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.M2C_RemoveKnapsackItem)]
    public partial class M2C_RemoveKnapsackItem : MessageObject, ILocationResponse
    {
        public static M2C_RemoveKnapsackItem Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(M2C_RemoveKnapsackItem), isFromPool) as M2C_RemoveKnapsackItem;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.M2C_SyncAllKnapsackItems)]
    public partial class M2C_SyncAllKnapsackItems : MessageObject, IMessage
    {
        public static M2C_SyncAllKnapsackItems Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(M2C_SyncAllKnapsackItems), isFromPool) as M2C_SyncAllKnapsackItems;
        }

        [MemoryPackOrder(0)]
        public List<ItemProto> ItemList { get; set; } = new();

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.ItemList.Clear();

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.AttributeEntryProto)]
    public partial class AttributeEntryProto : MessageObject
    {
        public static AttributeEntryProto Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(AttributeEntryProto), isFromPool) as AttributeEntryProto;
        }

        [MemoryPackOrder(0)]
        public long Id { get; set; }

        [MemoryPackOrder(1)]
        public int Key { get; set; }

        [MemoryPackOrder(2)]
        public long Value { get; set; }

        [MemoryPackOrder(3)]
        public int EntryType { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.Id = default;
            this.Key = default;
            this.Value = default;
            this.EntryType = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.EquipInfoProto)]
    public partial class EquipInfoProto : MessageObject
    {
        public static EquipInfoProto Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(EquipInfoProto), isFromPool) as EquipInfoProto;
        }

        [MemoryPackOrder(0)]
        public long Id { get; set; }

        [MemoryPackOrder(1)]
        public int Score { get; set; }

        [MemoryPackOrder(2)]
        public List<AttributeEntryProto> AttributeEntryProtoList { get; set; } = new();

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.Id = default;
            this.Score = default;
            this.AttributeEntryProtoList.Clear();

            ObjectPool.Instance.Recycle(this);
        }
    }

    /// <summary>
    /// 背包///////////////////////////////////////////
    /// </summary>
    /// <summary>
    /// 排行榜///////////////////////////////////////////
    /// </summary>
    [MemoryPackable]
    [Message(OuterMessage.RankInfoProto)]
    public partial class RankInfoProto : MessageObject
    {
        public static RankInfoProto Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(RankInfoProto), isFromPool) as RankInfoProto;
        }

        [MemoryPackOrder(0)]
        public long Id { get; set; }

        [MemoryPackOrder(1)]
        public long UnitId { get; set; }

        [MemoryPackOrder(3)]
        public string Name { get; set; }

        [MemoryPackOrder(4)]
        public long RankValue { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.Id = default;
            this.UnitId = default;
            this.Name = default;
            this.RankValue = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.C2Rank_GetRanksInfo)]
    [ResponseType(nameof(Rank2C_GetRanksInfo))]
    public partial class C2Rank_GetRanksInfo : MessageObject, IRankInfoRequest
    {
        public static C2Rank_GetRanksInfo Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(C2Rank_GetRanksInfo), isFromPool) as C2Rank_GetRanksInfo;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.Rank2C_GetRanksInfo)]
    public partial class Rank2C_GetRanksInfo : MessageObject, IRankInfoResponse
    {
        public static Rank2C_GetRanksInfo Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(Rank2C_GetRanksInfo), isFromPool) as Rank2C_GetRanksInfo;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        [MemoryPackOrder(3)]
        public List<RankInfoProto> RankInfoProtoList { get; set; } = new();

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.RankInfoProtoList.Clear();

            ObjectPool.Instance.Recycle(this);
        }
    }

    /// <summary>
    /// 排行榜///////////////////////////////////////////
    /// </summary>
    /// <summary>
    /// 邮件///////////////////////////////////////////
    /// </summary>
    [MemoryPackable]
    [Message(OuterMessage.MailInfoProto)]
    public partial class MailInfoProto : MessageObject
    {
        public static MailInfoProto Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(MailInfoProto), isFromPool) as MailInfoProto;
        }

        [MemoryPackOrder(0)]
        public long MailId { get; set; }

        [MemoryPackOrder(1)]
        public int ConfigId { get; set; }

        [MemoryPackOrder(2)]
        public string Title { get; set; }

        [MemoryPackOrder(3)]
        public string Message { get; set; }

        [MemoryPackOrder(4)]
        public List<ItemProto> RewardList { get; set; } = new();

        [MemoryPackOrder(5)]
        public bool IsRead { get; set; }

        [MemoryPackOrder(6)]
        public bool IsCollected { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.MailId = default;
            this.ConfigId = default;
            this.Title = default;
            this.Message = default;
            this.RewardList.Clear();
            this.IsRead = default;
            this.IsCollected = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.C2Mail_GetAllMailList)]
    [ResponseType(nameof(Mail2C_GetAllMailList))]
    public partial class C2Mail_GetAllMailList : MessageObject, IMailInfoRequest
    {
        public static C2Mail_GetAllMailList Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(C2Mail_GetAllMailList), isFromPool) as C2Mail_GetAllMailList;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.Mail2C_GetAllMailList)]
    public partial class Mail2C_GetAllMailList : MessageObject, IMailInfoResponse
    {
        public static Mail2C_GetAllMailList Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(Mail2C_GetAllMailList), isFromPool) as Mail2C_GetAllMailList;
        }

        [MemoryPackOrder(89)]
        public int RpcId { get; set; }

        [MemoryPackOrder(90)]
        public int Error { get; set; }

        [MemoryPackOrder(91)]
        public string Message { get; set; }

        [MemoryPackOrder(0)]
        public List<MailInfoProto> MailInfoList { get; set; } = new();

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.MailInfoList.Clear();

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.Mail2C_NewMail)]
    public partial class Mail2C_NewMail : MessageObject, IMessage
    {
        public static Mail2C_NewMail Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(Mail2C_NewMail), isFromPool) as Mail2C_NewMail;
        }

        [MemoryPackOrder(0)]
        public MailInfoProto MailInfo { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.MailInfo = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.C2Mail_CollectAttachment)]
    [ResponseType(nameof(Mail2C_CollectAttachment))]
    public partial class C2Mail_CollectAttachment : MessageObject, IMailInfoRequest
    {
        public static C2Mail_CollectAttachment Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(C2Mail_CollectAttachment), isFromPool) as C2Mail_CollectAttachment;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public long MailId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.MailId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.Mail2C_CollectAttachment)]
    public partial class Mail2C_CollectAttachment : MessageObject, IMailInfoResponse
    {
        public static Mail2C_CollectAttachment Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(Mail2C_CollectAttachment), isFromPool) as Mail2C_CollectAttachment;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.Mail2C_UpdateMailInfo)]
    public partial class Mail2C_UpdateMailInfo : MessageObject, IMessage
    {
        public static Mail2C_UpdateMailInfo Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(Mail2C_UpdateMailInfo), isFromPool) as Mail2C_UpdateMailInfo;
        }

        [MemoryPackOrder(0)]
        public MailInfoProto MailInfo { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.MailInfo = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.C2Mail_ReadMail)]
    public partial class C2Mail_ReadMail : MessageObject, IMailInfoMessage
    {
        public static C2Mail_ReadMail Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(C2Mail_ReadMail), isFromPool) as C2Mail_ReadMail;
        }

        [MemoryPackOrder(0)]
        public long MailId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.MailId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    /// <summary>
    /// 邮件///////////////////////////////////////////
    /// </summary>
    /// <summary>
    /// 组队///////////////////////////////////////////
    /// </summary>
    [MemoryPackable]
    [Message(OuterMessage.C2G_CreateTeam)]
    [ResponseType(nameof(G2C_CreateTeam))]
    public partial class C2G_CreateTeam : MessageObject, ISessionRequest
    {
        public static C2G_CreateTeam Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(C2G_CreateTeam), isFromPool) as C2G_CreateTeam;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        /// <summary>
        /// 队伍名
        /// </summary>
        [MemoryPackOrder(1)]
        public string TeamName { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.TeamName = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.G2C_CreateTeam)]
    public partial class G2C_CreateTeam : MessageObject, ISessionResponse
    {
        public static G2C_CreateTeam Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(G2C_CreateTeam), isFromPool) as G2C_CreateTeam;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.C2G_LeaveTeam)]
    [ResponseType(nameof(G2C_LeaveTeam))]
    public partial class C2G_LeaveTeam : MessageObject, ISessionRequest
    {
        public static C2G_LeaveTeam Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(C2G_LeaveTeam), isFromPool) as C2G_LeaveTeam;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public long Dissolve { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Dissolve = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.G2C_LeaveTeam)]
    public partial class G2C_LeaveTeam : MessageObject, ISessionResponse
    {
        public static G2C_LeaveTeam Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(G2C_LeaveTeam), isFromPool) as G2C_LeaveTeam;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    /// <summary>
    /// 组队///////////////////////////////////////////
    /// </summary>
    /// <summary>
    /// 技能///////////////////////////////////////////
    /// </summary>
    // cast开头
    [MemoryPackable]
    [Message(OuterMessage.C2M_CastStart)]
    [ResponseType(nameof(M2C_CastStart))]
    public partial class C2M_CastStart : MessageObject, ILocationRequest
    {
        public static C2M_CastStart Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(C2M_CastStart), isFromPool) as C2M_CastStart;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        /// <summary>
        /// 技能配表id
        /// </summary>
        [MemoryPackOrder(1)]
        public int CastConfigId { get; set; }

        /// <summary>
        /// 输入坐标
        /// </summary>
        [MemoryPackOrder(2)]
        public Unity.Mathematics.float3 InputPos { get; set; }

        /// <summary>
        /// 目标对象id
        /// </summary>
        [MemoryPackOrder(3)]
        public long InputUnitId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.CastConfigId = default;
            this.InputPos = default;
            this.InputUnitId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    // cast回调
    [MemoryPackable]
    [Message(OuterMessage.M2C_CastStart)]
    public partial class M2C_CastStart : MessageObject, ILocationResponse
    {
        public static M2C_CastStart Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(M2C_CastStart), isFromPool) as M2C_CastStart;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        /// <summary>
        /// 技能uuid
        /// </summary>
        [MemoryPackOrder(3)]
        public long CastId { get; set; }

        /// <summary>
        /// 释放者id
        /// </summary>
        [MemoryPackOrder(4)]
        public long CasterId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.CastId = default;
            this.CasterId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    // cast结束
    [MemoryPackable]
    [Message(OuterMessage.M2C_CastFinish)]
    public partial class M2C_CastFinish : MessageObject, IMessage
    {
        public static M2C_CastFinish Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(M2C_CastFinish), isFromPool) as M2C_CastFinish;
        }

        /// <summary>
        /// 技能uuid
        /// </summary>
        [MemoryPackOrder(0)]
        public long CastId { get; set; }

        /// <summary>
        /// 释放者id
        /// </summary>
        [MemoryPackOrder(1)]
        public long CasterId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.CastId = default;
            this.CasterId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    // cast打断
    [MemoryPackable]
    [Message(OuterMessage.M2C_CastBreak)]
    public partial class M2C_CastBreak : MessageObject, IMessage
    {
        public static M2C_CastBreak Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(M2C_CastBreak), isFromPool) as M2C_CastBreak;
        }

        /// <summary>
        /// 技能uuid
        /// </summary>
        [MemoryPackOrder(0)]
        public long CastId { get; set; }

        /// <summary>
        /// 释放者id
        /// </summary>
        [MemoryPackOrder(1)]
        public long CasterId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.CastId = default;
            this.CasterId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    /// <summary>
    /// 技能///////////////////////////////////////////
    /// </summary>
    /// <summary>
    /// 相关GM///////////////////////////////////////////
    /// </summary>
    [MemoryPackable]
    [Message(OuterMessage.C2Mail_GMAddMail)]
    [ResponseType(nameof(Mail2C_GMAddMail))]
    public partial class C2Mail_GMAddMail : MessageObject, IMailInfoRequest
    {
        public static C2Mail_GMAddMail Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(C2Mail_GMAddMail), isFromPool) as C2Mail_GMAddMail;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int ConfigId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.ConfigId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(OuterMessage.Mail2C_GMAddMail)]
    public partial class Mail2C_GMAddMail : MessageObject, IMailInfoResponse
    {
        public static Mail2C_GMAddMail Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(Mail2C_GMAddMail), isFromPool) as Mail2C_GMAddMail;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    /// <summary>
    /// 相关GM///////////////////////////////////////////
    /// </summary>
    public static class OuterMessage
    {
        public const ushort HttpGetRouterResponse = 10002;
        public const ushort RouterSync = 10003;
        public const ushort C2M_TestRequest = 10004;
        public const ushort M2C_TestResponse = 10005;
        public const ushort C2G_EnterMap = 10006;
        public const ushort G2C_EnterMap = 10007;
        public const ushort MoveInfo = 10008;
        public const ushort UnitInfo = 10009;
        public const ushort M2C_CreateUnits = 10010;
        public const ushort M2C_CreateMyUnit = 10011;
        public const ushort M2C_StartSceneChange = 10012;
        public const ushort M2C_RemoveUnits = 10013;
        public const ushort C2M_PathfindingResult = 10014;
        public const ushort C2M_Stop = 10015;
        public const ushort M2C_PathfindingResult = 10016;
        public const ushort M2C_Stop = 10017;
        public const ushort C2G_Ping = 10018;
        public const ushort G2C_Ping = 10019;
        public const ushort G2C_Test = 10020;
        public const ushort C2M_Reload = 10021;
        public const ushort M2C_Reload = 10022;
        public const ushort C2R_Login = 10023;
        public const ushort R2C_Login = 10024;
        public const ushort C2G_LoginGate = 10025;
        public const ushort G2C_LoginGate = 10026;
        public const ushort G2C_TestHotfixMessage = 10027;
        public const ushort C2M_TestRobotCase = 10028;
        public const ushort M2C_TestRobotCase = 10029;
        public const ushort C2M_TestRobotCase2 = 10030;
        public const ushort M2C_TestRobotCase2 = 10031;
        public const ushort C2M_TransferMap = 10032;
        public const ushort M2C_TransferMap = 10033;
        public const ushort C2G_Benchmark = 10034;
        public const ushort G2C_Benchmark = 10035;
        public const ushort RoleInfoProto = 10036;
        public const ushort C2R_GetRoles = 10037;
        public const ushort R2C_GetRoles = 10038;
        public const ushort C2R_CreateRole = 10039;
        public const ushort R2C_CreateRole = 10040;
        public const ushort C2R_DeleteRole = 10041;
        public const ushort R2C_DeleteRole = 10042;
        public const ushort C2R_LoginAccount = 10043;
        public const ushort R2C_LoginAccount = 10044;
        public const ushort A2C_Disconnet = 10045;
        public const ushort ServerInfoProto = 10046;
        public const ushort C2R_GetServerInfos = 10047;
        public const ushort R2C_GetServerInfos = 10048;
        public const ushort C2R_GetRealmKey = 10049;
        public const ushort R2C_GetRealmKey = 10050;
        public const ushort C2G_LoginGameGate = 10051;
        public const ushort G2C_LoginGameGate = 10052;
        public const ushort C2G_EnterGame = 10053;
        public const ushort G2C_EnterGame = 10054;
        public const ushort C2M_TestNumericValue = 10055;
        public const ushort M2C_TestNumericValue = 10056;
        public const ushort M2C_NoticeUnitNumeric = 10057;
        public const ushort M2C_NoticeNumericMsg = 10058;
        public const ushort M2C_NoticeUnitNumericList = 10059;
        public const ushort ItemProto = 10060;
        public const ushort M2C_UpdateItemInfo = 10061;
        public const ushort C2M_GetAllKnapsack = 10062;
        public const ushort M2C_GetAllKnapsack = 10063;
        public const ushort C2M_AddKnapsackItem = 10064;
        public const ushort M2C_AddKnapsackItem = 10065;
        public const ushort C2M_RemoveKnapsackItem = 10066;
        public const ushort M2C_RemoveKnapsackItem = 10067;
        public const ushort M2C_SyncAllKnapsackItems = 10068;
        public const ushort AttributeEntryProto = 10069;
        public const ushort EquipInfoProto = 10070;
        public const ushort RankInfoProto = 10071;
        public const ushort C2Rank_GetRanksInfo = 10072;
        public const ushort Rank2C_GetRanksInfo = 10073;
        public const ushort MailInfoProto = 10074;
        public const ushort C2Mail_GetAllMailList = 10075;
        public const ushort Mail2C_GetAllMailList = 10076;
        public const ushort Mail2C_NewMail = 10077;
        public const ushort C2Mail_CollectAttachment = 10078;
        public const ushort Mail2C_CollectAttachment = 10079;
        public const ushort Mail2C_UpdateMailInfo = 10080;
        public const ushort C2Mail_ReadMail = 10081;
        public const ushort C2G_CreateTeam = 10082;
        public const ushort G2C_CreateTeam = 10083;
        public const ushort C2G_LeaveTeam = 10084;
        public const ushort G2C_LeaveTeam = 10085;
        public const ushort C2M_CastStart = 10086;
        public const ushort M2C_CastStart = 10087;
        public const ushort M2C_CastFinish = 10088;
        public const ushort M2C_CastBreak = 10089;
        public const ushort C2Mail_GMAddMail = 10090;
        public const ushort Mail2C_GMAddMail = 10091;
    }
}