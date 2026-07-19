namespace ET.Client
{
    //buff添加
    [Event(SceneType.Current)]
    public class BuffAdd_BuffView:AEvent<Scene,BuffAdd>
    {
        protected override async ETTask Run(Scene scene, BuffAdd args)
        {
            Unit unit = args.Unit;
            if (unit == null || unit.IsDisposed)
            {
                return;
            }

            if (!BuffConfigCategory.Instance.Contain(args.BuffConfigId))
            {
                Log.Error($"BuffAdd_BuffView BuffConfig not found: {args.BuffConfigId}");
                return;
            }

            BuffConfig buffConfig = BuffConfigCategory.Instance.Get(args.BuffConfigId);
            int[] ownerEffects = buffConfig.OwnerEffect;
            if (ownerEffects == null || ownerEffects.Length == 0)
            {
                return;
            }

            MountComponent mountComponent = unit.GetComponent<MountComponent>();
            if (mountComponent == null || mountComponent.IsDisposed)
            {
                return;
            }

            foreach (int effectConfigId in ownerEffects)
            {
                if (effectConfigId == 0)
                {
                    continue;
                }
                
                await mountComponent.MountEffect(effectConfigId, args.BuffId);
            }
        }
    }
}
