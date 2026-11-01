using System.Collections.Generic;

namespace ET
{
    public static class KnapsackSlotHelper
    {
        public const int UnassignedIndex = -1;

        public static bool IsValidSlotIndex(int index, int maxLoad)
        {
            return index >= 0 && index < maxLoad;
        }

        public static void CollectOccupiedIndices(IEnumerable<Item> items, int maxLoad, HashSet<int> occupied, long excludeItemId = 0)
        {
            occupied.Clear();
            if (items == null)
            {
                return;
            }

            foreach (Item item in items)
            {
                if (item == null || item.IsDisposed || item.Id == excludeItemId)
                {
                    continue;
                }

                if (IsValidSlotIndex(item.Index, maxLoad))
                {
                    occupied.Add(item.Index);
                }
            }
        }

        public static int FindFirstFreeSlotIndex(HashSet<int> occupied, int maxLoad)
        {
            for (int i = 0; i < maxLoad; i++)
            {
                if (!occupied.Contains(i))
                {
                    return i;
                }
            }

            return UnassignedIndex;
        }

        public static int GetFreeSlotCount(int maxLoad, HashSet<int> occupied)
        {
            return maxLoad - occupied.Count;
        }
    }
}
