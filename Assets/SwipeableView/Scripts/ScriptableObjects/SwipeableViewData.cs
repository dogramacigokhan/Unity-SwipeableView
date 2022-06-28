using UnityEngine;

namespace SwipeableView
{
    [CreateAssetMenu(menuName = "ScriptableObject/Create SwipeableViewData", fileName = "SwipeableViewData")]
    public class SwipeableViewData : ScriptableObject
    {
        [SerializeField] private float swipeDuration = 0.28f;
        [SerializeField] private float bottomCardScale = 0.92f;
        [SerializeField] private int maxInclinationAngle = 10;
        [SerializeField] private AnimationCurve cardAnimationCurve = new AnimationCurve(
            new Keyframe(0f, 0f, 0f, 2f), new Keyframe(1f, 1f, 0f, 0f));

        public float SwipeDuration => this.swipeDuration;
        public float BottomCardScale => this.bottomCardScale;
        public int MaxInclinationAngle => this.maxInclinationAngle;
        public AnimationCurve CardAnimationCurve => this.cardAnimationCurve;
    }
}