using UnityEngine;

namespace ET
{
    /** 废弃  现在用sort
    public enum UILayer
    {
        Hidden = 0,
        Low = 10,
        Mid = 20,
        High = 30,
    }*/
        
    /// <summary>
    /// UI层级枚举。
    /// </summary>
    public enum UISortingOrder : int
    {
        Bottom = 0,
        UI = 1,
        Top = 2,
        Tips = 3,
        System = 4,
    }
    
    
    public class UILayerScript: MonoBehaviour
    {
        //public UILayer UILayer;
    }
}