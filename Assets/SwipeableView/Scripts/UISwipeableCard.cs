using System;
using System.Collections;
using UnityEngine;

namespace SwipeableView
{
    public class UISwipeableCard<TData, TContext> : MonoBehaviour, ISwipeable where TContext : class
    {
        [SerializeField] private SwipeableViewData viewData;

        /// <summary>
        /// Index of Card Data.
        /// </summary>
        public int DataIndex { get; set; }

        /// <summary>
        /// Callbacks
        /// </summary>
        public event Action<UISwipeableCard<TData, TContext>> ActionSwipedRight;
        public event Action<UISwipeableCard<TData, TContext>> ActionSwipedLeft;
        public event Action<UISwipeableCard<TData, TContext>, float> ActionSwipingRight;
        public event Action<UISwipeableCard<TData, TContext>, float> ActionSwipingLeft;

        public TData Data { get; private set; }
        public TContext Context { get; private set; }

        private RectTransform cachedRect;
        private int screenSize;

        private const float Epsion = 1.192093E-07f;

        private void OnEnable()
        {
            this.cachedRect = this.transform as RectTransform;
            this.screenSize = Screen.height > Screen.width ? Screen.width : Screen.height;
        }

        private void Update()
        {
            var rectPosX = this.cachedRect.localPosition.x;
            if (Math.Abs(rectPosX) < Epsion)
            {
                this.SwipingRight(0);
                this.SwipingLeft(0);
                return;
            }

            var t = this.GetCurrentPosition(rectPosX);
            var maxAngle = rectPosX < 0 ? this.viewData.MaxInclinationAngle : -this.viewData.MaxInclinationAngle;
            this.UpdateRotation(Vector3.Lerp(Vector3.zero, new Vector3(0f, 0f, maxAngle), t));

            if (rectPosX > 0)
            {
                this.SwipingRight(t);
                this.ActionSwipingRight?.Invoke(this, t);
            }
            else if (rectPosX < 0)
            {
                this.SwipingLeft(t);
                this.ActionSwipingLeft?.Invoke(this, t);
            }
        }


        /// <summary>
        /// Updates the Content.
        /// </summary>
        /// <param name="data"></param>
        public virtual void UpdateContent(TData data)
        {
            this.Data = data;
        }

        /// <summary>
        /// Set the Context.
        /// </summary>
        /// <param name="context"></param>
        public virtual void SetContext(TContext context)
        {
            this.Context = context;
        }

        /// <summary>
        /// Set the visible.
        /// </summary>
        /// <param name="visible"></param>
        public virtual void SetVisible(bool visible)
        {
            this.gameObject.SetActive(visible);
        }

        /// <summary>
        /// Updates the position.
        /// </summary>
        /// <param name="position"></param>
        public virtual void UpdatePosition(Vector3 position)
        {
            this.cachedRect.localPosition = position;
        }

        /// <summary>
        /// Updates the rotaion.
        /// </summary>
        /// <param name="rotation"></param>
        public virtual void UpdateRotation(Vector3 rotation)
        {
            this.cachedRect.localEulerAngles = rotation;
        }

        /// <summary>
        /// Update the scale.
        /// </summary>
        /// <param name="scale"></param>
        public virtual void UpdateScale(float scale)
        {
            this.cachedRect.localScale = scale * Vector3.one;
        }

        /// <summary>
        /// Right swiping.
        /// </summary>
        /// <param name="rate"></param>
        protected virtual void SwipingRight(float rate)
        { }

        /// <summary>
        /// Left swiping.
        /// </summary>
        /// <param name="rate"></param>
        protected virtual void SwipingLeft(float rate)
        { }

#region ISwipeable
        public void Swipe(Vector2 position)
        {
            this.UpdatePosition(this.cachedRect.localPosition + new Vector3(position.x, position.y, 0));
        }

        public void EndSwipe()
        {
            // over required distance -> Auto swipe
            if (this.IsSwipedRight(this.cachedRect.localPosition))
            {
                this.AutoSwipeRight(this.cachedRect.localPosition);
            }
            else if (this.IsSwipedLeft(this.cachedRect.localPosition))
            {
                this.AutoSwipeLeft(this.cachedRect.localPosition);
            }
            // Not been reached required distance -> Return to default position
            else
            {
                this.StartCoroutine(this.MoveCoroutine(this.cachedRect.localPosition, Vector3.zero));
            }
        }

        public void AutoSwipeRight(Vector3 from)
        {
            var vec = from != Vector3.zero ? (from - Vector3.zero).normalized : Vector3.right;
            var to = vec * this.screenSize;
            this.StartCoroutine(this.MoveCoroutine(from, to, () => this.ActionSwipedRight?.Invoke(this)));
        }

        public void AutoSwipeLeft(Vector3 from)
        {
            var vec = from != Vector3.zero ? (from - Vector3.zero).normalized : Vector3.left;
            var to = vec * this.screenSize;
            this.StartCoroutine(this.MoveCoroutine(from, to, () => this.ActionSwipedLeft?.Invoke(this)));
        }
#endregion

private bool IsSwipedRight(Vector3 position)
        {
            return position.x > 0 && position.x > this.GetRequiredDistance(position.x);
        }

private bool IsSwipedLeft(Vector3 position)
        {
            return position.x < 0 && position.x < this.GetRequiredDistance(position.x);
        }

private float GetRequiredDistance(float positionX)
        {
            return positionX > 0 ? this.cachedRect.rect.size.x / 2 : -(this.cachedRect.rect.size.x / 2);
        }

private float GetCurrentPosition(float positionX)
        {
            return positionX / this.GetRequiredDistance(positionX);
        }

private IEnumerator MoveCoroutine(Vector3 from, Vector3 to, Action onComplete = null)
        {
            var endTime = Time.time + this.viewData.SwipeDuration;

            while (true)
            {
                var diff = endTime - Time.time;
                if (diff <= 0)
                {
                    break;
                }

                var rate = 1 - Mathf.Clamp01(diff / this.viewData.SwipeDuration);
                var t = this.viewData.CardAnimationCurve.Evaluate(rate);
                this.cachedRect.localPosition = Vector3.Lerp(from, to, t);
                yield return null;
            }

            this.cachedRect.localPosition = to;
            onComplete?.Invoke();
        }
    }

    public class UISwipeableCard<TData> : UISwipeableCard<TData, SwipeableViewNullContext>
    { }
}