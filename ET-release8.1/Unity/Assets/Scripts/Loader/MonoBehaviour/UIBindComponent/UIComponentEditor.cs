#if UNITY_EDITOR

using UnityEngine;

namespace GameLogic
{
    public partial class UIBindComponent
    {
        [SerializeField, HideInInspector] private string genCodePath;
        [SerializeField, HideInInspector] private string className;
        [SerializeField, HideInInspector] private string impCodePath;
        [SerializeField, HideInInspector] private bool isGenImpClass;
        [SerializeField, HideInInspector] private string uiType;

        public void AddReference(Object reference)
        {
            if (m_components != null && reference != null && !m_components.Contains(reference))
            {
                m_components.Add(reference);
            }
        }

        public void AddComponent(Component component)
        {
            AddReference(component);
        }

        public void AddGameObject(GameObject gameObject)
        {
            AddReference(gameObject);
        }

        public void Clear()
        {
            m_components?.Clear();
        }
    }
}

#endif