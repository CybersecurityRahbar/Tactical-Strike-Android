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
        private int activePointerId = int.MinValue;

        public void Initialize(FpsPlayerController owner, RectTransform knobRect, float joystickRadius)
        {
            player = owner;
            area = transform as RectTransform;
            knob = knobRect;
            radius = joystickRadius;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (activePointerId != int.MinValue) return;
            activePointerId = eventData.pointerId;
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId != activePointerId || area == null || knob == null || player == null)
                return;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(area, eventData.position,
                eventData.pressEventCamera, out Vector2 local)) return;
            Vector2 clamped = Vector2.ClampMagnitude(local, radius);
            knob.anchoredPosition = clamped;
            player.SetMoveVector(clamped / radius);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId != activePointerId) return;
            Release();
        }

        private void OnDisable() { Release(); }

        private void Release()
        {
            activePointerId = int.MinValue;
            if (knob != null) knob.anchoredPosition = Vector2.zero;
            if (player != null) player.SetMoveVector(Vector2.zero);
        }
    }
}