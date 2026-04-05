using UnityEngine.UI;
using UnityEngine;

namespace TEngine {
    // 循环列表需要的一些工具方法
    public static class LayoutLoopListUtils {
        // 获取头部距离
        public static int GetPaddingHead(this HorizontalOrVerticalLayoutGroup layoutGroup, int axis) {
            return axis == 0 ? layoutGroup.padding.left : layoutGroup.padding.bottom;
        }

        // 获取尾部距离
        public static int GetPaddingTail(this HorizontalOrVerticalLayoutGroup layoutGroup, int axis) {
            return axis == 0 ? layoutGroup.padding.right : layoutGroup.padding.top;
        }

        // 获取RectTransform指定轴和中心点在父对象中的归一位置
        public static float GetNormalizedPos(this RectTransform self, int axis, float pivot = 0.5f, float offset = 0f) {
            RectTransform parent = self.parent as RectTransform;
            Debug.Assert((axis | 1) == 1, $"axis错误：{axis}");
            Debug.Assert(parent != null, $"item没有父对象，无法获取相对Pivot值：{self.name}");
            // 目标位置获取
            float pos = self.anchoredPosition[axis];
            float parentSize = parent.rect.size[axis];
            float selfSize = self.rect.size[axis];
            if (Mathf.Approximately(parentSize, 0)) return 0;
            // 锚点坐标的依据值
            float anchorOffset = self.anchorMin[axis] + (self.anchorMax[axis] - self.anchorMin[axis]) * self.pivot[axis];
            float targetPos = (pivot - self.pivot[axis]) * selfSize * self.localScale[axis] + pos + anchorOffset * parentSize;
            // 添加偏移
            if (!Mathf.Approximately(offset, 0)) targetPos += offset * Mathf.Abs(self.localScale[axis]);
            float normalizedPos = targetPos / parentSize;

            return normalizedPos;
        }

        // 获取Content在Viewport内的对齐位置
        private static float GetContentAlignPos(RectTransform content, RectTransform viewport, int axis) {
            Debug.Assert((axis | 1) == 1, $"axis错误：{axis}");
            Debug.Assert(content && viewport && (content.parent == viewport), "viewport必须是content的父对象。");
            float pivot = content.pivot[axis];
            float anchorOffset = content.anchorMin[axis] + (content.anchorMax[axis] - content.anchorMin[axis]) * pivot;
            float viewportOffset = pivot * viewport.rect.size[axis];
            float contentAnchorPos = viewportOffset - anchorOffset * viewport.rect.size[axis];
            if (content.localScale[axis] < 0) {
                contentAnchorPos += (pivot - 0.5f) * content.localScale[axis] * 2 * content.rect.size[axis];
            }
            return contentAnchorPos;
        }

        // 获取Content不会回弹的位置
        public static Vector2 GetContentValidPos(RectTransform viewport, RectTransform content, int axis) {
            float contentSize = Mathf.Abs(content.rect.size[axis] * content.localScale[axis]);
            float viewSize = viewport.rect.size[axis];
            Vector2 anchorPosition = content.anchoredPosition;
            // content尺寸小于viewport，content会根据anchor和pivot对齐到viewport中
            if (contentSize <= viewSize || Mathf.Approximately(contentSize, viewSize)) {
                anchorPosition[axis] = GetContentAlignPos(content, viewport, axis);
                return anchorPosition;
            }

            float headPivot = 0, tailPivot = 1;
            if (content.localScale[axis] < 0f) {
                (headPivot, tailPivot) = (tailPivot, headPivot);
            }
            // content边缘在viewport内部，对齐到边缘
            float relativePivot = GetNormalizedPos(content, axis, headPivot);
            if (relativePivot > 0) {
                anchorPosition[axis] -= relativePivot * viewSize;
                return anchorPosition;
            }
            relativePivot = GetNormalizedPos(content, axis, tailPivot);
            if (relativePivot < 1) {
                anchorPosition[axis] += (1 - relativePivot) * viewSize;
                return anchorPosition;
            }
            return anchorPosition;
        }
    }
}
