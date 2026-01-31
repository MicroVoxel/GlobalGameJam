using UnityEngine;
using UnityEngine.Events;
using Core.Patterns;
using Core.Input;

namespace Core.Managers
{
    public class RealityManager : Singleton<RealityManager>
    {
        [Header("Dependencies")]
        [SerializeField] private GameInputReader _inputReader;
        [SerializeField] private Camera _mainCamera;

        [Header("Layer Configuration")]
        [SerializeField] private LayerMask _defaultLayers;
        [SerializeField] private LayerMask _realityLayer;
        [SerializeField] private LayerMask _maskLayer;

        public event UnityAction<bool> OnRealityChanged;
        public bool IsMaskEquipped { get; private set; } = false;

        private void Start()
        {
            if (_mainCamera == null) _mainCamera = Camera.main;
            UpdateCameraCulling();
        }

        // [Removed] ลบ OnEnable/OnDisable ที่คอยฟัง InputReader ออก
        // เพราะเราจะย้ายการควบคุม Input ไปที่ PlayerController เพื่อให้รออนิเมชั่นก่อน

        public void ToggleReality()
        {
            IsMaskEquipped = !IsMaskEquipped;

            UpdateCameraCulling();
            OnRealityChanged?.Invoke(IsMaskEquipped);

            Debug.Log($"🎭 Reality Switched. Mask Equipped: {IsMaskEquipped}");
        }

        private void UpdateCameraCulling()
        {
            if (_mainCamera == null) return;

            if (IsMaskEquipped)
            {
                _mainCamera.cullingMask = _defaultLayers | _maskLayer;
            }
            else
            {
                _mainCamera.cullingMask = _defaultLayers | _realityLayer;
            }
        }
    }
}