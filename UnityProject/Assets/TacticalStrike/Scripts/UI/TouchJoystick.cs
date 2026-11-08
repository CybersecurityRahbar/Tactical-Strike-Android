using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TacticalStrike
{
    public sealed class TouchJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private RectTransform area;
        private RectTransform knob;
        private FpsPlayerController player;
        private float radius;
        public void Initialize(FpsPlayerController owner, RectTransform knobRect, float joystickRadius)
        {
            player = owner;
            area = transform as RectTransform;
            knob = knobRect;
            radius = joystickRadius;
        }
        public void OnPointerDown(PointerEventData eventData) { OnDrag(eventData); }
        public void OnDrag(PointerEventData eventData)
        {
            if (area == null || knob == null || player == null) return;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(area, eventData.position,
                eventData.pressEventCamera, out Vector2 local)) return;
            Vector2 clamped = Vector2.ClampMagnitude(local, radius);
            knob.anchoredPosition = clamped;
            player.SetMoveVector(clamped / radius);
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            if (knob != null) knob.anchoredPosition = Vector2.zero;
            if (player != null) player.SetMoveVector(Vector2.zero);
        }
    }
}
