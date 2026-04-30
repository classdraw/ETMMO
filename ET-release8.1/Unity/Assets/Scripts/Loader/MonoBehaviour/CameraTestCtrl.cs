using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 挂在摄像机上：对 <see cref="SetUnit"/> 传入的 target，每帧将相机位置平滑插值到 <c>target.position + offset</c>。
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

        /// <summary>相对 target 世界坐标的目标点偏移，相机插值到 <c>target.position + offset</c>。</summary>
        [SerializeField]
        private Vector3 offset;

        [SerializeField]
        private float smoothTime = 0.15f;

        private Vector3 smoothVelocity;

        private void Awake()
        {
            _instance = this;
        }

        private void LateUpdate()
        {
            if (this._unit == null)
            {
                return;
            }

            Vector3 target = this._unit.position + this.offset;
            this.transform.position = Vector3.SmoothDamp(this.transform.position, target, ref this.smoothVelocity, this.smoothTime);
        }
    }
}
