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
    /// UI层级。数值直接作为 Canvas.sortingOrder。
    /// Bottom 常驻主界面；Popup 弹窗，叠在主界面上且不隐藏主界面。
    /// </summary>
    public enum UISortingOrder : int
    {
        Bottom = 0,
        UI = 1,
        Popup = 2,
        Tips = 3,
        System = 4,
    }
    
    
    public class UILayerScript: MonoBehaviour
    {
        //public UILayer UILayer;
    }
}