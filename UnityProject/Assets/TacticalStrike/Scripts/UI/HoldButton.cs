using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TacticalStrike
{
    public sealed class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public Action<bool> OnHoldChanged;
        private bool held;
        public void OnPointerDown(PointerEventData eventData)
        {
            if (held) return;
            held = true;
            OnHoldChanged?.Invoke(true);
        }
        public void OnPointerUp(PointerEventData eventData) { Release(); }
        public void OnPointerExit(PointerEventData eventData) { Release(); }
        private void Release()
        {
            if (!held) return;
            held = false;
            OnHoldChanged?.Invoke(false);
        }
        private void OnDisable() { Release(); }
    }
}
