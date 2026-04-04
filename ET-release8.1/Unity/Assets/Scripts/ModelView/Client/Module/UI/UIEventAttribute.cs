using System;

namespace ET.Client
{
    public class UIEventAttribute: BaseAttribute
    {
        public string UIType { get; }

        public int UISortingOrder { get; }

        public UIEventAttribute(string uiType, int uiSortingOrder = 0)
        {
            this.UIType = uiType;
            this.UISortingOrder = uiSortingOrder;
        }
    }
}