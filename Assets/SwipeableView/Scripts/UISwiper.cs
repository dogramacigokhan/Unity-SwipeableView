using UnityEngine;
using UnityEngine.EventSystems;

namespace SwipeableView
{
    public class UISwiper : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private RectTransform cachedRect;

        private ISwipeable swipeable;

        /// <summary>
        /// Set the target.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="swipeable"></param>
        public void SetTarget(GameObject target, ISwipeable swipeable)
        {
            this.cachedRect = target.transform as RectTransform;
            this.swipeable = swipeable;
        }

        /// <summary>
        /// Auto Swipe to the specified direction.
        /// </summary>
        /// <param name="direction"></param>
        public void AutoSwipe(SwipeDirection direction)
        {
            if (direction == SwipeDirection.Right)
            {
                this.swipeable.AutoSwipeRight(Vector3.zero);
            }
            else
            {
                this.swipeable.AutoSwipeLeft(Vector3.zero);
            }
        }

#region DragHandler

private Vector2 pointerStartLocalPosition;
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            if (this.cachedRect == null || !this.cachedRect.gameObject.activeInHierarchy)
            {
                return;
            }

            this.pointerStartLocalPosition = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(this.cachedRect,
                eventData.position,
                eventData.pressEventCamera,
                out this.pointerStartLocalPosition
            );
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            if (this.cachedRect == null || !this.cachedRect.gameObject.activeInHierarchy)
            {
                return;
            }

            Vector2 localCursor;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(this.cachedRect,
                    eventData.position,
                    eventData.pressEventCamera,
                    out localCursor
                ))
            {
                return;
            }

            var pointerDelta = localCursor - this.pointerStartLocalPosition;
            this.swipeable.Swipe(pointerDelta);
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            if (this.cachedRect == null || !this.cachedRect.gameObject.activeInHierarchy)
            {
                return;
            }

            this.swipeable.EndSwipe();
        }
#endregion
    }
}