using UnityEngine;
using UnityEngine.EventSystems;

namespace TacticalStrike
{
    // One finger owns camera-look at a time. Other fingers remain free for movement,
    // ADS, firing, stance and lean controls at the same time.
    public sealed class TouchLookArea : MonoBehaviour,
        IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private FpsPlayerController player;
        private Vector2 previous;
        private int activePointerId = int.MinValue;

        public void Initialize(FpsPlayerController owner) { player = owner; }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (activePointerId != int.MinValue) return;
            activePointerId = eventData.pointerId;
            previous = eventData.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (player == null || eventData.pointerId != activePointerId) return;
            Vector2 delta = eventData.position - previous;
            previous = eventData.position;
            player.AddLookDelta(delta);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId == activePointerId)
                activePointerId = int.MinValue;
        }

        private void OnDisable() { activePointerId = int.MinValue; }
    }
}