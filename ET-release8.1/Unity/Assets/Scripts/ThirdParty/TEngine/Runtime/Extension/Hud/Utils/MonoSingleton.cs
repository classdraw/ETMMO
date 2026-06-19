using System;
using TEngine;
using UnityEngine;

namespace XEngine.Utilities{
public class  AutoCreateInstanceAttribute:Attribute
    {
        public bool _autoCreate=true;
        public AutoCreateInstanceAttribute(bool create){
            _autoCreate=create;
        }
        
    }
    /// <summary>
    /// 带mono的单利
    /// </summary>
    [AutoCreateInstance(true)]
    public class  MonoSingleton<T>:MonoBehaviour where T :Component
    {
        private static T _instance;
        public static bool HasInstance(){
			return _instance!=null;
		}

        public static T GetInstance(){
            return Instance;
        }
        public static T Instance{
            get{
                if(MonoSingleton<T>._instance==null){
                    Type t=typeof(T);
                    object[] customAttrs=t.GetCustomAttributes(typeof(AutoCreateInstanceAttribute),true);
                    if(customAttrs!=null&&customAttrs.Length>0&&!((AutoCreateInstanceAttribute)customAttrs[0])._autoCreate){
                        return null;
                    }
                    CreateInstance();
                }
                return MonoSingleton<T>._instance;

            }
        }


        public static T CreateInstance(Transform parent=null){
            // Debug.LogError(">>>>>"+typeof(T));
            if(MonoSingleton<T>._instance==null){ 
                Type t=typeof(T);
                GameObject go=new GameObject(t.Name,t);
                GameObject.DontDestroyOnLoad(go);
                if(parent!=null){
                    go.transform.parent=parent;
                }
                MonoSingleton<T>._instance=go.GetComponent<T>();
            }
            return MonoSingleton<T>._instance;
        }

        public static void DestroyInstance(){
            if(MonoSingleton<T>._instance!=null){
                Log.Debug("DestroyInstance:"+_instance.name);
                GameObject.DestroyImmediate(MonoSingleton<T>._instance.gameObject);
            }
            MonoSingleton<T>._instance=null;
        }

        protected virtual void Awake(){
            if(MonoSingleton<T>._instance!=null&&MonoSingleton<T>._instance.gameObject!=this.gameObject){
                if(Application.isPlaying){
                    //销毁gameobject
                    GameObject.Destroy(this.gameObject);
                }else{
                    GameObject.DestroyImmediate(this.gameObject);
                }
            }else if(MonoSingleton<T>._instance==null){
                MonoSingleton<T>._instance=base.GetComponent<T>();
            }
            this.Init();
        }

        protected virtual void Init(){

        }

        protected virtual void Release(){

        }


        void Destroy(){
            this.OnDestroy();
        }
        protected virtual void OnDestroy(){
            this.Release();
            if( MonoSingleton<T>._instance != null && MonoSingleton<T>._instance.gameObject == this.gameObject ){
                MonoSingleton<T>._instance = null;
            }
        }
    }
}
    