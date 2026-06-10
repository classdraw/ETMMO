using System.Collections.Generic;

namespace ET
{
    public struct CastActionTimes
    {
        public int Index;
        public bool IsSelfHit;
    }

    public partial class CastConfig
    {
        public List<int> Times = new List<int>();
        public MultiMap<int, CastActionTimes> TimesDict = new MultiMap<int, CastActionTimes>();

        public override void EndInit()
        {
            for (int i=0;i<this.SelfHitActionTimes.Length;i++)
            {
                int time = this.SelfHitActionTimes[i];
                if (!this.Times.Contains(time))
                {
                    this.Times.Add(time);
                }
                this.TimesDict.Add(time,new CastActionTimes(){Index = i,IsSelfHit = true});
            }
            
            for (int i=0;i<this.HitActionTimes.Length;i++)
            {
                int time = this.HitActionTimes[i];
                if (!this.Times.Contains(time))
                {
                    this.Times.Add(time);
                }
                this.TimesDict.Add(time,new CastActionTimes(){Index = i,IsSelfHit = false});
            }
            this.Times.Sort();
        }
    }
}

