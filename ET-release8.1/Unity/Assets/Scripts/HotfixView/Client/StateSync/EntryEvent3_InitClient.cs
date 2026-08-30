using System;
using System.Collections.Generic;
using System.IO;

namespace ET.Client
{
    [Event(SceneType.Main)]
    public class EntryEvent3_InitClient: AEvent<Scene, EntryEvent3>
    {
        protected override async ETTask Run(Scene root, EntryEvent3 args)
        {
            GlobalComponent globalComponent = root.AddComponent<GlobalComponent>();

            root.AddComponent<ResourcesLoaderComponent>();
            root.AddComponent<PoolComponent>();
            //显示层框架入口 资源加载用到ResourcesLoaderComponent 在这个后面
            await root.AddComponent<TEngineComponent>().Init();
            //相机需要一些准备（MainCamera + CameraRoot）
            await root.AddComponent<CameraPlayComponent>().Init();
            //角色贴图准备
            await root.AddComponent<RoleTextureComponent>().Init();
            //hud需要一些准备
            root.AddComponent<HudComponent>();
            
            root.AddComponent<UIGlobalComponent>();
            root.AddComponent<UIComponent>();
            
            root.AddComponent<PlayerComponent>();
            root.AddComponent<NetworkCacheComponent>();
            root.AddComponent<CurrentScenesComponent>();

            root.AddComponent<ClientKnapsackComponent>();
            
            root.AddComponent<RankComponent>();
            
            // 根据配置修改掉Main Fiber的SceneType
            SceneType sceneType = EnumHelper.FromString<SceneType>(globalComponent.GlobalConfig.AppType.ToString());
            root.SceneType = sceneType;
            
            await EventSystem.Instance.PublishAsync(root, new AppStartInitFinish());
            //await EventSystem.Instance.PublishAsync(root, new TestEventSee());
        }
    }
}