using System;
using System.Collections.Generic;
using UnityEngine;

namespace SwipeableView
{
    public class UISwipeableView<TData, TContext> : MonoBehaviour where TContext : class
    {
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private Transform cardRoot;
        [SerializeField] private UISwiper swiper;
        [SerializeField] private SwipeableViewData viewData;

        public event Action<ISwipeable> ActiveCardChanged;
        public event Action<UISwipeableCard<TData, TContext>> ActionSwipedRight;
        public event Action<UISwipeableCard<TData, TContext>> ActionSwipedLeft;

        /// <summary>
        /// Is the card swiping.
        /// </summary>
        public bool IsAutoSwiping { get; private set; }

        /// <summary>
        /// Is the card exists.
        /// </summary>
        protected bool ExistsCard { get; private set; }

        protected TContext Context { get; set; }

        private List<TData> data = new List<TData>();

        private readonly Dictionary<int, UISwipeableCard<TData, TContext>> cards = new Dictionary<int, UISwipeableCard<TData, TContext>>(MaxCreateCardCount);
        private const int MaxCreateCardCount = 2;

        /// <summary>
        /// Initialize of SwipeableView
        /// </summary>
        /// <param name="data"></param>
        protected void Initialize(List<TData> data)
        {
            this.data = data;

            var createCount = data.Count > MaxCreateCardCount ?
                MaxCreateCardCount : data.Count;

            for (var i = 0; i < createCount; ++i)
            {
                var card = this.CreateCard();
                card.DataIndex = i;
                this.UpdateCardPosition(card);
                this.cards[i] = card;

                if (i == createCount - 1)
                {
                    this.UpdateSwiperTarget(card.gameObject);
                }
            }
        }

        /// <summary>
        /// Auto Swipe to the specified direction.
        /// </summary>
        /// <param name="direction"></param>
        public void AutoSwipe(SwipeDirection direction)
        {
            if (!this.ExistsCard) return;
            this.IsAutoSwiping = true;
            this.swiper.AutoSwipe(direction);
        }

        private UISwipeableCard<TData, TContext> CreateCard()
        {
            var cardObject = Instantiate(this.cardPrefab, this.cardRoot);
            var card = cardObject.GetComponent<UISwipeableCard<TData, TContext>>();
            card.SetContext(this.Context);
            card.SetVisible(false);
            card.ActionSwipedRight += swipeableCard => this.ActionSwipedRight?.Invoke(swipeableCard);
            card.ActionSwipedLeft += swipeableCard => this.ActionSwipedLeft?.Invoke(swipeableCard);
            card.ActionSwipedRight += swipeableCard =>
            {
                this.UpdateCardPosition(swipeableCard);
                this.UpdateSwiperTarget(swipeableCard.gameObject);
            };
            card.ActionSwipedLeft += swipeableCard =>
            {
                this.UpdateCardPosition(swipeableCard);
                this.UpdateSwiperTarget(swipeableCard.gameObject);
            };
            card.ActionSwipingRight += this.MoveToFrontNextCard;
            card.ActionSwipingLeft += this.MoveToFrontNextCard;

            return card;
        }

        private void UpdateCardPosition(UISwipeableCard<TData, TContext> card)
        {
            // move to the back
            card.transform.SetAsFirstSibling();
            card.UpdatePosition(Vector3.zero);
            card.UpdateRotation(Vector3.zero);

            var childCount = this.transform.childCount;
            card.UpdateScale(childCount == 1 ? 1f : this.viewData.BottomCardScale);

            // When there are three or more data,
            // Replace card index with the second index from here.
            var index = this.cards.Count < 2 ? card.DataIndex : card.DataIndex + 2;
            this.UpdateCard(card, index);
        }

        private void UpdateSwiperTarget(GameObject cardGameObject)
        {
            var childCount = this.transform.childCount;
            var target = childCount == 1 ? cardGameObject : this.transform.GetChild(1).gameObject;
            var targetSwipeable = target.GetComponent<ISwipeable>();
            this.swiper.SetTarget(target, targetSwipeable);
            this.ActiveCardChanged?.Invoke(targetSwipeable);
        }

        private void UpdateCard(UISwipeableCard<TData, TContext> card, int dataIndex)
        {
            this.IsAutoSwiping = false;
            this.ExistsCard = dataIndex != this.data.Count + 1;
            // if data doesn't exist hide card
            if (dataIndex < 0 || dataIndex > this.data.Count - 1)
            {
                card.SetVisible(false);
                return;
            }

            card.SetVisible(true);
            card.DataIndex = dataIndex;
            card.UpdateContent(this.data[dataIndex]);
        }

        private void MoveToFrontNextCard(UISwipeableCard<TData, TContext> card, float rate)
        {
            if (!this.cards.TryGetValue(card.DataIndex + 1, out var nextCard))
            {
                return;
            }

            var t = this.viewData.CardAnimationCurve.Evaluate(rate);
            nextCard.UpdateScale(Mathf.Lerp(this.viewData.BottomCardScale, 1f, t));
        }
    }

    /// <summary>
    /// Direction on the swipe.
    /// </summary>
    public enum SwipeDirection
    {
        Right,
        Left,
    }

    public sealed class SwipeableViewNullContext { }

    public class UISwipeableView<TData> : UISwipeableView<TData, SwipeableViewNullContext>
    { }
}