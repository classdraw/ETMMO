using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace TEngine {
    /// <summary>
    /// 循环列表
    /// </summary>
    public class LayoutLoopList : ScrollRect {
        // 模版使用记录
        private class TemplateUseRecord {
            private static ObjectPool<TemplateUseRecord> m_RecordPool = new ObjectPool<TemplateUseRecord>(() => new TemplateUseRecord());
            // 对应的组件和对象池
            public Component component { get; private set; }
            private ObjectPool<Component> m_ComponentPool;
            // 隐藏构造
            private TemplateUseRecord() { }
            public static TemplateUseRecord Create(Component component, ObjectPool<Component> componentPool) {
                TemplateUseRecord record = m_RecordPool.Get();
                record.component = component;
                record.m_ComponentPool = componentPool;
                return record;
            }

            public void Release() {
                if (!component) return;
                m_ComponentPool.Release(component);
                component = null;
                m_ComponentPool = null;
                m_RecordPool.Release(this);
            }
        }

        #region 序列化参数
        [SerializeField, HideInInspector, Header("模板列表")] private Component[] m_Templates;
        [SerializeField, Header("边缘缓冲")] private int m_ExDistance = 20;
        [SerializeField, Header("模拟滑动速度")] private float m_ScrollSpeed = 1000;
        [SerializeField, Header("最大绘制数量")] private int m_MaxDisplayCount = 100;
        [SerializeField, Header("在Viewport中的锚点")] private float m_AlignAnchor = 0f;
        [SerializeField, Header("Item位置")] private float m_AlignItemPivot = 0f;
        [SerializeField, Header("Item偏移")] private float m_AlignItemOffset = 0f;
        [SerializeField, Header("循环")] private bool m_Loop = false;
        [SerializeField, Header("自动对齐")] private bool m_AutoAlign;
        [SerializeField, Header("滑动过程中执行对齐回调")] private bool m_CallInScrolling;
        #endregion

        public int exDistance { get => m_ExDistance; set => m_ExDistance = value; }
        public int maxDisplayCount { get => m_MaxDisplayCount; set => m_MaxDisplayCount = value; }
        public float scrollSpeed { get => m_ScrollSpeed; set => m_ScrollSpeed = value; }
        public float alignAnchor { get => m_AlignAnchor; set => m_AlignAnchor = value; }
        public float alignItemPivot { get => m_AlignItemPivot; set => m_AlignItemPivot = value; }
        public float alignItemOffset { get => m_AlignItemOffset; set => m_AlignItemOffset = value; }
        public bool loop { get => m_Loop; set => m_Loop = value; }
        public bool autoAlign { get => m_AutoAlign; set => m_AutoAlign = value; }
        public bool callInScrolling { get => m_CallInScrolling; set => m_CallInScrolling = value; }

        #region 回调
        // 回调
        private Func<int, int> m_OnItemPreCreate;
        private UnityEvent<Component, int> m_OnItemRefresh;
        private UnityEvent<Component, int> m_OnItemRelease;
        private UnityEvent<Component, int> m_OnItemAlign;       // Item对齐回调
        private UnityEvent<Component, int> m_OnHeadListener;
        private UnityEvent<Component, int> m_OnTailListener;

        public event Func<int, int> OnItemPreCreate { add => m_OnItemPreCreate += value; remove => m_OnItemPreCreate = null; }
        public UnityEvent<Component, int> OnItemRefresh => m_OnItemRefresh ??= new UnityEvent<Component, int>();
        public UnityEvent<Component, int> OnItemRelease => m_OnItemRelease ??= new UnityEvent<Component, int>();
        public UnityEvent<Component, int> OnItemAlign => m_OnItemAlign ??= new UnityEvent<Component, int>();
        public UnityEvent<Component, int> OnHeadListener => m_OnHeadListener ??= new UnityEvent<Component, int>();
        public UnityEvent<Component, int> OnTailListener => m_OnTailListener ??= new UnityEvent<Component, int>();

        #endregion

        #region 绘制Item数据记录
        // 模版使用记录
        private Dictionary<Transform, TemplateUseRecord> m_TemplateUseRecords = new Dictionary<Transform, TemplateUseRecord>();
        // 总数量
        public int dataCount { get; private set; }
        // 当前显示起始下标
        public int dataStartIndex { get; private set; }
        // 当前显示数量
        public int displayCount { get; private set; }
        // 下标对齐
        private bool m_IndexAligned = false;
        // 当前对齐的数据下标：仅Align.force = true时生效
        public int alignDataIndex { get; private set; } = -1;
        public bool scrollInHead { get; private set; } = false;
        public bool scrollInTail { get; private set; } = false;
        #endregion

        #region 滑动区域信息        
        // 初始化标记
        private bool m_Init = false;
        // 轴：0水平 1垂直
        private int m_Axis = 0;
        // Viewport尺寸
        private float m_ViewSize = 0f;
        // 数据是否翻转
        private bool m_DataReverse;
        // 缩放是否翻转
        private bool m_ScaleReverse;
        // 排版容器是否应用子项缩放：
        private bool m_UseChildScale;
        // content刷新位置标记，位置在范围内时不需要刷新
        private float[] m_ContentPosRange = { 0, 0, 0 };
        // 刷新边缘
        private float[] edge = { 0f, 0f };
        // Content计算边缘的端点
        private float[] endpoint = { 0f, 0f };
        #endregion

        #region 对象池及其他
        // 排版组件
        private HorizontalOrVerticalLayoutGroup m_LayoutGroup;
        // 对象池
        private ObjectPool<Component>[] m_Pools;
        private Transform[] m_PoolParents;
        private Transform m_PoolRoot;
        // 偏移记录
        private Vector2 m_Offset = Vector2.zero;
        #endregion

        #region 模拟滑动
        // 模拟滑动状态枚举值
        private enum SimulationScrollState { None, Scrolling, Completed }
        // 模拟滑动状态
        private SimulationScrollState m_ScrollState = SimulationScrollState.None;
        // 目标数据下标
        private int m_ScrollDataIndex;
        // viewport锚点
        private float m_ScrollViewAnchor;
        // item中心点
        private float m_ScrollItemPivot;
        // item偏移
        private float m_ScrollItemOffset;
        // 目标方向：1、初始距离大于0 2、初始距离小于0
        private int m_ScrollDirection;
        // 当前剩余距离
        private float m_ScrollDistance;
        // 等待Item显示：用于ScrollToRange方法，Item显示根据scale判定是否需要重新计算
        private bool m_WaitItemShow = false;
        // 当前是否在拖动中
        private bool m_IsDragging = false;
        #endregion

        protected override void Awake() {
            base.Awake();
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            AwakeInit();
            RefreshArea();
        }

        public override void OnBeginDrag(PointerEventData eventData) {
            base.OnBeginDrag(eventData);
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (!IsActive())
                return;
            m_IsDragging = true;
            ResetSiumlationScroll();
        }

        public override void OnEndDrag(PointerEventData eventData) {
            base.OnEndDrag(eventData);
            if (eventData.button != PointerEventData.InputButton.Left)
                return;
            m_IsDragging = false;
            ResetSiumlationScroll();
        }

        protected override void OnDisable() {
            base.OnDisable();
            m_IsDragging = false;
            ResetSiumlationScroll();
        }

        /// <summary>
        /// 刷新区域：运行时排版容器参数如果发生变化，手动调用刷新
        /// </summary>
        public void RefreshArea() {
            // 刷新Viewport尺寸
            m_ViewSize = 0f;
            float curViewSize = GetViewportSize();
            if (Mathf.Abs(curViewSize) < 0.00001f) return;
            m_ViewSize = curViewSize;
            // 获取排版信息
            m_UseChildScale = m_Axis == 0 ? m_LayoutGroup.childScaleWidth : m_LayoutGroup.childScaleHeight;
            m_DataReverse = (m_LayoutGroup && m_LayoutGroup.reverseArrangement != m_ScaleReverse) ? m_Axis == 0 : m_Axis != 0;
            float contentScale = content.localScale[m_Axis];
            float scaleAbs = Mathf.Abs(contentScale);
            // 容器边界
            int paddingHead = m_Axis == 0 ? m_LayoutGroup.padding.left : m_LayoutGroup.padding.bottom;
            int paddingTail = m_Axis == 0 ? m_LayoutGroup.padding.right : m_LayoutGroup.padding.top;
            // 刷新边界值及端点
            float exHead = paddingHead * scaleAbs + m_ExDistance;
            float exTail = paddingTail * scaleAbs + m_ExDistance;
            (endpoint[0], endpoint[1]) = (0f, 1f);
            m_ScaleReverse = contentScale < 0f;
            // Content的scale小于0，需要翻转对应边界
            if (m_ScaleReverse) {
                (exHead, exTail) = (exTail, exHead);
                (endpoint[0], endpoint[1]) = (endpoint[1], endpoint[0]);
            }
            // 相对viewport归一的边界值
            edge[0] = 0 - exHead / m_ViewSize;
            edge[1] = 1 + exTail / m_ViewSize;
            m_ContentPosRange[2] = 0;
        }

        // Content区域是否足够覆盖viewport区域
        private bool CoverViewport() {
            if (!content) return false;
            float contentSize = Mathf.Abs(content.rect.size[m_Axis] * content.localScale[m_Axis]);
            return contentSize >= m_ViewSize;
        }

        /// <summary>
        /// 检查Item是否需要回收
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private bool CheckItemRelease(RectTransform item) {
            bool data0 = item.GetSiblingIndex() == 0;
            bool fromView0 = data0 != m_DataReverse;
            float pivot = endpoint[fromView0 ? 0 : 1];
            // 获取item在content占用的偏移值
            float itemOffset = item.rect.size[m_Axis];
            if (m_UseChildScale) itemOffset *= item.localScale[m_Axis];
            itemOffset += m_LayoutGroup.spacing;
            if (!fromView0) itemOffset = -itemOffset;

            float offsetPos = content.GetNormalizedPos(m_Axis, pivot, itemOffset);
            return fromView0 ? offsetPos < edge[0] : offsetPos > edge[1];
        }

        /// <summary>
        /// 检查当前是否需要更新Item
        /// </summary>
        /// <returns></returns>
        private bool NeedUpdate() {
            if (m_ContentPosRange[2] < 1) return true;
            float contentPos = content.anchoredPosition[m_Axis];
            return Mathf.Clamp(contentPos, m_ContentPosRange[0], m_ContentPosRange[1]) != contentPos;
        }

        // 更新完毕
        private void UpdateComplate() {
            float contentPos = content.anchoredPosition[m_Axis];
            float addOffset = edge[0] - content.GetNormalizedPos(m_Axis, endpoint[0]);
            float subOffset = content.GetNormalizedPos(m_Axis, endpoint[1]) - edge[1];

            m_ContentPosRange[0] = subOffset < 0 ? float.MinValue : contentPos - subOffset * m_ViewSize;
            m_ContentPosRange[1] = addOffset < 0 ? float.MaxValue : contentPos + addOffset * m_ViewSize;

            m_ContentPosRange[2] = 2;
        }

        // 获取viewport尺寸
        private float GetViewportSize() {
            var parentLayoutGroup = viewport.GetComponentInParent<HorizontalOrVerticalLayoutGroup>();
            if (parentLayoutGroup) LayoutRebuilder.ForceRebuildLayoutImmediate(parentLayoutGroup.transform as RectTransform);
            return viewport.rect.size[m_Axis];
        }

        // 初始化
        public void AwakeInit() {
            if (m_Init) return;
            // 必要组件初始化
            m_LayoutGroup = content.GetComponent<HorizontalOrVerticalLayoutGroup>();
            Debug.Assert(m_LayoutGroup, $"可回收滑动组件需要排版组件：(Horizontal/Vertical)LayoutGroup。");
            Debug.Assert(content.GetComponent<ContentSizeFitter>(), $"可回收滑动组件需要自适应组件：ContentSizeFitter。");
            onValueChanged.AddListener(OnScrollChanged);
            m_Init = true;
            // 关注的滑动方向轴
            m_Axis = (m_LayoutGroup is VerticalLayoutGroup) ? 1 : 0;
            m_Offset = Vector2.zero;
            // 对象池初始化
            InitPoolRoot();
            InitTemplatePools();
        }

        // 初始化对象池根节点
        private void InitPoolRoot() {
            if (m_PoolRoot) return;
            m_PoolRoot = viewport.Find("ItemPoolRoot");
            if (m_PoolRoot) return;
            m_PoolRoot = new GameObject("ItemPoolRoot").transform;
            m_PoolRoot.SetParent(viewport, false);
        }

        // 获取对象池父对象
        private Transform GetPoolParent(int index) {
            if (m_PoolParents[index] == null) {
                // 创建缓存池承载对象
                m_PoolParents[index] = new GameObject("ItemPool_" + index).transform;
                m_PoolParents[index].SetParent(m_PoolRoot, false);
                m_PoolParents[index].localScale = Vector3.zero;
            }
            return m_PoolParents[index];
        }

        // 初始化Item模板缓存池
        private void InitTemplatePools() {
            if (m_Pools != null) ResetItems(0);
            m_Pools = new ObjectPool<Component>[m_Templates.Length];
            m_PoolParents = new Transform[m_Templates.Length];
            for (int i = 0; i < m_Templates.Length; i++) {
                int templateIndex = i;
                // 创建Item缓存池
                Component itemTemplate = m_Templates[i];
                m_Pools[i] = new ObjectPool<Component>(
                    createFunc: () => Instantiate(itemTemplate, content, false),
                    actionOnGet: (item) => item.transform.SetParent(content, false),
                    actionOnRelease: (item) => item.transform.SetParent(GetPoolParent(templateIndex), false),
                    actionOnDestroy: (item) => Destroy(item.gameObject));
            }
        }

        // 滑动回调
        private void OnScrollChanged(Vector2 _) {
            RefreshItems();
        }

        // 重置Item
        private void ResetItems(int count) {
            int childCount = content.childCount;
            for (int i = childCount - 1; i >= 0; i--) {
                if (m_TemplateUseRecords.TryGetValue(content.GetChild(i), out var record)) {
                    m_OnItemRelease?.Invoke(record.component, (i + dataStartIndex) % dataCount);
                    record.Release();
                }
            }
            m_TemplateUseRecords.Clear();
            if (childCount != 0) {
                LayoutRebuilder.ForceRebuildLayoutImmediate(content);
                m_ContentStartPosition = content.anchoredPosition = LayoutLoopListUtils.GetContentValidPos(viewport, content, m_Axis);
            }
            displayCount = 0;
            dataCount = count;
            dataStartIndex = 0;
            alignDataIndex = -1;
            scrollInHead = false;
            scrollInTail = false;
            ResetSiumlationScroll();
        }

        // 重置模拟滑动参数
        void ResetSiumlationScroll() {
            m_ScrollState = SimulationScrollState.None;
            m_WaitItemShow = false;
            m_IndexAligned = false;
        }

        // 容器本身发生变化时，更新viewport
        protected override void OnRectTransformDimensionsChange() {
            base.OnRectTransformDimensionsChange();
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            UpdateViewportSize();
        }

        // 更新Viewport尺寸：会根据Align参数固定content与viewport的相对位置
        public void UpdateViewportSize() {
            AwakeInit();
            StopMovement();
            float viewSize = m_ViewSize;
            RefreshArea();
            // 尺寸未发生变化
            if (Mathf.Approximately(viewSize, m_ViewSize) || Mathf.Approximately(viewSize, 0)) return;
            /**
             * 1、content.anchorMin[m_Axis] == content.anchorMax[m_Axis]
             *      此时viewport发生变化，content的位置只与content.anchorMin[m_Axis]/content.anchorMax[m_Axis]有关
             * 2、content.anchorMin[m_Axis] != content.anchorMax[m_Axis]
             *      此时viewport发生变化会导致content进行拉伸，然后被ContentSizeFitter组件修改回原大小，过程中会有pivot的影响，但可以引入pivot转换为等效的anchor值
             *      content.anchorMin[m_Axis] + (content.anchorMax[m_Axis] - content.anchorMin[m_Axis]) * content.pivot[m_Axis]
             */
            float anchor = content.anchorMin[m_Axis] + (content.anchorMax[m_Axis] - content.anchorMin[m_Axis]) * content.pivot[m_Axis];
            // 位置偏移
            m_Offset[m_Axis] = (anchor - m_AlignAnchor) * (viewSize - m_ViewSize);
            // 有尺寸拉伸时，需要先进行布局修正
            if (!Mathf.Approximately(content.anchorMin[m_Axis], content.anchorMax[m_Axis])) LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            content.anchoredPosition += m_Offset;
            // 修正回弹效果
            if (movementType != MovementType.Unrestricted) content.anchoredPosition = LayoutLoopListUtils.GetContentValidPos(viewport, content, m_Axis);

            RefreshItems(true);
        }

        // 刷新Items列表
        private void RefreshItems(bool fixContentPos = false) {
            // 没有
            if (dataCount <= 0) return;
            if (m_Loop && inertia && Mathf.Abs(decelerationRate) >= 1) {
                decelerationRate = 0.1f;
#if UNITY_EDITOR
                throw new ArgumentException("循环滑动惯性不能超过1，否则会无限加速滑动且不会结束！！！");
#endif
            }
            float contentPos = content.anchoredPosition[m_Axis];
            if (fixContentPos || NeedUpdate()) {
                // 添加Items
                AddItems(false);
                // 滑动位置修复，避免回弹
                if (fixContentPos) {
                    AddItems(true);
                    Vector2 fixPos = LayoutLoopListUtils.GetContentValidPos(viewport, content, m_Axis);
                    if (fixPos != content.anchoredPosition) {
                        m_ContentStartPosition = content.anchoredPosition = LayoutLoopListUtils.GetContentValidPos(viewport, content, m_Axis);
                        RefreshItems();
                        return;
                    }
                }
                // 回收
                if (displayCount > 0) ReleaseItems();
                UpdateComplate();
            }

            // 回调执行
            scrollInHead = UpdateOnHeadOrTailListener(true, scrollInHead, m_OnHeadListener);
            scrollInTail = UpdateOnHeadOrTailListener(false, scrollInTail, m_OnTailListener);
            m_IndexAligned = false;
        }

        // 更新首尾回调
        private bool UpdateOnHeadOrTailListener(bool isHead, bool inHeadOrTail, UnityEvent<Component, int> m_OnHeadListener) {
            if (m_OnHeadListener == null) return false;
            int dataIndex = isHead ? 0 : dataCount - 1;
            RectTransform item = GetItemRT(dataIndex);
            if (!item) return false;
            float itemPos = item.GetNormalizedPos(m_Axis);
            float itemPosInView = content.GetNormalizedPos(m_Axis, itemPos);
            float halfSizeInView = Mathf.Abs(item.rect.size[m_Axis] * item.localScale[m_Axis] * content.localScale[m_Axis]) * 0.5f / m_ViewSize;
            bool cur = false;
            if (isHead != m_DataReverse) {
                cur = itemPosInView - halfSizeInView > -0.00001f;
            } else {
                cur = 1 - itemPosInView - halfSizeInView > -0.00001f;
            }
            if (cur != inHeadOrTail && cur) {
                m_OnHeadListener?.Invoke(GetItemComponent(item, dataIndex), dataIndex);
            }
            return cur;
        }

        /// <summary>
        /// 添加Items
        /// </summary>
        /// <param name="needCoverView">需要增加到覆盖Viewport</param>
        private void AddItems(bool needCoverView) {
            float contentPos = content.GetNormalizedPos(m_Axis, endpoint[1]);
            // Item补充
            while (displayCount < m_MaxDisplayCount
                && ValidAdd(!m_DataReverse)
                && ((needCoverView && !CoverViewport()) || content.GetNormalizedPos(m_Axis, endpoint[0]) > edge[0])) {

                CreateItem(!m_DataReverse, endpoint[1], contentPos);
            }
            contentPos = content.GetNormalizedPos(m_Axis, endpoint[0]);
            while (displayCount < m_MaxDisplayCount
                && ValidAdd(m_DataReverse)
                && ((needCoverView && !CoverViewport()) || content.GetNormalizedPos(m_Axis, endpoint[1]) < edge[1])) {

                CreateItem(m_DataReverse, endpoint[0], contentPos);
            }
        }

        // 验证当前能否新增数据
        private bool ValidAdd(bool dataToHead) {
            if (m_Loop && dataCount > 0) return true;
            if (dataToHead) {
                return dataStartIndex > 0;
            }
            return dataStartIndex + displayCount < dataCount;
        }

        // 增加Item
        private RectTransform CreateItem(bool dataTo0, float fixedAnchor, float contentPos) {
            // 数据下标获取
            int dataIndex = dataTo0 ? dataStartIndex - 1 : (dataStartIndex + displayCount) % dataCount;
            if (dataIndex < 0) {
                dataIndex = displayCount == 0 ? 0 : dataCount - 1;
            }
            // 模板下标获取
            int templateIndex = m_OnItemPreCreate == null ? 0 : m_OnItemPreCreate.Invoke(dataIndex);
            // Item创建
            Component item = m_Pools[templateIndex].Get();
            item.gameObject.name = $"item_{dataIndex}";
            m_TemplateUseRecords.Add(item.transform, TemplateUseRecord.Create(item, m_Pools[templateIndex]));
            m_OnItemRefresh?.Invoke(item, dataIndex);
            displayCount++;
            if (dataTo0) {
                item.transform.SetAsFirstSibling();
                dataStartIndex = dataIndex;
            }
            // 位置还原
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            float relativePos = content.GetNormalizedPos(m_Axis, fixedAnchor);
            m_Offset[m_Axis] = (relativePos - contentPos) * m_ViewSize;
            content.anchoredPosition -= m_Offset;
            m_ContentStartPosition -= m_Offset;

            return item.transform as RectTransform;
        }

        // 回收Items
        private void ReleaseItems() {
            while (displayCount > 1 && TryReleaseItem(true, !m_DataReverse)) ;
            while (displayCount > 1 && TryReleaseItem(false, m_DataReverse)) ;
        }

        // 尝试执行回收Item
        private bool TryReleaseItem(bool viewFrom0, bool dataFrom0) {
            // 回收项获取
            int index = dataFrom0 ? 0 : content.childCount - 1;

            RectTransform itemRT = content.GetChild(index).transform as RectTransform;
            if (!CheckItemRelease(itemRT)) return false;
            // 位置检查
            float itemPivot = viewFrom0 ? endpoint[1] : endpoint[0];

            float contentPivot1 = content.GetNormalizedPos(m_Axis, itemPivot);
            float contentPos = content.anchoredPosition[m_Axis];

            // 执行回收
            if (m_TemplateUseRecords.TryGetValue(itemRT, out var record)) {
                m_OnItemRelease?.Invoke(record.component, (dataStartIndex + itemRT.GetSiblingIndex()) % dataCount);
                record.Release();
                m_TemplateUseRecords.Remove(itemRT);
            }
            displayCount--;
            if (dataFrom0) {
                dataStartIndex++;
                if (dataStartIndex >= dataCount) dataStartIndex %= dataCount;
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            float contentPivot2 = content.GetNormalizedPos(m_Axis, itemPivot);
            m_Offset[m_Axis] = (contentPivot2 - contentPivot1) * m_ViewSize;
            //m_Offset[m_Axis] = (content.anchoredPosition[m_Axis] - contentPos) * (contentPivot2 - contentPivot1);
            content.anchoredPosition -= m_Offset;
            m_ContentStartPosition -= m_Offset;

            return true;
        }

        // 更新Item对齐回调
        private void UpdateOnItemAlign() {
            if (m_OnItemAlign == null || m_IndexAligned) return;
            if (!m_CallInScrolling) {
                // 正在拖动，速度不为0，在滑动中
                if (m_IsDragging || velocity[m_Axis] != 0 || m_ScrollState == SimulationScrollState.Scrolling) return;
            }
            m_IndexAligned = true;
            RectTransform item = GetNearItemRT(m_AlignAnchor, m_AlignItemPivot, m_AlignItemOffset, out int viewIndex, out _);
            if (viewIndex < 0) return;
            int dataIndex = (viewIndex + dataStartIndex) % dataCount;
            if (dataIndex == alignDataIndex) return;
            alignDataIndex = dataIndex;
            m_OnItemAlign.Invoke(GetItemComponent(item, dataIndex), alignDataIndex);
        }

        // 获取Item对应组件
        private Component GetItemComponent(RectTransform itemRT, int dataIndex) {
            if (itemRT && m_TemplateUseRecords.TryGetValue(itemRT, out TemplateUseRecord record)) {
                return record.component;
            }
            return null;
        }

        /// <summary>
        /// 刷新数据数量
        /// </summary>
        /// <param name="dataCount">数据数量</param>
        /// <param name="dataIndex">对齐的数据下标</param>
        /// <param name="applyAlign">应用对齐参数</param>
        public void RefreshDataCount(int dataCount, int dataIndex = -1, bool applyAlign = false) {
            StopAllCoroutines();
            StartCoroutine(DelayRefreshItemCount(dataCount, dataIndex, applyAlign));
        }

        /// <summary>
        /// 保持位置的更新
        /// </summary>
        /// <param name="dataCount">数据数量</param>
        public void UpdateHoldon(int dataCount) {
            if (displayCount < 1 || dataCount < this.dataCount) {
                RefreshDataCount(dataCount);
                return;
            }
            _ = GetNearItemRT(m_AlignAnchor, m_AlignItemPivot, m_AlignItemOffset, out int viewIndex, out float dis);
            int targetIndex = (viewIndex + dataStartIndex) % this.dataCount;
            ResetItems(dataCount);
            dataStartIndex = targetIndex;
            RectTransform item = CreateItem(false, 0, 1);
            AlignItem(item, m_AlignAnchor, m_AlignItemPivot, m_AlignItemOffset - dis);
            RefreshItems(true);
        }

        /// <summary>
        /// 获取指定数据下标的RectTransform
        /// </summary>
        /// <param name="dataIndex"></param>
        /// <param name="from0"></param>
        public RectTransform GetItemRT(int dataIndex, bool from0 = true) {
            int viewIndex = GetItemViewIndex(dataIndex, from0);
            if (viewIndex >= 0) return content.GetChild(viewIndex) as RectTransform;
            return null;
        }

        /// <summary>
        /// 获取指定Item组件
        /// </summary>
        /// <param name="dataIndex">数据下标</param>
        /// <param name="from0">是否从前往后</param>
        public Component GetItem(int dataIndex, bool from0 = true) {
            return GetItemComponent(GetItemRT(dataIndex, from0), dataIndex);
        }

        /// <summary>
        /// 获取Item列表
        /// </summary>
        /// <param name="dataIndex"></param>
        /// <param name="from0"></param>
        public List<Component> GetItems(int dataIndex, bool from0 = true) {
            List<Component> items = new List<Component>();
            List<RectTransform> itemRTList = ListPool<RectTransform>.Get();
            GetItemRTList(dataIndex, itemRTList, from0);
            for (int i = 0; i < itemRTList.Count; i++) {
                items.Add(GetItemComponent(GetItemRT(i), dataIndex));
            }
            ListPool<RectTransform>.Release(itemRTList);
            return items;
        }

        // 延迟刷新Item数量
        private IEnumerator DelayRefreshItemCount(int count, int dataIndex, bool applyAlign) {
            // 如果ViewSize为0，可能当前组件在排版组件中，延迟一帧重新获取尺寸再刷新
            if (Mathf.Approximately(m_ViewSize, 0)) {
                yield return 0;
                m_ViewSize = GetViewportSize();
                RefreshArea();
            }
            ResetItems(count);
            if (count < 1) yield break;
            dataStartIndex = Mathf.Clamp(dataIndex, 0, dataCount - 1);
            StopMovement();
            RectTransform item = CreateItem(false, 0, 1);
            if (m_AutoAlign || applyAlign) {
                AlignItem(item, m_AlignAnchor, m_AlignItemPivot, m_AlignItemOffset);
            } else {
                float viewAnchor = m_DataReverse ? 1 : 0;
                float itemPivot = m_ScaleReverse ? (1 - viewAnchor) : viewAnchor;
                float itemOffset = dataIndex < 0 && !m_Loop ? (m_DataReverse ? m_LayoutGroup.GetPaddingTail(m_Axis) : -m_LayoutGroup.GetPaddingHead(m_Axis)) : 0f;

                AlignItem(item, viewAnchor, item.localScale[m_Axis] < 0 ? 1 - itemPivot : itemPivot, itemOffset);
            }
            RefreshItems(true);
        }

        // 对齐item
        private void AlignItem(RectTransform item, float viewAnchor = 0f, float itemPivot = 0f, float offset = 0f) {
            m_Offset[m_Axis] = GetItemPosInViewport(item, viewAnchor, itemPivot, offset);
            m_ContentStartPosition = content.anchoredPosition -= m_Offset;
        }

        // 获取item在viewport中的相对位置
        private float GetItemPosInViewport(RectTransform item, float viewAnchor, float itemPivot, float offset = 0f) {
            float itemAnchorPos = item.GetNormalizedPos(m_Axis, itemPivot);
            float contentAnchorPos = content.GetNormalizedPos(m_Axis, itemAnchorPos);
            return (contentAnchorPos - viewAnchor) * m_ViewSize + offset;
        }

        // 获取指定数据下标的RectTransform列表
        private void GetItemRTList(int dataIndex, List<RectTransform> items, bool from0 = true) {
            int viewIndex = GetItemViewIndex(dataIndex, from0);
            int step = from0 ? dataCount : -dataCount;
            for (; viewIndex >= 0 && viewIndex < displayCount; viewIndex += step) {
                items.Add(content.GetChild(viewIndex) as RectTransform);
            }
        }

        // 根据数据下标获取Item显示下标
        private int GetItemViewIndex(int dataIndex, bool from0) {
            if (dataCount < 1 || displayCount < 1) return -1;
            int itemIndex;
            if (from0) {
                itemIndex = dataIndex - dataStartIndex;
                if (itemIndex < 0) itemIndex += dataCount;
            } else {
                int maxIndex = (dataStartIndex + displayCount - 1) % dataCount;
                itemIndex = displayCount - 1 - (maxIndex - dataIndex);
                if (itemIndex >= displayCount) itemIndex -= dataCount;
            }
            // 下标非法
            if (itemIndex < 0 || itemIndex >= displayCount) return -1;

            return itemIndex;
        }

        protected override void LateUpdate() {
            // 先进行强制定位判定，如果有惯性，且惯性速度小于模拟滑动速度，则停止惯性滑动，切换至强制定位滑动状态
            TryAutoAlign();
            base.LateUpdate();
            UpdateSimulationScroll();
            UpdateOnItemAlign();
        }

        // 尝试执行自动对齐
        private void TryAutoAlign() {
            // 无需操作、模拟滑动中/滑动结束、未实例化显示对象、拖动中
            if (!m_AutoAlign || m_ScrollState != SimulationScrollState.None || displayCount < 1 || m_IsDragging) return;
            // 当前滑动速度
            float curScrollSpeed = inertia ? velocity[m_Axis] * Mathf.Pow(decelerationRate, Time.unscaledDeltaTime) : 0f;
            // 速度大于模拟滑动速度：保持惯性滑动
            if (Mathf.Abs(curScrollSpeed) > m_ScrollSpeed) return;
            // 终止惯性滑动，速度重置
            ResetVelocity();
            // 获取对齐位置最近的Item
            _ = GetNearItemRT(m_AlignAnchor, m_AlignItemPivot, m_AlignItemOffset, out int viewIndex, out float dis);
            if (Mathf.Abs(dis) < 0.00001f) {
                m_ScrollState = SimulationScrollState.Completed;
            } else {
                ScrollTo((viewIndex + dataStartIndex) % dataCount);
            }
        }

        private void ResetVelocity() {
            Vector2 newVelocity = velocity;
            newVelocity[m_Axis] = 0;
            velocity = newVelocity;
            m_ContentStartPosition = content.anchoredPosition;
        }

        // 更新模拟滑动状态
        private void UpdateSimulationScroll() {
            if (!content || m_ScrollState != SimulationScrollState.Scrolling) return;
            float deltaTime = Time.unscaledDeltaTime;
            if (deltaTime <= 0.0f) return;
            float deltaDis = deltaTime * m_ScrollSpeed;
            RectTransform item = GetNearItemRT(m_ScrollDataIndex, m_ScrollViewAnchor, m_ScrollItemPivot, m_ScrollItemOffset, out float itemDis);
            if (m_WaitItemShow && item) {
                m_WaitItemShow = false;

                if (item.localScale[m_Axis] < 0) {
                    ScrollTo(m_ScrollDataIndex, m_ScrollViewAnchor, 1 - m_ScrollItemPivot, m_ScrollItemOffset);
                    return;
                }
            }
            // 1、距离不减：拉到了边沿 2、已经接近当前位置
            if (Mathf.Abs(itemDis) > Mathf.Abs(m_ScrollDistance) || Mathf.Abs(deltaDis) >= Mathf.Abs(itemDis) || m_ScrollDirection * itemDis < 0f) {
                if (!item) {
                    ResetItems(dataCount);
                    dataStartIndex = m_ScrollDataIndex;
                    item = CreateItem(false, 0, 1);
                }
                AlignItem(item, m_ScrollViewAnchor, m_ScrollItemPivot, m_ScrollItemOffset);
                RefreshItems();
                content.anchoredPosition = LayoutLoopListUtils.GetContentValidPos(viewport, content, m_Axis);
                m_ScrollState = SimulationScrollState.Completed;
                ResetVelocity();
                return;
            }
            m_ScrollDistance = itemDis;
            m_Offset[m_Axis] = deltaDis * m_ScrollDirection;
            content.anchoredPosition -= m_Offset;
            RefreshItems();
        }

        /// <summary>
        /// 滑动到指定Index
        /// </summary>
        /// <param name="dataIndex">数据下标</param>
        /// <param name="anchor">viewport的锚点</param>
        /// <param name="pivot">item的中心点</param>
        /// <param name="offset">偏移值</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void ScrollTo(int dataIndex, float anchor, float pivot, float offset = 0f) {
            if (m_IsDragging) return;
            if (dataIndex < 0 || dataIndex >= dataCount) throw new ArgumentOutOfRangeException($"dataIndex : {dataIndex}", $"ScrollTo index out range: [0, {dataCount})");
            StopMovement();
            _ = GetNearItemRT(dataIndex, anchor, pivot, offset, out m_ScrollDistance);
            if (Mathf.Abs(m_ScrollDistance) < 0.00001f) return; // 已经在目标位置/无法到达目标位置
            // 初始化滑动参数
            m_ScrollState = SimulationScrollState.Scrolling;
            m_ScrollDataIndex = dataIndex;
            m_ScrollViewAnchor = anchor;
            m_ScrollItemPivot = pivot;
            m_ScrollItemOffset = offset;
            m_ScrollDirection = m_ScrollDistance > 0 ? 1 : -1;
        }

        /// <summary>
        /// 滑动到指定Index
        /// </summary>
        /// <param name="dataIndex">数据下标</param>
        public void ScrollTo(int dataIndex) => ScrollTo(dataIndex, m_AlignAnchor, m_AlignItemPivot, m_AlignItemOffset);

        /// <summary>
        /// 滑动到头部
        /// </summary>
        /// <param name="usePadding">使用边距</param>
        public void ScrollToHead(bool usePadding = true) {
            if (m_AutoAlign) {
                ScrollTo(0);
                return;
            }
            float offset = 0f;
            if (!m_Loop && usePadding) {
                offset = m_DataReverse ? m_LayoutGroup.GetPaddingTail(m_Axis) : -m_LayoutGroup.GetPaddingHead(m_Axis);
            }
            float anchor = m_DataReverse ? 1 : 0;
            float pivot = m_ScaleReverse != m_DataReverse ? 1 : 0;
            m_WaitItemShow = true;
            ScrollTo(0, anchor, pivot, offset);
        }

        /// <summary>
        /// 滑动到尾部
        /// </summary>
        /// <param name="usePadding">是否应用边距</param>
        public void ScrollToTail(bool usePadding = true) {
            if (m_AutoAlign) {
                ScrollTo(dataCount - 1);
                return;
            }
            float offset = 0f;
            if (!m_Loop && usePadding) {
                offset = m_DataReverse ? -m_LayoutGroup.GetPaddingHead(m_Axis) : m_LayoutGroup.GetPaddingTail(m_Axis);
            }
            float anchor = m_DataReverse ? 0 : 1;
            float pivot = m_ScaleReverse != m_DataReverse ? 0 : 1;
            m_WaitItemShow = true;
            ScrollTo(dataCount - 1, anchor, pivot, offset);
        }

        /// <summary>
        /// 将指定Index滑动到显示范围内
        /// </summary>
        /// <param name="dataIndex">数据下标</param>
        public void ScrollToRange(int dataIndex) {
            // 获取离中间最近的Item
            RectTransform item = GetNearItemRT(dataIndex, 0.5f, 0.5f, 0f, out float dis);
            if (item) {
                float pos0 = GetItemPosInViewport(item, 0.5f, 0);
                float pos1 = GetItemPosInViewport(item, 0.5f, 1);
                float pos0Abs = Mathf.Abs(pos0);
                float pos1Abs = Mathf.Abs(pos1);
                // viewport完全覆盖item
                if (pos0Abs + pos1Abs <= m_ViewSize) return;
                float halfViewSize = m_ViewSize / 2f;
                // item完全覆盖viewport
                if (pos0 * pos1 < 0f && pos0Abs >= halfViewSize && pos1Abs >= halfViewSize) return;
                float pivot = pos0Abs > pos1Abs ? 0 : 1;
                float anchor = dis < 0 ? 0 : 1;
                // item尺寸大于viewport时，对齐方式相反
                if (Mathf.Abs(pos1 - pos0) >= m_ViewSize) {
                    pivot = 1 - pivot;
                    anchor = 1 - anchor;
                }
                m_WaitItemShow = false;
                ScrollTo(dataIndex, anchor, pivot);
            } else {
                float anchor = dis < 0 ? 0 : 1;
                float pivot = (dis < 0) == m_ScaleReverse ? 1 : 0;
                m_WaitItemShow = true;
                ScrollTo(dataIndex, anchor, pivot);
            }
        }

        /// <summary>
        /// 根据数据下标获取离目标位置最近的Item
        /// </summary>
        /// <param name="dataIndex">数据下标</param>
        /// <param name="anchor">锚点：相对viewport</param>
        /// <param name="pivot">Item中心点</param>
        /// <param name="offset">位置偏移</param>
        /// <param name="dis">当前距离</param>
        /// <returns></returns>
        public RectTransform GetNearItemRT(int dataIndex, float anchor, float pivot, float offset, out float dis) {
            RectTransform target = null;
            dis = 0f;
            if (dataIndex < 0 || dataIndex >= dataCount) {
                return target;
            }
            List<RectTransform> items = ListPool<RectTransform>.Get();
            GetItemRTList(dataIndex, items);
            foreach (RectTransform item in items) {
                float itemDis = GetItemPosInViewport(item, anchor, pivot, offset);
                if (target && Mathf.Abs(itemDis) >= Mathf.Abs(dis)) continue;
                target = item;
                dis = itemDis;
            }
            ListPool<RectTransform>.Release(items);

            if (target) return target;

            // 未显示项，根据下标，返回带符号的最大距离
            _ = GetNearItemRT(anchor, pivot, offset, out int viewIndex, out _);
            int curIndex = viewIndex + dataStartIndex;
            dis = float.MaxValue;
            if (m_Loop) {
                int count1 = curIndex - dataIndex;
                int count2 = dataIndex - curIndex;
                if (count1 < 0) count1 = dataCount - count1;
                if (count2 < 0) count2 += dataCount;
                dis = count1 < count2 ? float.MinValue : float.MaxValue;
            } else {
                dis = dataIndex < curIndex ? float.MinValue : float.MaxValue;
            }

            if (m_DataReverse) dis = -dis;

            return target;
        }

        /// <summary>
        /// 获取离目标位置最近的最近的Item组件
        /// </summary>
        /// <param name="dataIndex">数据下标</param>
        /// <param name="anchor">锚点：相对viewport</param>
        /// <param name="pivot">Item中心点</param>
        /// <param name="offset">位置偏移</param>
        /// <returns></returns>
        public Component GetNearItem(int dataIndex, float anchor, float pivot = 0.5f, float offset = 0.5f) => GetItemComponent(GetNearItemRT(dataIndex, anchor, pivot, offset, out _), dataIndex);

        /// <summary>
        /// 获取目标位置最近的Item
        /// </summary>
        /// <param name="anchor">锚点：相对viewport</param>
        /// <param name="pivot">Item中心点</param>
        /// <param name="offset">位置偏移</param>
        /// <param name="viewIndex">显示下标</param>
        /// <param name="dis">距离</param>
        /// <returns></returns>
        public RectTransform GetNearItemRT(float anchor, float pivot, float offset, out int viewIndex, out float dis) {
            dis = 0f;
            viewIndex = -1;
            if (displayCount < 1) {
                return null;
            }
            RectTransform target = null;
            for (int i = content.childCount - 1; i >= 0; i--) {
                RectTransform item = content.GetChild(i) as RectTransform;
                float itemDis = GetItemPosInViewport(item, anchor, pivot, offset);
                if (target && Mathf.Abs(itemDis) > Mathf.Abs(dis)) continue;
                dis = itemDis;
                target = item;
                viewIndex = i;
            }
            return target;
        }

        /// <summary>
        /// 获取目标位置最近的Item
        /// </summary>
        /// <param name="anchor">锚点：相对viewport</param>
        /// <param name="pivot">Item中心点</param>
        /// <param name="offset">位置偏移</param>
        public Component GetNearItem(float anchor, float pivot = 0.5f, float offset = 0f) {
            RectTransform itemRT = GetNearItemRT(anchor, pivot, offset, out int viewIndex, out _);
            if (!itemRT) return null;
            int dataIndex = (viewIndex + dataStartIndex) % dataCount;
            return GetItemComponent(itemRT, dataIndex);
        }

#if UNITY_EDITOR
        #region 测试方法
        [ContextMenu("滑动到头部，usePadding=true")]
        private void TestScrollToHead1() => ScrollToHead(true);
        [ContextMenu("滑动到头部，usePadding=false")]
        private void TestScrollToHead2() => ScrollToHead(false);
        [ContextMenu("滑动到尾部，usePadding=true")]
        private void TestScrollToTail1() => ScrollToTail(true);
        [ContextMenu("滑动到尾部，usePadding=false")]
        private void TestScrollToTail2() => ScrollToTail(false);
        [ContextMenu("保持当前位置刷新")]
        private void TestUpdateHoldon() => UpdateHoldon(dataCount);
        #endregion
#endif
    }
}