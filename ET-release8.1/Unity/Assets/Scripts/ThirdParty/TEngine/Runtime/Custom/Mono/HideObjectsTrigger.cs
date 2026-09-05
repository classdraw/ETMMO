using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    /// <summary>
    /// 触发器区域：指定 Layer 的物体进入时隐藏列表内对象，离开后重新显示。
    /// NeedMainPlayer 勾选时，仅名称以 * 结尾的物体（主玩家）可触发。
    /// 挂到 GameObject 上会自动添加 BoxCollider（Trigger）与 Kinematic Rigidbody。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HideObjectsTrigger : MonoBehaviour
    {
        [SerializeField]
        private List<GameObject> hideObjects = new List<GameObject>();

        [SerializeField]
        private LayerMask triggerLayerMask = ~0;

        [SerializeField]
        private bool needMainPlayer = true;

        [SerializeField]
        private bool colliderEnabled = true;

        private BoxCollider boxCollider;
        private int triggerCount;

        public IReadOnlyList<GameObject> HideObjects => hideObjects;

        public LayerMask TriggerLayerMask
        {
            get => triggerLayerMask;
            set => triggerLayerMask = value;
        }

        public bool NeedMainPlayer
        {
            get => needMainPlayer;
            set => needMainPlayer = value;
        }

        public bool ColliderEnabled
        {
            get => colliderEnabled;
            set
            {
                colliderEnabled = value;
                ApplyColliderEnabled();
            }
        }

        private void Reset()
        {
            EnsureTriggerComponents();
        }

        private void Awake()
        {
            EnsureTriggerComponents();
            ApplyColliderEnabled();
        }

        private void OnValidate()
        {
            EnsureTriggerComponents();
            ApplyColliderEnabled();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!colliderEnabled || !IsValidTrigger(other))
            {
                return;
            }

            triggerCount++;
            if (triggerCount == 1)
            {
                SetHideObjectsActive(false);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!IsValidTrigger(other))
            {
                return;
            }

            triggerCount = Mathf.Max(0, triggerCount - 1);
            if (triggerCount == 0)
            {
                SetHideObjectsActive(true);
            }
        }

        private void EnsureTriggerComponents()
        {
            if (boxCollider == null)
            {
                boxCollider = GetComponent<BoxCollider>();
            }

            if (boxCollider == null)
            {
                boxCollider = gameObject.AddComponent<BoxCollider>();
            }

            boxCollider.isTrigger = true;

            Rigidbody rigidbody = GetComponent<Rigidbody>();
            if (rigidbody == null)
            {
                rigidbody = gameObject.AddComponent<Rigidbody>();
            }

            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;
        }

        private void ApplyColliderEnabled()
        {
            if (boxCollider != null)
            {
                boxCollider.enabled = colliderEnabled;
            }
        }

        private bool IsValidTrigger(Collider other)
        {
            if (other == null)
            {
                return false;
            }

            int layerMask = 1 << other.gameObject.layer;
            if ((triggerLayerMask.value & layerMask) == 0)
            {
                return false;
            }

            if (needMainPlayer && !other.gameObject.name.EndsWith("*"))
            {
                return false;
            }

            return true;
        }

        private void SetHideObjectsActive(bool active)
        {
            for (int i = 0; i < hideObjects.Count; i++)
            {
                GameObject target = hideObjects[i];
                if (target != null)
                {
                    target.SetActive(active);
                }
            }
        }
    }
}
