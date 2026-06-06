using Sirenix.OdinInspector;

namespace ET.Server
{
    [EntitySystemOf(typeof (MapUnit))]
    [FriendOf(typeof(MapUnit))]
    public static partial class MapUnitSystem
    {
        [EntitySystem]
        private static void Awake(this MapUnit self, int mapId)
        {
            self.mapConfigId = mapId;
        }

        [EntitySystem]
        private static void Destroy(this MapUnit self)
        {
            self.mapConfigId = 0;

            self.count = 0;
            self.closeTime = 0;
            self.validTime = 0;
            
            self.fiberId = 0;
            self.actorId=default;
            self.actorStr = string.Empty;
            self.ctx = default;
            
        }

        public static void AddCount(this MapUnit self)
        {
            self.count++;
        }
        
        public static void RemoveCount(this MapUnit self)
        {
            self.count--;
        }

        /// <summary>
        /// 地图是否已经满了
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool IsFull(this MapUnit self)
        {
            return self.count >= self.MapConfig.MaxPlayer;
        }
        
        /// <summary>
        /// 当前地图是否可用
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool IsAvailable(this MapUnit self)
        {
            //self.validTime > 0 待销毁状态
            //(self.closeTime > 0 && TimeInfo.Instance.FrameTime >= self.closeTime) 关闭状态
            if (self.validTime > 0 || (self.closeTime > 0 && TimeInfo.Instance.FrameTime >= self.closeTime))
            {
                return false;
            }

            if (self.IsFull())
            {
                return false;
            }

            return true;
        }
    }
}

