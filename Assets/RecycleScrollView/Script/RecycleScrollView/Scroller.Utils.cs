using UnityEngine;
using MovementType = UnityEngine.UI.ScrollRect.MovementType;
using ScrollRectEvent = UnityEngine.UI.ScrollRect.ScrollRectEvent;

namespace RecycleScrollView
{
    public partial class Scroller
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="localPosition">The local position inside viewport</param>
        private void SetContentPosition(Vector2 localPosition)
        {
            Vector2 contentCenter = m_contentBounds.center;
            if (!_horizontal)
            {
                localPosition.x = contentCenter.x;
            }
            if (!_vertical)
            {
                localPosition.y = contentCenter.y;
            }

            if (localPosition != contentCenter)
            {
                m_contentBounds.center = localPosition;
            }
        }

        private void InitDefaultBounds()
        {
            m_viewportBounds = new Bounds(_viewport.rect.center, _viewport.rect.size);
            m_contentSize = _viewport.rect.size;
            m_contentBounds = new Bounds(_viewport.rect.center, m_contentSize);
        }

        private Vector2 CalculateOffset(in Vector2 delta)
        {
            return InternalCalculateOffset(in m_viewportBounds, in m_contentBounds, in delta, _horizontal, _vertical, _movementType);
        }

        private static Vector2 InternalCalculateOffset(in Bounds viewportBounds, in Bounds contentBounds, in Vector2 delta, bool horizontal, bool vertical, MovementType movementType)
        {
            Vector2 offset = Vector2.zero;
            if (MovementType.Unrestricted == movementType)
            {
                return offset;
            }

            Vector2 min = contentBounds.min;
            Vector2 max = contentBounds.max;

            // min/max offset extracted to check if approximately 0 and avoid recalculating layout every frame (case 1010178)

            if (horizontal)
            {
                min.x += delta.x;
                max.x += delta.x;

                float maxOffset = viewportBounds.max.x - max.x;
                float minOffset = viewportBounds.min.x - min.x;

                if (-0.001f > minOffset)
                {
                    offset.x = minOffset;
                }
                else if (0.001f < maxOffset)
                {
                    offset.x = maxOffset;
                }
            }

            if (vertical)
            {
                min.y += delta.y;
                max.y += delta.y;

                float maxOffset = viewportBounds.max.y - max.y;
                float minOffset = viewportBounds.min.y - min.y;

                if (0.001f < maxOffset)
                {
                    offset.y = maxOffset;
                }
                else if (-0.001f > minOffset)
                {
                    offset.y = minOffset;
                }
            }

            return offset;
        }

        private static float RubberDelta(float overStretching, float viewSize)
        {
            return (1 - (1 / ((Mathf.Abs(overStretching) * 0.55f / viewSize) + 1))) * viewSize * Mathf.Sign(overStretching);
        }

    }
}