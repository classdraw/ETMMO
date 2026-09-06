#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.Rendering;
#endif
using System;
using UnityEngine;


namespace CenturyGame.FakeLight
{
    [ExecuteInEditMode]
    public class FakeLight : MonoBehaviour
    {
        public enum FakeLightType
        {
            Point,
            Spot
        }

        public FakeLightType lightType = FakeLightType.Point;
        public Color color = Color.white;
        [Min(0f)]
        public float intensity = 1.0f;
        [Min(0f)]
        public float range = 10.0f;
        [Min(0)]
        public int priority = 100;

        //Spot
        [Range(0f, 179f)]
        public float outerSpotAngle = 30.0f;
        [Range(0f, 179f)]
        public float innerSpotAngle = 21.8f;
        
        public Bounds bounds
        {
            get
            {
                Vector3 center = transform.position;
                if (lightType == FakeLightType.Point)
                {
                    return new Bounds(center, new Vector3(range * 2, range * 2, range * 2));
                }
                else // Spot
                {
                    return CalculateSpotLightBounds();
                }
            }
        }
        
        private Bounds CalculateSpotLightBounds()
        {
            Vector3 pos = transform.position;
            Vector3 forward = transform.forward;
            Vector3 up = transform.up;
            Vector3 right = transform.right;
            
            float halfAngle = outerSpotAngle * 0.5f * Mathf.Deg2Rad;
            float radius = Mathf.Sin(halfAngle) * range;
            
            Vector3 circleCenter = pos + forward * range * Mathf.Cos(halfAngle);
            float length = radius / Mathf.Sin(45 * Mathf.Deg2Rad);
            Vector3 rightOffset = right * length;
            Vector3 upOffset = up * length;
            Vector3 forwardOffset = forward * range / Mathf.Cos(halfAngle / 2);
                
            Vector3 pos0 = pos;
            Bounds bounds = new Bounds(pos0, Vector3.zero);
            //x正方向
            Vector3 pos1 = circleCenter + rightOffset;
            bounds.Encapsulate(pos1);
            //x负方向
            Vector3 pos2 = circleCenter - rightOffset;
            bounds.Encapsulate(pos2);
            //y正方向
            Vector3 pos3 = circleCenter + upOffset;
            bounds.Encapsulate(pos3);
            //y负方向
            Vector3 pos4 = circleCenter - upOffset;
            bounds.Encapsulate(pos4);
            Vector3 pos5 = pos + forwardOffset;
            bounds.Encapsulate(pos5);
            
            return bounds;
        }

        public void OnDrawGizmos()
        {
            // Gizmos.color = Color.yellow;
            // Bounds b = bounds;
            // Gizmos.DrawWireCube(b.center, b.size);
        }

        private void OnEnable()
        {
            FakeLightListManager.Instance.Register(this);
        }

        private void OnDisable()
        {
            FakeLightListManager.Instance.Unregister(this);
        }
        
    }
}