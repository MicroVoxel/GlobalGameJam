using UnityEngine;

namespace Core.Player
{
    public class PlayerAnimationEvents : MonoBehaviour
    {
        private PlayerController _controller;

        private void Awake()
        {
            _controller = GetComponentInParent<PlayerController>();
            if (_controller == null) Debug.LogError("❌ AnimationEvents: No PlayerController found in parent!");
        }

        // --- Event Methods ---

        public void OnAnimGrabMask()
        {
            // Debug.Log("✋ AnimEvent: Grab Mask Triggered");
            _controller?.AnimEvent_GrabMask();
        }

        public void OnAnimEquipMask()
        {
            // Debug.Log("😷 AnimEvent: Equip Mask Triggered");
            _controller?.AnimEvent_EquipMask();
        }

        public void OnAnimFinish()
        {
            _controller?.AnimEvent_Finish();
        }
    }
}