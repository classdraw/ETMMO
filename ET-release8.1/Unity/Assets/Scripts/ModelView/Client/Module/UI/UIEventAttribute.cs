using System;

namespace ET.Client
{
    public class UIEventAttribute: BaseAttribute
    {
        public string UIType { get; }

        public int UISortingOrder { get; }

        public bool FullScreen { get; }

        public UIEventAttribute(string uiType, int uiSortingOrder = 0, bool fullScreen = false)
        {
            this.UIType = uiType;
            this.UISortingOrder = uiSortingOrder;
            this.FullScreen = fullScreen;
        }
    }
}