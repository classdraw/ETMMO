using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    [EntitySystemOf(typeof(UI))]
    [FriendOf(typeof(UI))]
    public static partial class UISystem
    {

        [EntitySystem]
        private static void Awake(this UI self, string name, GameObject gameObject, int uiSortingOrder)
        {
            self.nameChildren.Clear();
            gameObject.layer = UIType.WINDOW_SHOW_LAYER;
            self.FullScreen = false;
            self.IsHide = false;
            self.Name = name;
            self.GameObject = gameObject;
            
            self.UICanvas = gameObject.GetComponent<Canvas>();
            self.ChildCanvases.Clear();
            if (self.UICanvas != null)
            {
                self.UICanvas.overrideSorting = true;
                self.UICanvas.sortingOrder = uiSortingOrder;
                self.UICanvas.sortingLayerName = "Default";

                Canvas[] allCanvas = gameObject.GetComponentsInChildren<Canvas>(true);
                List<Canvas> childCanvases = new List<Canvas>(allCanvas.Length);
                foreach (Canvas canvas in allCanvas)
                {
                    if (canvas != self.UICanvas)
                    {
                        childCanvases.Add(canvas);
                    }
                }

                self.ChildCanvases = childCanvases;
            }
            
            self.Raycaster = gameObject.GetComponent<GraphicRaycaster>();
            self.ChildRaycasters.Clear();
            if (self.Raycaster != null)
            {
                GraphicRaycaster[] all = gameObject.GetComponentsInChildren<GraphicRaycaster>(true);
                List<GraphicRaycaster> childRaycasters = new List<GraphicRaycaster>(all.Length);
                foreach (GraphicRaycaster gr in all)
                {
                    if (gr != self.Raycaster)
                    {
                        childRaycasters.Add(gr);
                    }
                }

                self.ChildRaycasters = childRaycasters;
            }
        }
		
        [EntitySystem]
        private static void Destroy(this UI self)
        {

            foreach (UI ui in self.nameChildren.Values)
            {
                ui.Dispose();
            }
		
            UnityEngine.Object.Destroy(self.GameObject);
            self.nameChildren.Clear();
            
            self.FullScreen = false;
            self.Raycaster = null;
            self.UICanvas = null;
            self.IsHide = false;
            self.ChildRaycasters.Clear();
            self.ChildCanvases.Clear();
            self.GameObject = null;

        }

        public static void SetAsFirstSibling(this UI self)
        {
            self.GameObject.transform.SetAsFirstSibling();
        }

        public static bool Visible(this UI self)
        {
            if (self.UICanvas != null)
            {
                return self.UICanvas.gameObject.layer == UIType.WINDOW_SHOW_LAYER;
            }

            return false;
        }

        public static void Visible(this UI self, bool visible)
        {
            if (self.UICanvas == null)
            {
                return;
            }

            int setLayer = visible ? UIType.WINDOW_SHOW_LAYER : UIType.WINDOW_HIDE_LAYER;
            if (self.UICanvas.gameObject.layer == setLayer)
            {
                return;
            }

            self.UICanvas.gameObject.layer = setLayer;
            for (int i = 0; i < self.ChildCanvases.Count; i++)
            {
                Canvas child = self.ChildCanvases[i];
                if (child != null)
                {
                    child.gameObject.layer = setLayer;
                }
            }

            self.Interactable(visible);
        }
        public static bool Interactable(this UI self)
        {
            if (self.Raycaster != null)
            {
                return self.Raycaster.enabled;
            }

            return false;
        }

        public static void Interactable(this UI self, bool interactable)
        {
            if (self.Raycaster == null)
            {
                return;
            }

            self.Raycaster.enabled = interactable;
            for (int i = 0; i < self.ChildRaycasters.Count; i++)
            {
                GraphicRaycaster child = self.ChildRaycasters[i];
                if (child != null)
                {
                    child.enabled = interactable;
                }
            }
        }
        public static void Add(this UI self, UI ui)
        {
            self.nameChildren.Add(ui.Name, ui);
        }

        public static void Remove(this UI self, string name)
        {
            EntityRef<UI> uiRef;
            if (!self.nameChildren.Remove(name, out uiRef))
            {
                return;
            }

            UI ui = uiRef;
            ui?.Dispose();
        }

        public static UI Get(this UI self, string name)
        {
            EntityRef<UI> uiRef;
            if (self.nameChildren.TryGetValue(name, out uiRef))
            {
                return uiRef;
            }
            GameObject childGameObject = self.GameObject.transform.Find(name)?.gameObject;
            if (childGameObject == null)
            {
                return null;
            }
            UI child = self.AddChild<UI, string, GameObject, int>(name, childGameObject, 0);
            self.Add(child);
            return child;
        }
    }
    
    [ChildOf()]
    public sealed class UI: Entity, IAwake<string, GameObject, int>, IDestroy
    {
        public GameObject GameObject { get; set; }
        public Canvas UICanvas { get; set; }

        public List<Canvas> ChildCanvases = new List<Canvas>();

        public GraphicRaycaster Raycaster { get; set; }

        public List<GraphicRaycaster> ChildRaycasters = new List<GraphicRaycaster>();

        public string Name { get; set; }

        public bool FullScreen { get; set; }

        public bool IsHide { get; set; }
        

        public Dictionary<string, EntityRef<UI>> nameChildren = new();
    }
}