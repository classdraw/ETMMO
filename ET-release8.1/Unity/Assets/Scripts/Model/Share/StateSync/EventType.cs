using System;

namespace ET
{
    public struct SceneChangeStart
    {
    }
    
    public struct SceneChangeFinish
    {
    }
    
    public struct AfterCreateClientScene
    {
    }
    
    public struct AfterCreateCurrentScene
    {
    }

    public struct AppStartInitFinish
    {
    }

    public struct LoginFinish
    {
    }

    public struct EnterMapFinish
    {
    }

    public struct AfterUnitCreate
    {
        public Unit Unit;
    }

    /// <summary>
    /// 主角 Unit View 创建完成，供相机等全局显示层组件绑定。
    /// </summary>
    public struct MainPlayerUnitViewCreate
    {
        public Unit Unit;
    }

    public struct TestEventSee
    {
        public int TestValue;
    }

    /// <summary>
    /// NetClient 会话断开，通知显示层（如打开 UISessionError）。
    /// </summary>
    public struct SessionDisposeNotify
    {
        public int Error;
    }

    public struct UnitGetComponent
    {
        public EntityRef<Unit> Unit;
        public Type Type;
    }
}