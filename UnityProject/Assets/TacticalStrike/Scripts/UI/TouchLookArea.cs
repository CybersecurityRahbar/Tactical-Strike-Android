using UnityEngine;
using UnityEngine.EventSystems;

namespace TacticalStrike
{
    public sealed class TouchLookArea : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        private FpsPlayerController player;
        private Vector2 previous;
        public void Initialize(FpsPlayerController owner) { player = owner; }
        public void OnPointerDown(PointerEventData eventData) { previous = eventData.position; }
        public void OnDrag(PointerEventData eventData)
        {
            if (player == null) return;
            Vector2 delta = eventData.position - previous;
            previous = eventData.position;
            player.AddLookDelta(delta);
        }
    }
}
