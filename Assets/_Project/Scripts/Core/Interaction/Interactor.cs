using UnityEngine;
using Core.Input;
using Core.Player;
using System;

namespace Core.Interaction
{
    public class Interactor : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private GameInputReader _inputReader;
        [SerializeField] private PlayerController _playerController; // เอาไว้อ้างอิงตัวคนกด

        [Header("Settings")]
        [SerializeField] private Transform _raycastOrigin; // ปกติคือ Camera
        [SerializeField] private float _interactionRange = 3.0f;
        [SerializeField] private LayerMask _interactableLayer; // Layer ของสิ่งของ (Default, Reality, Mask)

        // Events สำหรับ UI (บอก UI ว่าตอนนี้มองอะไรอยู่)
        public event Action<bool, string> OnInteractableStateChanged; // bool: found?, string: prompt

        private IInteractable _currentInteractable;
        private RaycastHit _hitInfo;

        private void Start()
        {
            if (_raycastOrigin == null) _raycastOrigin = Camera.main.transform;
            if (_playerController == null) _playerController = GetComponentInParent<PlayerController>();
        }

        private void OnEnable()
        {
            if (_inputReader != null)
                _inputReader.InteractEvent += PerformInteraction;
        }

        private void OnDisable()
        {
            if (_inputReader != null)
                _inputReader.InteractEvent -= PerformInteraction;
        }

        private void Update()
        {
            CheckForInteractable();
        }

        private void CheckForInteractable()
        {
            // ยิง Raycast ตรงกลางหน้าจอ
            if (Physics.Raycast(_raycastOrigin.position, _raycastOrigin.forward, out _hitInfo, _interactionRange, _interactableLayer))
            {
                // ลองดึง Component ที่มี Interface IInteractable ออกมา
                IInteractable interactable = _hitInfo.collider.GetComponent<IInteractable>();

                if (interactable != null)
                {
                    // ถ้าเจอของชิ้นใหม่ (หรือของเดิม)
                    if (interactable != _currentInteractable)
                    {
                        // เลิกโฟกัสอันเก่า
                        if (_currentInteractable != null) _currentInteractable.OnLoseFocus();

                        // โฟกัสอันใหม่
                        _currentInteractable = interactable;
                        _currentInteractable.OnFocus();

                        // แจ้ง UI
                        OnInteractableStateChanged?.Invoke(true, _currentInteractable.InteractionPrompt);
                    }
                    return; // เจอแล้ว จบ function
                }
            }

            // ถ้าไม่เจออะไรเลย หรือเจอแต่ไม่ใช่ IInteractable
            if (_currentInteractable != null)
            {
                _currentInteractable.OnLoseFocus();
                _currentInteractable = null;

                // แจ้ง UI ให้ปิด
                OnInteractableStateChanged?.Invoke(false, string.Empty);
            }
        }

        private void PerformInteraction()
        {
            if (_currentInteractable != null)
            {
                _currentInteractable.OnInteract(_playerController);
            }
        }

        // Visualize Ray in Editor
        private void OnDrawGizmos()
        {
            if (_raycastOrigin != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(_raycastOrigin.position, _raycastOrigin.forward * _interactionRange);
            }
        }
    }
}