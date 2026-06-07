using MemoryPack;
using System.Collections.Generic;

namespace ET
{
    [MemoryPackable]
    [Message(InnerMessage.ObjectQueryRequest)]
    [ResponseType(nameof(ObjectQueryResponse))]
    public partial class ObjectQueryRequest : MessageObject, IRequest
    {
        public static ObjectQueryRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(ObjectQueryRequest), isFromPool) as ObjectQueryRequest;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public long Key { get; set; }

        [MemoryPackOrder(2)]
        public long InstanceId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Key = default;
            this.InstanceId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(InnerMessage.M2A_Reload)]
    [ResponseType(nameof(A2M_Reload))]
    public partial class M2A_Reload : MessageObject, IRequest
    {
        public static M2A_Reload Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(M2A_Reload), isFromPool) as M2A_Reload;
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
    [Message(InnerMessage.A2M_Reload)]
    public partial class A2M_Reload : MessageObject, IResponse
    {
        public static A2M_Reload Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(A2M_Reload), isFromPool) as A2M_Reload;
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
    [Message(InnerMessage.G2G_LockRequest)]
    [ResponseType(nameof(G2G_LockResponse))]
    public partial class G2G_LockRequest : MessageObject, IRequest
    {
        public static G2G_LockRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(G2G_LockRequest), isFromPool) as G2G_LockRequest;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public long Id { get; set; }

        [MemoryPackOrder(2)]
        public string Address { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Id = default;
            this.Address = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(InnerMessage.G2G_LockResponse)]
    public partial class G2G_LockResponse : MessageObject, IResponse
    {
        public static G2G_LockResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(G2G_LockResponse), isFromPool) as G2G_LockResponse;
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
    [Message(InnerMessage.G2G_LockReleaseRequest)]
    [ResponseType(nameof(G2G_LockReleaseResponse))]
    public partial class G2G_LockReleaseRequest : MessageObject, IRequest
    {
        public static G2G_LockReleaseRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(G2G_LockReleaseRequest), isFromPool) as G2G_LockReleaseRequest;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public long Id { get; set; }

        [MemoryPackOrder(2)]
        public string Address { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Id = default;
            this.Address = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(InnerMessage.G2G_LockReleaseResponse)]
    public partial class G2G_LockReleaseResponse : MessageObject, IResponse
    {
        public static G2G_LockReleaseResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(G2G_LockReleaseResponse), isFromPool) as G2G_LockReleaseResponse;
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
    [Message(InnerMessage.ObjectAddRequest)]
    [ResponseType(nameof(ObjectAddResponse))]
    public partial class ObjectAddRequest : MessageObject, IRequest
    {
        public static ObjectAddRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(ObjectAddRequest), isFromPool) as ObjectAddRequest;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Type { get; set; }

        [MemoryPackOrder(2)]
        public long Key { get; set; }

        [MemoryPackOrder(3)]
        public ActorId ActorId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Type = default;
            this.Key = default;
            this.ActorId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(InnerMessage.ObjectAddResponse)]
    public partial class ObjectAddResponse : MessageObject, IResponse
    {
        public static ObjectAddResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(ObjectAddResponse), isFromPool) as ObjectAddResponse;
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
    [Message(InnerMessage.ObjectLockRequest)]
    [ResponseType(nameof(ObjectLockResponse))]
    public partial class ObjectLockRequest : MessageObject, IRequest
    {
        public static ObjectLockRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(ObjectLockRequest), isFromPool) as ObjectLockRequest;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Type { get; set; }

        [MemoryPackOrder(2)]
        public long Key { get; set; }

        [MemoryPackOrder(3)]
        public ActorId ActorId { get; set; }

        [MemoryPackOrder(4)]
        public int Time { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Type = default;
            this.Key = default;
            this.ActorId = default;
            this.Time = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(InnerMessage.ObjectLockResponse)]
    public partial class ObjectLockResponse : MessageObject, IResponse
    {
        public static ObjectLockResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(ObjectLockResponse), isFromPool) as ObjectLockResponse;
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
    [Message(InnerMessage.ObjectUnLockRequest)]
    [ResponseType(nameof(ObjectUnLockResponse))]
    public partial class ObjectUnLockRequest : MessageObject, IRequest
    {
        public static ObjectUnLockRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(ObjectUnLockRequest), isFromPool) as ObjectUnLockRequest;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Type { get; set; }

        [MemoryPackOrder(2)]
        public long Key { get; set; }

        [MemoryPackOrder(3)]
        public ActorId OldActorId { get; set; }

        [MemoryPackOrder(4)]
        public ActorId NewActorId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Type = default;
            this.Key = default;
            this.OldActorId = default;
            this.NewActorId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(InnerMessage.ObjectUnLockResponse)]
    public partial class ObjectUnLockResponse : MessageObject, IResponse
    {
        public static ObjectUnLockResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(ObjectUnLockResponse), isFromPool) as ObjectUnLockResponse;
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
    [Message(InnerMessage.ObjectRemoveRequest)]
    [ResponseType(nameof(ObjectRemoveResponse))]
    public partial class ObjectRemoveRequest : MessageObject, IRequest
    {
        public static ObjectRemoveRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(ObjectRemoveRequest), isFromPool) as ObjectRemoveRequest;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Type { get; set; }

        [MemoryPackOrder(2)]
        public long Key { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Type = default;
            this.Key = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(InnerMessage.ObjectRemoveResponse)]
    public partial class ObjectRemoveResponse : MessageObject, IResponse
    {
        public static ObjectRemoveResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(ObjectRemoveResponse), isFromPool) as ObjectRemoveResponse;
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
    [Message(InnerMessage.ObjectGetRequest)]
    [ResponseType(nameof(ObjectGetResponse))]
    public partial class ObjectGetRequest : MessageObject, IRequest
    {
        public static ObjectGetRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(ObjectGetRequest), isFromPool) as ObjectGetRequest;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Type { get; set; }

        [MemoryPackOrder(2)]
        public long Key { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Type = default;
            this.Key = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(InnerMessage.ObjectGetResponse)]
    public partial class ObjectGetResponse : MessageObject, IResponse
    {
        public static ObjectGetResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(ObjectGetResponse), isFromPool) as ObjectGetResponse;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        [MemoryPackOrder(3)]
        public int Type { get; set; }

        [MemoryPackOrder(4)]
        public ActorId ActorId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Type = default;
            this.ActorId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(InnerMessage.R2G_GetLoginKey)]
    [ResponseType(nameof(G2R_GetLoginKey))]
    public partial class R2G_GetLoginKey : MessageObject, IRequest
    {
        public static R2G_GetLoginKey Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(R2G_GetLoginKey), isFromPool) as R2G_GetLoginKey;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public string Account { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Account = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(InnerMessage.G2R_GetLoginKey)]
    public partial class G2R_GetLoginKey : MessageObject, IResponse
    {
        public static G2R_GetLoginKey Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(G2R_GetLoginKey), isFromPool) as G2R_GetLoginKey;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        [MemoryPackOrder(3)]
        public long Key { get; set; }

        [MemoryPackOrder(4)]
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
            this.Key = default;
            this.GateId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(InnerMessage.G2M_SessionDisconnect)]
    public partial class G2M_SessionDisconnect : MessageObject, ILocationMessage
    {
        public static G2M_SessionDisconnect Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(G2M_SessionDisconnect), isFromPool) as G2M_SessionDisconnect;
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
    [Message(InnerMessage.ObjectQueryResponse)]
    public partial class ObjectQueryResponse : MessageObject, IResponse
    {
        public static ObjectQueryResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(ObjectQueryResponse), isFromPool) as ObjectQueryResponse;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        [MemoryPackOrder(3)]
        public byte[] Entity { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.Entity = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(InnerMessage.M2M_UnitTransferRequest)]
    [ResponseType(nameof(M2M_UnitTransferResponse))]
    public partial class M2M_UnitTransferRequest : MessageObject, IRequest
    {
        public static M2M_UnitTransferRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(M2M_UnitTransferRequest), isFromPool) as M2M_UnitTransferRequest;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public ActorId OldActorId { get; set; }

        [MemoryPackOrder(2)]
        public byte[] Unit { get; set; }

        [MemoryPackOrder(3)]
        public int MapId { get; set; }

        [MemoryPackOrder(4)]
        public bool IsEnterGame { get; set; }

        [MemoryPackOrder(5)]
        public List<byte[]> Entitys { get; set; } = new();

        [MemoryPackOrder(6)]
        public List<string> Types { get; set; } = new();

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.OldActorId = default;
            this.Unit = default;
            this.MapId = default;
            this.IsEnterGame = default;
            this.Entitys.Clear();
            this.Types.Clear();

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(InnerMessage.M2M_UnitTransferResponse)]
    public partial class M2M_UnitTransferResponse : MessageObject, IResponse
    {
        public static M2M_UnitTransferResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(M2M_UnitTransferResponse), isFromPool) as M2M_UnitTransferResponse;
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
    [Message(InnerMessage.M2M_InitMap)]
    public partial class M2M_InitMap : MessageObject, IMessage
    {
        public static M2M_InitMap Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(M2M_InitMap), isFromPool) as M2M_InitMap;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int MapConfigId { get; set; }

        [MemoryPackOrder(2)]
        public CreateMapCtx Ctx { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.MapConfigId = default;
            this.Ctx = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    // 地图关闭前强制传送玩家
    [MemoryPackable]
    [Message(InnerMessage.M2M_MapCloseTransfer)]
    public partial class M2M_MapCloseTransfer : MessageObject, IMessage
    {
        public static M2M_MapCloseTransfer Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(M2M_MapCloseTransfer), isFromPool) as M2M_MapCloseTransfer;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int MapConfigId { get; set; }

        [MemoryPackOrder(2)]
        public ActorId TargetActorId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.MapConfigId = default;
            this.TargetActorId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    // 获取地图地址
    [MemoryPackable]
    [Message(InnerMessage.O2M_GetMapActorIdRequest)]
    [ResponseType(nameof(M2O_GetMapActorIdResponse))]
    public partial class O2M_GetMapActorIdRequest : MessageObject, IRequest
    {
        public static O2M_GetMapActorIdRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(O2M_GetMapActorIdRequest), isFromPool) as O2M_GetMapActorIdRequest;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int MapConfigId { get; set; }

        [MemoryPackOrder(2)]
        public long Id { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.MapConfigId = default;
            this.Id = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(InnerMessage.M2O_GetMapActorIdResponse)]
    public partial class M2O_GetMapActorIdResponse : MessageObject, IResponse
    {
        public static M2O_GetMapActorIdResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(M2O_GetMapActorIdResponse), isFromPool) as M2O_GetMapActorIdResponse;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        [MemoryPackOrder(3)]
        public ActorId ActorId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.ActorId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    // 进入地图
    [MemoryPackable]
    [Message(InnerMessage.O2M_EnterMap)]
    public partial class O2M_EnterMap : MessageObject, IMessage
    {
        public static O2M_EnterMap Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(O2M_EnterMap), isFromPool) as O2M_EnterMap;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int MapConfigId { get; set; }

        [MemoryPackOrder(2)]
        public long Id { get; set; }

        [MemoryPackOrder(3)]
        public ActorId MapActorId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.MapConfigId = default;
            this.Id = default;
            this.MapActorId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    // 创建地图
    [MemoryPackable]
    [Message(InnerMessage.O2M_CreateMapRequest)]
    [ResponseType(nameof(M2O_CreateMapResponse))]
    public partial class O2M_CreateMapRequest : MessageObject, IRequest
    {
        public static O2M_CreateMapRequest Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(O2M_CreateMapRequest), isFromPool) as O2M_CreateMapRequest;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int MapConfigId { get; set; }

        [MemoryPackOrder(2)]
        public CreateMapCtx Ctx { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.MapConfigId = default;
            this.Ctx = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(InnerMessage.M2O_CreateMapResponse)]
    public partial class M2O_CreateMapResponse : MessageObject, IResponse
    {
        public static M2O_CreateMapResponse Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(M2O_CreateMapResponse), isFromPool) as M2O_CreateMapResponse;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        [MemoryPackOrder(3)]
        public ActorId ActorId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.ActorId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(InnerMessage.R2L_LoginAccount)]
    [ResponseType(nameof(L2R_LoginAccount))]
    public partial class R2L_LoginAccount : MessageObject, IRequest
    {
        public static R2L_LoginAccount Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(R2L_LoginAccount), isFromPool) as R2L_LoginAccount;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public string AccountName { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.AccountName = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(InnerMessage.L2R_LoginAccount)]
    public partial class L2R_LoginAccount : MessageObject, IResponse
    {
        public static L2R_LoginAccount Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(L2R_LoginAccount), isFromPool) as L2R_LoginAccount;
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
    [Message(InnerMessage.L2G_DisConnectGateUnit)]
    [ResponseType(nameof(G2L_DisConnectGateUnit))]
    public partial class L2G_DisConnectGateUnit : MessageObject, IRequest
    {
        public static L2G_DisConnectGateUnit Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(L2G_DisConnectGateUnit), isFromPool) as L2G_DisConnectGateUnit;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public string AccountName { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.AccountName = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(InnerMessage.G2L_DisConnectGateUnit)]
    public partial class G2L_DisConnectGateUnit : MessageObject, IResponse
    {
        public static G2L_DisConnectGateUnit Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(G2L_DisConnectGateUnit), isFromPool) as G2L_DisConnectGateUnit;
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
    [Message(InnerMessage.G2L_AddLoginRecord)]
    [ResponseType(nameof(L2G_AddLoginRecord))]
    public partial class G2L_AddLoginRecord : MessageObject, IRequest
    {
        public static G2L_AddLoginRecord Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(G2L_AddLoginRecord), isFromPool) as G2L_AddLoginRecord;
        }

        /// <summary>
        /// 登录gate
        /// </summary>
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public string AccountName { get; set; }

        [MemoryPackOrder(2)]
        public int ServerId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.AccountName = default;
            this.ServerId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(InnerMessage.L2G_AddLoginRecord)]
    public partial class L2G_AddLoginRecord : MessageObject, IResponse
    {
        public static L2G_AddLoginRecord Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(L2G_AddLoginRecord), isFromPool) as L2G_AddLoginRecord;
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
    [Message(InnerMessage.G2L_RemoveLoginRecord)]
    [ResponseType(nameof(L2G_RemoveLoginRecord))]
    public partial class G2L_RemoveLoginRecord : MessageObject, IRequest
    {
        public static G2L_RemoveLoginRecord Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(G2L_RemoveLoginRecord), isFromPool) as G2L_RemoveLoginRecord;
        }

        /// <summary>
        /// 登录gate
        /// </summary>
        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public string AccountName { get; set; }

        [MemoryPackOrder(2)]
        public int ServerId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.AccountName = default;
            this.ServerId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(InnerMessage.L2G_RemoveLoginRecord)]
    public partial class L2G_RemoveLoginRecord : MessageObject, IResponse
    {
        public static L2G_RemoveLoginRecord Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(L2G_RemoveLoginRecord), isFromPool) as L2G_RemoveLoginRecord;
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
    [Message(InnerMessage.G2M_RequestExitGame)]
    [ResponseType(nameof(M2G_RequestExitGame))]
    public partial class G2M_RequestExitGame : MessageObject, ILocationRequest
    {
        public static G2M_RequestExitGame Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(G2M_RequestExitGame), isFromPool) as G2M_RequestExitGame;
        }

        /// <summary>
        /// 登录gate
        /// </summary>
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
    [Message(InnerMessage.M2G_RequestExitGame)]
    public partial class M2G_RequestExitGame : MessageObject, ILocationResponse
    {
        public static M2G_RequestExitGame Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(M2G_RequestExitGame), isFromPool) as M2G_RequestExitGame;
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
    [Message(InnerMessage.G2M_SecondLogin)]
    [ResponseType(nameof(M2G_SecondLogin))]
    public partial class G2M_SecondLogin : MessageObject, ILocationRequest
    {
        public static G2M_SecondLogin Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(G2M_SecondLogin), isFromPool) as G2M_SecondLogin;
        }

        /// <summary>
        /// 玩家二次登陆
        /// </summary>
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
    [Message(InnerMessage.M2G_SecondLogin)]
    public partial class M2G_SecondLogin : MessageObject, ILocationResponse
    {
        public static M2G_SecondLogin Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(M2G_SecondLogin), isFromPool) as M2G_SecondLogin;
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

    // *********************************
    // ***********玩家缓存相关***********
    // *********************************
    // ---------------------缓存服--------------------
    [MemoryPackable]
    [Message(InnerMessage.Other2UnitCache_AddOrUpdateUnit)]
    [ResponseType(nameof(UnitCache2Other_AddOrUpdateUnit))]
    public partial class Other2UnitCache_AddOrUpdateUnit : MessageObject, IRequest
    {
        public static Other2UnitCache_AddOrUpdateUnit Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(Other2UnitCache_AddOrUpdateUnit), isFromPool) as Other2UnitCache_AddOrUpdateUnit;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        /// <summary>
        /// 需要缓存的UnitId
        /// </summary>
        [MemoryPackOrder(1)]
        public long UnitId { get; set; }

        /// <summary>
        /// 实体类型
        /// </summary>
        [MemoryPackOrder(2)]
        public List<string> EntityTypes { get; set; } = new();

        /// <summary>
        /// 实体序列化后的bytes
        /// </summary>
        [MemoryPackOrder(3)]
        public List<byte[]> EntityBytes { get; set; } = new();

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.UnitId = default;
            this.EntityTypes.Clear();
            this.EntityBytes.Clear();

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(InnerMessage.UnitCache2Other_AddOrUpdateUnit)]
    public partial class UnitCache2Other_AddOrUpdateUnit : MessageObject, IResponse
    {
        public static UnitCache2Other_AddOrUpdateUnit Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(UnitCache2Other_AddOrUpdateUnit), isFromPool) as UnitCache2Other_AddOrUpdateUnit;
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
    [Message(InnerMessage.Other2UnitCache_GetUnit)]
    [ResponseType(nameof(UnitCache2Other_GetUnit))]
    public partial class Other2UnitCache_GetUnit : MessageObject, IRequest
    {
        public static Other2UnitCache_GetUnit Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(Other2UnitCache_GetUnit), isFromPool) as Other2UnitCache_GetUnit;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public long UnitId { get; set; }

        /// <summary>
        /// 需要获取的组件名
        /// </summary>
        [MemoryPackOrder(2)]
        public List<string> ComponentNameList { get; set; } = new();

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.UnitId = default;
            this.ComponentNameList.Clear();

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(InnerMessage.UnitCache2Other_GetUnit)]
    public partial class UnitCache2Other_GetUnit : MessageObject, IResponse
    {
        public static UnitCache2Other_GetUnit Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(UnitCache2Other_GetUnit), isFromPool) as UnitCache2Other_GetUnit;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        [MemoryPackOrder(3)]
        public List<byte[]> EntityList { get; set; } = new();

        [MemoryPackOrder(4)]
        public List<string> ComponentNameList { get; set; } = new();

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.EntityList.Clear();
            this.ComponentNameList.Clear();

            ObjectPool.Instance.Recycle(this);
        }
    }

    // -------------缓存服-------------
    // ---------------------排行榜--------------------
    [MemoryPackable]
    [Message(InnerMessage.Map2Rank_AddOrUpdateRankInfo)]
    public partial class Map2Rank_AddOrUpdateRankInfo : MessageObject, IMessage
    {
        public static Map2Rank_AddOrUpdateRankInfo Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(Map2Rank_AddOrUpdateRankInfo), isFromPool) as Map2Rank_AddOrUpdateRankInfo;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public RankInfoProto RankInfoProto { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.RankInfoProto = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    // -------------排行榜-------------
    // ---------------------邮箱--------------------
    [MemoryPackable]
    [Message(InnerMessage.Mail2M_CollectAttachment)]
    [ResponseType(nameof(M2Mail_CollectAttachment))]
    public partial class Mail2M_CollectAttachment : MessageObject, ILocationRequest
    {
        public static Mail2M_CollectAttachment Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(Mail2M_CollectAttachment), isFromPool) as Mail2M_CollectAttachment;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public List<ItemProto> AttachItems { get; set; } = new();

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.AttachItems.Clear();

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(InnerMessage.M2Mail_CollectAttachment)]
    public partial class M2Mail_CollectAttachment : MessageObject, ILocationResponse
    {
        public static M2Mail_CollectAttachment Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(M2Mail_CollectAttachment), isFromPool) as M2Mail_CollectAttachment;
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
    [Message(InnerMessage.G2Mail_LoginMailServer)]
    [ResponseType(nameof(Mail2G_LoginMailServer))]
    public partial class G2Mail_LoginMailServer : MessageObject, IRequest
    {
        public static G2Mail_LoginMailServer Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(G2Mail_LoginMailServer), isFromPool) as G2Mail_LoginMailServer;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public long UnitId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.UnitId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(InnerMessage.Mail2G_LoginMailServer)]
    public partial class Mail2G_LoginMailServer : MessageObject, IResponse
    {
        public static Mail2G_LoginMailServer Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(Mail2G_LoginMailServer), isFromPool) as Mail2G_LoginMailServer;
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
    [Message(InnerMessage.G2Mail_ExitMailServer)]
    [ResponseType(nameof(Mail2G_ExitMailServer))]
    public partial class G2Mail_ExitMailServer : MessageObject, IRequest
    {
        public static G2Mail_ExitMailServer Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(G2Mail_ExitMailServer), isFromPool) as G2Mail_ExitMailServer;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public long UnitId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.UnitId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(InnerMessage.Mail2G_ExitMailServer)]
    public partial class Mail2G_ExitMailServer : MessageObject, IResponse
    {
        public static Mail2G_ExitMailServer Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(Mail2G_ExitMailServer), isFromPool) as Mail2G_ExitMailServer;
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

    // -------------邮箱-------------
    // -------------关系服-------------
    [MemoryPackable]
    [Message(InnerMessage.G2Relationship_LoginRelationshipServer)]
    [ResponseType(nameof(Relationship2G_LoginRelationshipServer))]
    public partial class G2Relationship_LoginRelationshipServer : MessageObject, IRequest
    {
        public static G2Relationship_LoginRelationshipServer Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(G2Relationship_LoginRelationshipServer), isFromPool) as G2Relationship_LoginRelationshipServer;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public long UnitId { get; set; }

        [MemoryPackOrder(2)]
        public long TeamId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.UnitId = default;
            this.TeamId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(InnerMessage.Relationship2G_LoginRelationshipServer)]
    public partial class Relationship2G_LoginRelationshipServer : MessageObject, IResponse
    {
        public static Relationship2G_LoginRelationshipServer Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(Relationship2G_LoginRelationshipServer), isFromPool) as Relationship2G_LoginRelationshipServer;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        [MemoryPackOrder(3)]
        public long ClearUnitTeamId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.ClearUnitTeamId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(InnerMessage.G2Relationship_ExitRelationshipServer)]
    [ResponseType(nameof(Relationship2G_ExitRelationshipServer))]
    public partial class G2Relationship_ExitRelationshipServer : MessageObject, IRequest
    {
        public static G2Relationship_ExitRelationshipServer Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(G2Relationship_ExitRelationshipServer), isFromPool) as G2Relationship_ExitRelationshipServer;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public long UnitId { get; set; }

        [MemoryPackOrder(2)]
        public long TeamId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.UnitId = default;
            this.TeamId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(InnerMessage.Relationship2G_ExitRelationshipServer)]
    public partial class Relationship2G_ExitRelationshipServer : MessageObject, IResponse
    {
        public static Relationship2G_ExitRelationshipServer Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(Relationship2G_ExitRelationshipServer), isFromPool) as Relationship2G_ExitRelationshipServer;
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
    [Message(InnerMessage.G2M_GetUnitTeamId)]
    [ResponseType(nameof(M2G_GetUnitTeamId))]
    public partial class G2M_GetUnitTeamId : MessageObject, ILocationRequest
    {
        public static G2M_GetUnitTeamId Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(G2M_GetUnitTeamId), isFromPool) as G2M_GetUnitTeamId;
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
    [Message(InnerMessage.M2G_GetUnitTeamId)]
    public partial class M2G_GetUnitTeamId : MessageObject, ILocationResponse
    {
        public static M2G_GetUnitTeamId Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(M2G_GetUnitTeamId), isFromPool) as M2G_GetUnitTeamId;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        [MemoryPackOrder(3)]
        public long TeamId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.TeamId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(InnerMessage.G2Relationship_CreateTeam)]
    [ResponseType(nameof(Relationship2G_CreateTeam))]
    public partial class G2Relationship_CreateTeam : MessageObject, IRequest
    {
        public static G2Relationship_CreateTeam Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(G2Relationship_CreateTeam), isFromPool) as G2Relationship_CreateTeam;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public long UnitId { get; set; }

        [MemoryPackOrder(2)]
        public string TeamName { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.UnitId = default;
            this.TeamName = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(InnerMessage.Relationship2G_CreateTeam)]
    public partial class Relationship2G_CreateTeam : MessageObject, IResponse
    {
        public static Relationship2G_CreateTeam Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(Relationship2G_CreateTeam), isFromPool) as Relationship2G_CreateTeam;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        [MemoryPackOrder(3)]
        public long TeamId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.TeamId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(InnerMessage.G2M_SetUnitTeamId)]
    [ResponseType(nameof(M2G_SetUnitTeamId))]
    public partial class G2M_SetUnitTeamId : MessageObject, ILocationRequest
    {
        public static G2M_SetUnitTeamId Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(G2M_SetUnitTeamId), isFromPool) as G2M_SetUnitTeamId;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public long TeamId { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.TeamId = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(InnerMessage.M2G_SetUnitTeamId)]
    public partial class M2G_SetUnitTeamId : MessageObject, ILocationResponse
    {
        public static M2G_SetUnitTeamId Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(M2G_SetUnitTeamId), isFromPool) as M2G_SetUnitTeamId;
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
    [Message(InnerMessage.G2Relationship_LeaveTeam)]
    [ResponseType(nameof(Relationship2G_LeaveTeam))]
    public partial class G2Relationship_LeaveTeam : MessageObject, IRequest
    {
        public static G2Relationship_LeaveTeam Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(G2Relationship_LeaveTeam), isFromPool) as G2Relationship_LeaveTeam;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public long UnitId { get; set; }

        [MemoryPackOrder(2)]
        public long TeamId { get; set; }

        [MemoryPackOrder(3)]
        public long Dissolve { get; set; }

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.UnitId = default;
            this.TeamId = default;
            this.Dissolve = default;

            ObjectPool.Instance.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(InnerMessage.Relationship2G_LeaveTeam)]
    public partial class Relationship2G_LeaveTeam : MessageObject, IResponse
    {
        public static Relationship2G_LeaveTeam Create(bool isFromPool = false)
        {
            return ObjectPool.Instance.Fetch(typeof(Relationship2G_LeaveTeam), isFromPool) as Relationship2G_LeaveTeam;
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }

        [MemoryPackOrder(1)]
        public int Error { get; set; }

        [MemoryPackOrder(2)]
        public string Message { get; set; }

        [MemoryPackOrder(3)]
        public List<long> ClearUnitIds { get; set; } = new();

        public override void Dispose()
        {
            if (!this.IsFromPool)
            {
                return;
            }

            this.RpcId = default;
            this.Error = default;
            this.Message = default;
            this.ClearUnitIds.Clear();

            ObjectPool.Instance.Recycle(this);
        }
    }

    // -------------关系服-------------
    // -------------相关GM-------------
    // -------------相关GM-------------
    public static class InnerMessage
    {
        public const ushort ObjectQueryRequest = 20002;
        public const ushort M2A_Reload = 20003;
        public const ushort A2M_Reload = 20004;
        public const ushort G2G_LockRequest = 20005;
        public const ushort G2G_LockResponse = 20006;
        public const ushort G2G_LockReleaseRequest = 20007;
        public const ushort G2G_LockReleaseResponse = 20008;
        public const ushort ObjectAddRequest = 20009;
        public const ushort ObjectAddResponse = 20010;
        public const ushort ObjectLockRequest = 20011;
        public const ushort ObjectLockResponse = 20012;
        public const ushort ObjectUnLockRequest = 20013;
        public const ushort ObjectUnLockResponse = 20014;
        public const ushort ObjectRemoveRequest = 20015;
        public const ushort ObjectRemoveResponse = 20016;
        public const ushort ObjectGetRequest = 20017;
        public const ushort ObjectGetResponse = 20018;
        public const ushort R2G_GetLoginKey = 20019;
        public const ushort G2R_GetLoginKey = 20020;
        public const ushort G2M_SessionDisconnect = 20021;
        public const ushort ObjectQueryResponse = 20022;
        public const ushort M2M_UnitTransferRequest = 20023;
        public const ushort M2M_UnitTransferResponse = 20024;
        public const ushort M2M_InitMap = 20025;
        public const ushort M2M_MapCloseTransfer = 20026;
        public const ushort O2M_GetMapActorIdRequest = 20027;
        public const ushort M2O_GetMapActorIdResponse = 20028;
        public const ushort O2M_EnterMap = 20029;
        public const ushort O2M_CreateMapRequest = 20030;
        public const ushort M2O_CreateMapResponse = 20031;
        public const ushort R2L_LoginAccount = 20032;
        public const ushort L2R_LoginAccount = 20033;
        public const ushort L2G_DisConnectGateUnit = 20034;
        public const ushort G2L_DisConnectGateUnit = 20035;
        public const ushort G2L_AddLoginRecord = 20036;
        public const ushort L2G_AddLoginRecord = 20037;
        public const ushort G2L_RemoveLoginRecord = 20038;
        public const ushort L2G_RemoveLoginRecord = 20039;
        public const ushort G2M_RequestExitGame = 20040;
        public const ushort M2G_RequestExitGame = 20041;
        public const ushort G2M_SecondLogin = 20042;
        public const ushort M2G_SecondLogin = 20043;
        public const ushort Other2UnitCache_AddOrUpdateUnit = 20044;
        public const ushort UnitCache2Other_AddOrUpdateUnit = 20045;
        public const ushort Other2UnitCache_GetUnit = 20046;
        public const ushort UnitCache2Other_GetUnit = 20047;
        public const ushort Map2Rank_AddOrUpdateRankInfo = 20048;
        public const ushort Mail2M_CollectAttachment = 20049;
        public const ushort M2Mail_CollectAttachment = 20050;
        public const ushort G2Mail_LoginMailServer = 20051;
        public const ushort Mail2G_LoginMailServer = 20052;
        public const ushort G2Mail_ExitMailServer = 20053;
        public const ushort Mail2G_ExitMailServer = 20054;
        public const ushort G2Relationship_LoginRelationshipServer = 20055;
        public const ushort Relationship2G_LoginRelationshipServer = 20056;
        public const ushort G2Relationship_ExitRelationshipServer = 20057;
        public const ushort Relationship2G_ExitRelationshipServer = 20058;
        public const ushort G2M_GetUnitTeamId = 20059;
        public const ushort M2G_GetUnitTeamId = 20060;
        public const ushort G2Relationship_CreateTeam = 20061;
        public const ushort Relationship2G_CreateTeam = 20062;
        public const ushort G2M_SetUnitTeamId = 20063;
        public const ushort M2G_SetUnitTeamId = 20064;
        public const ushort G2Relationship_LeaveTeam = 20065;
        public const ushort Relationship2G_LeaveTeam = 20066;
    }
}