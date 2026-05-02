namespace ET
{
    [EntitySystemOf(typeof(Item))]
    [FriendOf(typeof(Item))]
    public static partial class ItemSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Item self, int configId)
        {

            self.ConfigId = configId;
        }
        [EntitySystem]
        private static void Destroy(this ET.Item self)
        {
            self.ConfigId = default;
            self.ContainerType = (int)KnapsackContainerType.None;
        }

        public static void FromMessage(this Item self, ItemProto itemProto)
        {
            self.ConfigId = itemProto.ConfigId;
            self.ContainerType = itemProto.ContainerType;
            self.Count = itemProto.Count;
            self.Quality = itemProto.ItemQuality;
            
            /**
             
            if (itemProto.EquipInfo != null)
            {
                EquipInfoComponent equipInfoComponent = self.GetComponent<EquipInfoComponent>();
                if (equipInfoComponent == null)
                {
                    equipInfoComponent = self.AddComponent<EquipInfoComponent>();
                }
                equipInfoComponent.FromMessage(itemProto.EquipInfo);
                
            }
             */
        }

        public static ItemProto ToMessage(this Item self,bool isAllInfo = true)
        {
            ItemProto message = ItemProto.Create(true);
            message.ConfigId = self.ConfigId;
            message.ContainerType = self.ContainerType;
            message.Id = self.Id;
            message.Count = self.Count;
            
            if (!isAllInfo)
            {
                return message;
            }
            
            /**
             * EquipInfoComponent equipInfoComponent = self.GetComponent<EquipInfoComponent>();
            if (equipInfoComponent != null)
            {
                message.EquipInfo = equipInfoComponent.ToMessage();
            }

            return message;
             */
            return null;
        }
    }
}

