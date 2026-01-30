using UnityEngine;
using UnityEngine.Events;
using Core.Patterns; // เรียกใช้ Singleton ที่เราเคยสร้าง
using Core.Input;

namespace Core.Managers
{
    public class RealityManager : Singleton<RealityManager>
    {
        [Header("Dependencies")]
        [SerializeField] private GameInputReader _inputReader;
        [SerializeField] private Camera _mainCamera;

        [Header("Layer Configuration")]
        [SerializeField] private LayerMask _defaultLayers; // Layer ที่เห็นตลอด (Default, Ground, Player)
        [SerializeField] private LayerMask _realityLayer;  // Layer 6
        [SerializeField] private LayerMask _maskLayer;     // Layer 7

        // Event ให้คนอื่นมาฟัง (ส่งค่า bool: true=ใส่หน้ากาก, false=ถอด)
        public event UnityAction<bool> OnRealityChanged;

        public bool IsMaskEquipped { get; private set; } = false;

        private void Start()
        {
            if (_mainCamera == null) _mainCamera = Camera.main;

            // ตั้งค่าเริ่มต้น (ถอดหน้ากาก)
            UpdateCameraCulling();
        }

        private void OnEnable()
        {
            if (_inputReader != null)
                _inputReader.ToggleMaskEvent += ToggleReality;
        }

        private void OnDisable()
        {
            if (_inputReader != null)
                _inputReader.ToggleMaskEvent -= ToggleReality;
        }

        // --- Core Logic ---

        public void ToggleReality()
        {
            IsMaskEquipped = !IsMaskEquipped;

            UpdateCameraCulling();

            // แจ้งเตือนทุกคนที่ Subscribe (Player, DualObjects)
            OnRealityChanged?.Invoke(IsMaskEquipped);

            Debug.Log($"🎭 Reality Switched. Mask Equipped: {IsMaskEquipped}");
        }

        private void UpdateCameraCulling()
        {
            if (_mainCamera == null) return;

            if (IsMaskEquipped)
            {
                // ใส่หน้ากาก: เห็น Default + MaskLayer (ซ่อน RealityLayer)
                _mainCamera.cullingMask = _defaultLayers | _maskLayer;
            }
            else
            {
                // ถอดหน้ากาก: เห็น Default + RealityLayer (ซ่อน MaskLayer)
                _mainCamera.cullingMask = _defaultLayers | _realityLayer;
            }
        }
    }
}