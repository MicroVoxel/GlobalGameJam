using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Core.Input
{
    [CreateAssetMenu(fileName = "GameInputReader", menuName = "Game/Input Reader")]
    public class GameInputReader : ScriptableObject, GameInput.IGameplayActions
    {
        public event UnityAction<Vector2> MoveEvent;
        public event UnityAction<Vector2> LookEvent;
        public event UnityAction JumpEvent;
        public event UnityAction CrouchEvent;
        public event UnityAction InteractEvent;
        public event UnityAction ToggleMaskEvent;

        private GameInput _gameInput;

        private void OnEnable()
        {
            // พยายามเปิดอัตโนมัติ (สำหรับ Editor Mode หรือตอน Asset ถูกโหลดครั้งแรก)
            EnableInput();
        }

        private void OnDisable()
        {
            DisableInput();
        }

        /// <summary>
        /// สั่งเปิดรับ Input อย่างชัดเจน (ควรเรียกจาก PlayerController หรือ InputManager ตอนเริ่มเกม)
        /// </summary>
        public void EnableInput()
        {
            if (_gameInput == null)
            {
                _gameInput = new GameInput();
                _gameInput.Gameplay.SetCallbacks(this);
            }

            _gameInput.Gameplay.Enable();
            Debug.Log($"🔌 GameInputReader Enabled: {_gameInput.asset.enabled}");
        }

        public void DisableInput()
        {
            _gameInput?.Gameplay.Disable();
        }

        // --- Interface Implementation ---

        public void OnMove(InputAction.CallbackContext context)
        {
            MoveEvent?.Invoke(context.ReadValue<Vector2>());
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            LookEvent?.Invoke(context.ReadValue<Vector2>());
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed) JumpEvent?.Invoke();
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed) CrouchEvent?.Invoke();
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed) InteractEvent?.Invoke();
        }

        public void OnToggleMask(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed) ToggleMaskEvent?.Invoke();
        }
    }
}