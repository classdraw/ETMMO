using UnityEngine;

namespace ET.Client
{
    
    
    [ComponentOf(typeof(Unit))]
    public class GameObjectComponent: Entity, IAwake, IDestroy
    {
        public GameObject gameObject;

        public GameObject GameObject
        {
            get
            {
                return this.gameObject;
            }
            set
            {
                if (value==null)
                {
                    this.gameObject = null;
                    this.Transform = null;
                }
                else
                {
                    this.gameObject = value;
                    this.Transform = value.transform;
                }
            }
        }

        public Transform Transform { get;set; }
        
    }
}