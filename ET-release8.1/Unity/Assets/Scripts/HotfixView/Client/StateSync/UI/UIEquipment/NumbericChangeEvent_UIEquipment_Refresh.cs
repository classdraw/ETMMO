namespace ET.Client
{
    [Event(SceneType.Current)]
    public class NumbericChangeEvent_UIEquipment_Refresh : AEvent<Scene, NumbericChange>
    {
        protected override async ETTask Run(Scene scene, NumbericChange args)
        {
            Unit unit = args.Unit;
            if (unit == null || unit.IsDisposed || scene == null || !unit.IsMyUnit(scene))
            {
                await ETTask.CompletedTask;
                return;
            }

            UI equipmentUi = scene.GetComponent<UIComponent>()?.Get(UIType.UIEquipment);
            equipmentUi?.GetComponent<UIEquipmentComponent>()?.RefreshPlayerInfo();

            await ETTask.CompletedTask;
        }
    }
}