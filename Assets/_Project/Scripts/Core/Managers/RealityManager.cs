using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement; // [New] เพิ่ม Library นี้
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
            // หา Camera ครั้งแรก
            InitializeCamera();
        }

        private void OnEnable()
        {
            // [New] ฟัง Event เมื่อโหลดฉากใหม่
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            // [New] เลิกฟัง
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        // [New] เมื่อฉากโหลดเสร็จ ให้หา Camera ใหม่ทันที
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            InitializeCamera();

            // Optional: รีเซ็ตหน้ากากเป็น "ถอด" ทุกครั้งที่เริ่มด่านใหม่ เพื่อกันงง
            // ถ้าอยากให้จำค่าเดิมข้ามด่านได้ ให้ลบบรรทัดนี้ออก
            if (IsMaskEquipped)
            {
                IsMaskEquipped = false;
                OnRealityChanged?.Invoke(IsMaskEquipped);
            }

            // อัปเดตการมองเห็นของกล้องใหม่
            UpdateCameraCulling();
        }

        private void InitializeCamera()
        {
            if (_mainCamera == null) _mainCamera = Camera.main;
            UpdateCameraCulling();
        }

        public void ToggleReality()
        {
            IsMaskEquipped = !IsMaskEquipped;

            UpdateCameraCulling();
            OnRealityChanged?.Invoke(IsMaskEquipped);

            Debug.Log($"🎭 Reality Switched. Mask Equipped: {IsMaskEquipped}");
        }

        private void UpdateCameraCulling()
        {
            // [Fix] เช็คว่ากล้องหายไปแล้วหรือยัง (เผื่อถูกทำลาย) ถ้าหายให้หาใหม่
            if (_mainCamera == null) _mainCamera = Camera.main;

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