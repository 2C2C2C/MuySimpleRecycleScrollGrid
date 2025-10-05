#if UNITY_EDITOR
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace RecycleScrollGrid
{
    public partial class RecycleScrollGridController : UIBehaviour
    {
        private const int INDEX_LABEL_FONT_SIZE = 16;

        [Space, Header("Debug draw")]
        public bool _enableDebugDraw = true;
        public bool _drawContentSize = true;
        public bool _drawGrids = true;

        private void OnDrawGizmos()
        {
            if (_enableDebugDraw)
            {
                DebugDrawStyle();
            }
        }

        private void DebugDrawStyle()
        {
            if (_drawGrids)
            {
                GizmoDrawContentGrids(m_simulatedDataCount);
            }
            if (_drawContentSize)
            {
                DrawDebugContentSize(m_simulatedDataCount);
            }
        }

        private void DrawDebugContentSize(int dataCount)
        {
            if (0 == dataCount)
            {
                return;
            }

            Vector2 rawSize = CalculateContentSize(dataCount);
            RectTransform scrollContent = _scrollRect.content;
            Vector2 contentPivot = scrollContent.pivot;
            Vector2 pivotLocalPosition = RectTransformEx.TransformNormalizedRectPositionToLocalPosition(scrollContent, contentPivot);
            Vector2 rectBottomLeftLocalPosition = pivotLocalPosition;
            rectBottomLeftLocalPosition.x = pivotLocalPosition.x - rawSize.x * contentPivot.x;
            rectBottomLeftLocalPosition.y = pivotLocalPosition.y - rawSize.y * contentPivot.y;

            Vector2 rectBottomRightLocalPosition = rectBottomLeftLocalPosition + Vector2.right * rawSize.x;
            Vector2 rectTopLeftLocalPosition = rectBottomLeftLocalPosition + Vector2.up * rawSize.y;
            Vector2 rectTopRightLocalPosition = rectBottomRightLocalPosition + Vector2.up * rawSize.y;

            Matrix4x4 localToWorld = scrollContent.localToWorldMatrix;
            Vector3 bottomLeft = localToWorld.MultiplyPoint(rectBottomLeftLocalPosition);
            Vector3 BottomRight = localToWorld.MultiplyPoint(rectBottomRightLocalPosition);
            Vector3 topLeft = localToWorld.MultiplyPoint(rectTopLeftLocalPosition);
            Vector3 topRight = localToWorld.MultiplyPoint(rectTopRightLocalPosition);

            Color prevColor = Gizmos.color;
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topLeft, bottomLeft);
            Gizmos.DrawLine(topRight, BottomRight);
            Gizmos.DrawLine(bottomLeft, BottomRight);
            Gizmos.color = prevColor;
        }

        private void GizmoDrawContentGrids(int dataCount)
        {
            if (IsCurrentLayoutDataInvalid())
            {
                return;
            }

            ScrollGridLayoutData gridLayoutData = _gridLayoutData;
            RectTransform scrollContent = _scrollRect.content;
            Vector2 rawSize = CalculateContentSize(dataCount);
            Vector2 contentPivot = scrollContent.pivot;
            Vector2 pivotLocalPosition = RectTransformEx.TransformNormalizedRectPositionToLocalPosition(scrollContent, contentPivot);
            Vector2 startLocalPosition = pivotLocalPosition;  // The start position of the first grid(bottom left)
            startLocalPosition.x = pivotLocalPosition.x - rawSize.x * contentPivot.x;
            startLocalPosition.y = pivotLocalPosition.y - rawSize.y * contentPivot.y;

            Vector2 gridSize = _gridLayoutData.gridSize;
            switch (_gridLayoutData.startCorner)
            {
                case GridLayoutGroup.Corner.LowerRight:
                    startLocalPosition.x += rawSize.x;
                    startLocalPosition.x -= gridLayoutData.RectPadding.right;
                    startLocalPosition.y += gridLayoutData.RectPadding.bottom;
                    startLocalPosition.x -= gridSize.x;
                    break;
                case GridLayoutGroup.Corner.UpperRight:
                    startLocalPosition += rawSize;
                    startLocalPosition.x -= gridLayoutData.RectPadding.right;
                    startLocalPosition.y -= gridLayoutData.RectPadding.top;
                    startLocalPosition.x -= gridSize.x;
                    startLocalPosition.y -= gridSize.y;
                    break;
                case GridLayoutGroup.Corner.UpperLeft:
                    startLocalPosition.y += rawSize.y;
                    startLocalPosition.x += gridLayoutData.RectPadding.left;
                    startLocalPosition.y -= gridLayoutData.RectPadding.top;
                    startLocalPosition.y -= gridSize.y;
                    break;
                case GridLayoutGroup.Corner.LowerLeft:
                default:
                    startLocalPosition.x += gridLayoutData.RectPadding.left;
                    startLocalPosition.y += gridLayoutData.RectPadding.bottom;
                    break;
            }

            Vector2 gridGroupMoveDirection = CalculateGridGroupMoveDirection();
            Vector2 gridGroupSpacing = Mathf.Approximately(0f, gridGroupMoveDirection.x) ? _gridLayoutData.Spacing.y * Vector2.up : _gridLayoutData.Spacing.x * Vector2.right;
            Vector2 girdMoveDirection = CalculateGridMoveDirectionInGroup();
            Vector2 gridSpacing = Mathf.Approximately(0f, girdMoveDirection.x) ? _gridLayoutData.Spacing.y * Vector2.up : _gridLayoutData.Spacing.x * Vector2.right;

            GUIStyle gridIndexLableStyle = new GUIStyle()
            {
                fontSize = INDEX_LABEL_FONT_SIZE,
                normal = new GUIStyleState() { textColor = Color.green },
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
            };

            Rect viewportRect = _scrollRect.viewport.rect;
            Matrix4x4 worldToViewportLocal = _scrollRect.viewport.worldToLocalMatrix;

            RectTransform content = _scrollRect.content;
            Matrix4x4 localToWorld = content.localToWorldMatrix;
            int constraintCount = _gridLayoutData.constraintCount;
            int groupCount = (dataCount % constraintCount > 0) ? (dataCount / constraintCount) + 1 : (dataCount / constraintCount);
            Vector2 groupStartPos = startLocalPosition;
            int gridDataIndex = 0;
            for (int i = 0; i < groupCount; i++)
            {
                Vector2 gridGroupStartPos = groupStartPos + (i * new Vector2(gridGroupMoveDirection.x * gridGroupSpacing.x, gridGroupMoveDirection.y * gridGroupSpacing.y));
                gridGroupStartPos += i * new Vector2(gridGroupMoveDirection.x * gridSize.x, gridGroupMoveDirection.y * gridSize.y);
                for (int j = 0; j < constraintCount; j++)
                {
                    Vector2 gridStartPos = gridGroupStartPos + (j * new Vector2(girdMoveDirection.x * gridSpacing.x, girdMoveDirection.y * gridSpacing.y));
                    gridStartPos += j * new Vector2(girdMoveDirection.x * gridSize.x, girdMoveDirection.y * gridSize.y);

                    // HACK Have to covert the rect to viewport's local space
                    Vector2 rectMinPoint = worldToViewportLocal.MultiplyPoint(localToWorld.MultiplyPoint(gridStartPos));
                    Rect gridRect = new Rect(rectMinPoint, gridSize);
                    bool isInterestedWithViewport = viewportRect.Overlaps(gridRect);

                    bool isOutOfDataCount = gridDataIndex > dataCount;
                    Color gizmoColor = isOutOfDataCount ? Color.yellow * 0.5f : (isInterestedWithViewport ? Color.green : 0.75f * Color.green);
                    gridIndexLableStyle.normal.textColor = gizmoColor;
                    DrawGridGizmo(gridStartPos, gridSize, localToWorld, gizmoColor);
                    DrawGridIndexHandle(gridStartPos, gridSize, gridDataIndex, localToWorld, gridIndexLableStyle);
                    ++gridDataIndex;
                }
            }
        }

        private void DrawGridGizmo(Vector2 bottomLeft, Vector2 gridSize, Matrix4x4 toWorldMatrix, Color color)
        {
            Vector2 topLeft = bottomLeft + new Vector2(0f, gridSize.y);
            Vector2 topRight = topLeft + new Vector2(gridSize.x, 0f);
            Vector2 bottomRight = bottomLeft + new Vector2(gridSize.x, 0f);

            Vector3 bottomLeftWorld = toWorldMatrix.MultiplyPoint(bottomLeft);
            Vector3 bottomRightWorld = toWorldMatrix.MultiplyPoint(bottomRight);
            Vector3 topLeftWorld = toWorldMatrix.MultiplyPoint(topLeft);
            Vector3 topRightWorld = toWorldMatrix.MultiplyPoint(topRight);

            Color prevColor = Gizmos.color;
            Gizmos.color = color;
            Gizmos.DrawLine(bottomLeftWorld, bottomRightWorld);
            Gizmos.DrawLine(topLeftWorld, topRightWorld);

            Gizmos.DrawLine(bottomLeftWorld, topLeftWorld);
            Gizmos.DrawLine(bottomRightWorld, topRightWorld);

            Gizmos.color = prevColor;
        }

        private void DrawGridIndexHandle(Vector3 bottomLeft, Vector3 itemSize, int index, Matrix4x4 toWorldMatrix, GUIStyle labelStyle = null)
        {
            Vector3 center = bottomLeft;
            center += new Vector3(0.5f * itemSize.x, 0.5f * itemSize.y, 0f);
            string labelText = index.ToString();
            Vector3 drawPosition = toWorldMatrix.MultiplyPoint(center);
            if (null == labelStyle)
            {
                labelStyle = new GUIStyle()
                {
                    fontSize = INDEX_LABEL_FONT_SIZE,
                    normal = new GUIStyleState() { textColor = Color.green },
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold,
                };
            }
            Handles.Label(drawPosition, labelText, labelStyle);
        }

    }
}
#endif