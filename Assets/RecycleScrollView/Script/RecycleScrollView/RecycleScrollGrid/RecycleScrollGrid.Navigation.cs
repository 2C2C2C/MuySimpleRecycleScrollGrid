using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extend;

namespace RecycleScrollView
{
    public partial class RecycleScrollGrid
    {
        [System.Serializable]
        public struct RecycleScrollGridElementNavigationParams
        {
            public Vector2 normalizedPositionInViewPort;
            public Vector2 normalizedElementRectPositionOffset;
        }

        [SerializeField]
        private RecycleScrollGridElementNavigationParams _defaultNavigationParams;

        public void JumpTo(int dataIndex)
        {
            if (HasDataSource)
            {
                if (0 > dataIndex || m_dataSource.DataElementCount <= dataIndex)
                {
                    return;
                }

                RectTransform viewport = _scrollRect.viewport;
                Vector2 viewportSize = viewport.rect.size;
                Vector2 gridSize = _gridLayoutData.gridSize;
                // TODO Currently it's bottom left.
                Vector2 elementInViewportPos = RectTransformEx.TransformNormalizedRectPositionToLocalPosition(viewport, _defaultNavigationParams.normalizedPositionInViewPort);
                Vector2 elementOffset = _defaultNavigationParams.normalizedElementRectPositionOffset;
                elementInViewportPos.x -= gridSize.x * (1f - elementOffset.x);
                elementInViewportPos.y -= gridSize.y * (1f - elementOffset.y);

                RectTransform content = _scrollRect.content;
                Vector2 contentSize = content.rect.size;
                // Calculate current position
                Vector2 elementRectPosition = CalculateElementRectPositionInCoutent(dataIndex);
                Vector3 worldPos = RectTransformEx.TransformNormalizedRectPositionToWorldPosition(content, new Vector2(elementRectPosition.x / contentSize.x, elementRectPosition.y / contentSize.y));
                Vector2 elementPosInViewport = (Vector2)viewport.InverseTransformPoint(worldPos);

                Vector2 delta = elementPosInViewport - elementInViewportPos;
                Vector2 scrollSize = new Vector2(contentSize.x - viewportSize.x, contentSize.y - viewportSize.y);
                Vector2 normalizedMove = new Vector2(delta.x / scrollSize.x, delta.y / scrollSize.y);
                if (!_scrollRect.horizontal)
                {
                    normalizedMove.x = 0f;
                }
                if (!_scrollRect.vertical)
                {
                    normalizedMove.y = 0f;
                }
                Vector2 nextNormalizedPos = _scrollRect.normalizedPosition + normalizedMove;
                _scrollRect.normalizedPosition = nextNormalizedPos;
            }
        }

        private Vector2 CalculateElementRectPositionInCoutent(int dataIndex)
        {
            dataIndex = Mathf.Clamp(dataIndex, 0, SimulatedDataCount - 1);
            int primaryCount = Mathf.Max(1, _gridLayoutData.constraintCount);
            int primaryIndex = dataIndex % primaryCount;
            int secondaryIndex = dataIndex / primaryCount;

            int rowIndex, columnIndex;
            if (_gridLayoutData.startAxis == GridLayoutGroup.Axis.Horizontal)
            {
                columnIndex = primaryIndex;
                rowIndex = secondaryIndex;
            }
            else // Vertical primary
            {
                rowIndex = primaryIndex;
                columnIndex = secondaryIndex;
            }

            // Grid stepping
            float stepX = _gridLayoutData.gridSize.x + _gridLayoutData.Spacing.x;
            float stepY = _gridLayoutData.gridSize.y + _gridLayoutData.Spacing.y;

            // default: UpperLeft behaviour (x grows right, y grows down -> negative local y)
            float x = _gridLayoutData.RectPadding.left + columnIndex * stepX;
            float y = -(_gridLayoutData.RectPadding.top + rowIndex * stepY);

            // adjust for other corners
            switch (_gridLayoutData.startCorner)
            {
                case GridLayoutGroup.Corner.UpperLeft:
                    // already set
                    break;
                case GridLayoutGroup.Corner.UpperRight:
                    x = -(_gridLayoutData.RectPadding.right + columnIndex * stepX);
                    break;
                case GridLayoutGroup.Corner.LowerLeft:
                    y = _gridLayoutData.RectPadding.bottom + rowIndex * stepY;
                    break;
                case GridLayoutGroup.Corner.LowerRight:
                    x = -(_gridLayoutData.RectPadding.right + columnIndex * stepX);
                    y = _gridLayoutData.RectPadding.bottom + rowIndex * stepY;
                    break;
            }

            return new Vector2(x, y);
        }

    }
}