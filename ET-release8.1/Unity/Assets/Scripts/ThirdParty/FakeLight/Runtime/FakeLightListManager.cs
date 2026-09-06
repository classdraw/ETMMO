using System;
using System.Collections.Generic;

namespace CenturyGame.FakeLight
{
    public class FakeLightListManager
    {
        public static FakeLightListManager Instance => _instance.Value;
        static Lazy<FakeLightListManager> _instance = new(() => new FakeLightListManager());
        public List<FakeLight> activeLights = new List<FakeLight>();

        public void Register(FakeLight light)
        {
            if (!activeLights.Contains(light))
            {
                OrderInsert(light);
            }
        }

        public void Unregister(FakeLight light)
        {
            if (activeLights.Contains(light))
            {
                activeLights.Remove(light);
            }
        }

        void OrderInsert(FakeLight light)
        {
            int index = 0;
            while (index < activeLights.Count && activeLights[index].priority > light.priority)
            {
                index++;
            }
            activeLights.Insert(index, light);
        }

        // 暂时只能在FakeLightManager里调用，每帧可能会多次调用
        // 用插入排序，稳定排序且最好时间复杂度是O(n)
        public void OrderUpdate()
        {
            for (int i = 1; i < activeLights.Count; i++)
            {
                var temp = activeLights[i];
                int j = i - 1;

                // Sort order is ascending
                while (j >= 0 && activeLights[j].priority < temp.priority)
                {
                    activeLights[j + 1] = activeLights[j];
                    j--;
                }

                activeLights[j + 1] = temp;
            }
        }
    }
}
