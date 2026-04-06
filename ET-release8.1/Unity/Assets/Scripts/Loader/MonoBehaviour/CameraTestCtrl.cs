using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 挂在摄像机上：发现名为 Unit 的物体则缓存，每帧仅将相机 X、Z 对齐到该物体（Y 不变）。
    /// </summary>
    public class CameraTestCtrl : MonoBehaviour
    {
        private static CameraTestCtrl _instance;

        public static CameraTestCtrl Instance
        {
            get
            {
                return _instance;
            }
        }

        public void SetUnit(Transform unit)
        {
            this._unit = unit;
        }
        [SerializeField]
        private Transform _unit;

        private void Awake()
        {
            _instance = this;
        }

        private void LateUpdate()
        {
            if (_unit == null)
            {
                return;
            }

            Vector3 pos = transform.position;
            pos.x = _unit.position.x;
            pos.z = _unit.position.z;
            transform.position = pos;
        }
    }
}
