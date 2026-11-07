using System.Collections.Generic;
using GameLogic;
using SuperScrollView;
using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    [EntitySystemOf(typeof(UIBagComponent))]
    [FriendOf(typeof(UIBagComponent))]
    [FriendOfAttribute(typeof(ET.Client.IconAtlasComponent))]
    public static partial class UIBagComponentSystem
    {
        [EntitySystem]
        private static void Awake(this UIBagComponent self)
        {
            UIBindComponent bind = self.GetParent<UI>().GameObject.GetComponent<UIBindComponent>();
            self.m_btnClose = bind.GetComponent<Button>(0);
            self.m_loopList2 = bind.GetComponent<LoopListView2>(1);
            self.m_btnClose.onClick.AddListener(() => { self.OnCloseAsync().Coroutine(); });
        }

        [EntitySystem]
        private static void Destroy(this UIBagComponent self)
        {
            self.m_loopList2 = null;
            self.SlotItems = null;
            self.ListInited = false;
        }

        private static IconAtlasComponent GetIconAtlas(this UIBagComponent self)
        {
            return self.GetParent<UI>().GetIconAtlas();
        }

        public static async ETTask ReloadFromServerAsync(this UIBagComponent self)
        {
            await self.LoadBagFromServerAsync();
        }

        public static void RefreshFromClientKnapsack(this UIBagComponent self)
        {
            self.RebuildSlotItemsFromKnapsack();
            self.RefreshBagListView();
            self.RefreshBagIconsAsync().Coroutine();
        }

        private static async ETTask RefreshBagIconsAsync(this UIBagComponent self)
        {
            await self.EnsureIconAtlasAsync();
            self.RefreshBagListViewIfAlive();
        }

        public static async ETTask OnCloseAsync(this UIBagComponent self)
        {
            await UIHelper.Remove(self.Scene(), UIType.UIBag);
        }

        private static async ETTask LoadBagFromServerAsync(this UIBagComponent self)
        {
            Log.Info("[UIBag] LoadBagFromServerAsync 开始");
            int error = await KnapsackHelper.GetAllItems(self.Root());
            if (self.IsDisposed)
            {
                return;
            }

            if (error != ErrorCode.ERR_Success)
            {
                Log.Error($"UIBag 拉取背包失败 Error={error}");
                self.SlotItems = new EntityRef<Item>[KnapsackConfigHelper.GetKnapsackMaxLoad()];
            }
            else
            {
                self.RebuildSlotItemsFromKnapsack();
            }

            self.InitBagList();
            self.RefreshBagListView();
            Log.Info($"[UIBag] 列表已刷新，背包格内物品数={self.CountOccupiedSlots()}，开始加载图标");
            await self.EnsureIconAtlasAsync();
            self.RefreshBagListViewIfAlive();
            if (self.IsDisposed)
            {
                return;
            }

            IconAtlasComponent atlas = self.GetIconAtlas();
            Log.Info($"[UIBag] 界面已刷新完成，图标缓存数={atlas?.SpriteCache?.Count ?? 0}");
        }

        private static void RefreshBagListViewIfAlive(this UIBagComponent self)
        {
            if (self.IsDisposed)
            {
                return;
            }

            self.RefreshBagListView();
        }

        private static int CountOccupiedSlots(this UIBagComponent self)
        {
            if (self.SlotItems == null)
            {
                return 0;
            }

            int n = 0;
            for (int i = 0; i < self.SlotItems.Length; i++)
            {
                Item item = self.SlotItems[i];
                if (item != null)
                {
                    n++;
                }
            }

            return n;
        }

        private static void RefreshBagListView(this UIBagComponent self)
        {
            if (!self.ListInited || self.m_loopList2 == null)
            {
                return;
            }

            self.m_loopList2.RefreshAllShownItem();
        }

        /// <summary>从 ClientKnapsack 背包容器 ListComponent&lt;Item&gt; 映射到 ConstantConfig 90001 格数的 UI。</summary>
        private static void RebuildSlotItemsFromKnapsack(this UIBagComponent self)
        {
            int totalSlots = KnapsackConfigHelper.GetKnapsackMaxLoad();
            self.SlotItems = new EntityRef<Item>[totalSlots];

            ClientKnapsackComponent knapsack = self.Root().GetComponent<ClientKnapsackComponent>();
            if (knapsack == null)
            {
                Log.Error("UIBag 未找到 ClientKnapsackComponent");
                return;
            }

            using (ListComponent<Item> items = ListComponent<Item>.Create())
            {
                knapsack.GetInventoryItems(items);
                foreach (Item item in items)
                {
                    if (item == null || item.IsDisposed)
                    {
                        continue;
                    }

                    if (!KnapsackSlotHelper.IsValidSlotIndex(item.Index, totalSlots))
                    {
                        Log.Warning($"[UIBag] 物品 Index 非法，已忽略 ItemId={item.Id}, Index={item.Index}");
                        continue;
                    }

                    Item itemTemp = self.SlotItems[item.Index];
                    if (itemTemp != null)
                    {
                        Log.Warning($"[UIBag] 格子 Index={item.Index} 重复，ItemId={item.Id}");
                        continue;
                    }

                    self.SlotItems[item.Index] = item;
                }
            }
        }

        private static void InitBagList(this UIBagComponent self)
        {
            if (self.ListInited || self.m_loopList2 == null)
            {
                if (self.m_loopList2 == null)
                {
                    Log.Error("UIBag 未绑定 m_loopList2（LoopListView2）");
                }

                return;
            }

            int totalSlots = KnapsackConfigHelper.GetKnapsackMaxLoad();
            int rowCount = (totalSlots + UIBagComponent.SlotsPerRow - 1) / UIBagComponent.SlotsPerRow;
            self.m_loopList2.InitListView(rowCount, (listView, rowIndex) => self.OnGetRowItemByIndex(listView, rowIndex));
            self.ListInited = true;
        }

        private static LoopListViewItem2 OnGetRowItemByIndex(this UIBagComponent self, LoopListView2 listView, int rowIndex)
        {
            LoopListViewItem2 rowItem = listView.NewListViewItem(UIBagComponent.RowItemPrefabName);
            if (rowItem == null)
            {
                Log.Error($"UIBag 创建列表项失败，Prefab 名应为 {UIBagComponent.RowItemPrefabName}");
                return null;
            }

            if (!rowItem.IsInitHandlerCalled)
            {
                rowItem.IsInitHandlerCalled = true;
                self.BindRowSlotClicks(rowItem);
            }

            self.RefreshRow(rowItem, rowIndex);
            return rowItem;
        }

        private static void BindRowSlotClicks(this UIBagComponent self, LoopListViewItem2 rowItem)
        {
            Transform rowTransform = rowItem.transform;
            for (int col = 0; col < UIBagComponent.SlotsPerRow; col++)
            {
                int column = col;
                Transform slotTransform = rowTransform.Find($"Slot{column + 1}");
                if (slotTransform == null)
                {
                    continue;
                }

                Transform backTransform = slotTransform.Find("Back");
                if (backTransform == null)
                {
                    continue;
                }

                Button button = backTransform.GetComponent<Button>();
                if (button == null)
                {
                    button = backTransform.gameObject.AddComponent<Button>();
                    button.transition = Selectable.Transition.None;
                }

                button.onClick.AddListener(() => self.OnSlotClicked(rowItem, column));
            }
        }

        private static Item GetSlotItem(this UIBagComponent self, int slotIndex)
        {
            if (self.SlotItems == null || slotIndex < 0 || slotIndex >= self.SlotItems.Length)
            {
                return null;
            }

            return self.SlotItems[slotIndex];
        }

        private static void RefreshRow(this UIBagComponent self, LoopListViewItem2 rowItem, int rowIndex)
        {
            Transform rowTransform = rowItem.transform;
            for (int col = 0; col < UIBagComponent.SlotsPerRow; col++)
            {
                int slotIndex = rowIndex * UIBagComponent.SlotsPerRow + col;
                Transform slotTransform = rowTransform.Find($"Slot{col + 1}");
                if (slotTransform == null)
                {
                    continue;
                }

                if (slotIndex >= KnapsackConfigHelper.GetKnapsackMaxLoad())
                {
                    slotTransform.gameObject.SetActive(false);
                    continue;
                }

                slotTransform.gameObject.SetActive(true);
                Item item = self.GetSlotItem(slotIndex);

                self.RefreshItemImage(slotTransform, item, slotIndex);
                self.RefreshItemCount(slotTransform, item);
            }
        }

        private static void RefreshItemImage(this UIBagComponent self, Transform slotTransform, Item item, int slotIndex)
        {
            Transform itemImageTransform = slotTransform.Find("ItemImage");
            if (itemImageTransform == null)
            {
                return;
            }

            Image itemImage = itemImageTransform.GetComponent<Image>();
            if (itemImage == null)
            {
                return;
            }

            if (item == null)
            {
                itemImage.sprite = null;
                itemImageTransform.gameObject.SetActive(false);
                return;
            }

            if (!TryGetItemConfig(item.ConfigId, out ItemConfig config) || string.IsNullOrEmpty(config.Icon))
            {
                itemImage.sprite = null;
                itemImageTransform.gameObject.SetActive(false);
                Log.Warning($"[UIBag] 物品 ConfigId={item.ConfigId} 未配置 Icon");
                return;
            }

            string iconKey = config.Icon;
            IconAtlasComponent itemAtlas = self.GetIconAtlas();
            if (itemAtlas != null && itemAtlas.TryGetSprite(iconKey, out Sprite cached))
            {
                itemImage.sprite = cached;
                itemImageTransform.gameObject.SetActive(true);
                itemImage.enabled = true;
                return;
            }

            itemImage.sprite = null;
            itemImageTransform.gameObject.SetActive(false);
            self.LoadItemIconAsync(itemImage, iconKey, slotIndex, item.Id).Coroutine();
        }

        private static async ETTask LoadItemIconAsync(this UIBagComponent self, Image itemImage, string iconKey, int slotIndex,
            long itemInstanceId)
        {
            IconAtlasComponent atlas = self.GetIconAtlas();
            Sprite sprite = atlas == null
                ? null
                : await atlas.GetSpriteAsync(iconKey, IconAtlasComponentSystem.CollectItemConfigIconNames());
            if (self.IsDisposed || itemImage == null)
            {
                return;
            }

            if (sprite == null)
            {
                if (atlas != null && !atlas.LoadFailed)
                {
                    Log.Warning(
                        $"[UIBag] 图集中无此 Icon={iconKey}，请核对 ItemConfig 与图集内 Sprite 名称，Atlas={IconAtlasComponent.AtlasLocation}");
                }

                return;
            }

            Item item = self.GetSlotItem(slotIndex);
            if (item == null || item.Id != itemInstanceId)
            {
                return;
            }

            itemImage.sprite = sprite;
            itemImage.enabled = true;
            itemImage.gameObject.SetActive(true);
        }

        private static async ETTask EnsureIconAtlasAsync(this UIBagComponent self)
        {
            IconAtlasComponent atlas = self.GetIconAtlas();
            if (atlas == null)
            {
                return;
            }

            await atlas.EnsureIndexedAsync(IconAtlasComponentSystem.CollectItemConfigIconNames());
        }

        /// <summary>仅把堆叠数量写入 Prefab 上的 ItemCount（Text），不再使用 StackCount。</summary>
        private static void RefreshItemCount(this UIBagComponent self, Transform slotTransform, Item item)
        {
            Transform labelTransform = slotTransform.Find("ItemCount");
            if (labelTransform == null)
            {
                return;
            }

            Text label = labelTransform.GetComponent<Text>();
            if (label == null)
            {
                return;
            }

            if (item == null)
            {
                label.text = string.Empty;
                labelTransform.gameObject.SetActive(false);
                return;
            }

            labelTransform.gameObject.SetActive(true);
            label.text = item.Count.ToString();
        }

        private static void OnSlotClicked(this UIBagComponent self, LoopListViewItem2 rowItem, int column)
        {
            int slotIndex = rowItem.ItemIndex * UIBagComponent.SlotsPerRow + column;
            if (slotIndex < 0 || slotIndex >= KnapsackConfigHelper.GetKnapsackMaxLoad())
            {
                return;
            }

            Item item = self.GetSlotItem(slotIndex);
            if (item != null)
            {
                string name = GetItemDisplayName(item.ConfigId);
                Log.Info(
                    $"[UIBag] 点击格子 {slotIndex} | ItemInstanceId={item.Id}, ConfigId={item.ConfigId}, Name={name}, Count={item.Count}");
            }
            else
            {
                Log.Info($"[UIBag] 点击空格子 {slotIndex}");
            }
        }

        private static bool TryGetItemConfig(int configId, out ItemConfig config)
        {
            if (ItemConfigCategory.Instance.Contain(configId))
            {
                config = ItemConfigCategory.Instance.Get(configId);
                return true;
            }

            config = null;
            return false;
        }

        private static string GetItemDisplayName(int configId)
        {
            return TryGetItemConfig(configId, out ItemConfig config) ? config.Name : string.Empty;
        }
    }
}
