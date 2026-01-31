using UnityEngine;
using DG.Tweening;

namespace Core.Player
{
    /// <summary>
    /// จัดการความสูงของกล้องโดยใช้ DOTween และ Anchors (Stand/Crouch)
    /// </summary>
    public class PlayerCameraVisuals : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerController _playerController;
        [Tooltip("Object ที่เป็นจุดหมุนของกล้อง (CameraRoot)")]
        [SerializeField] private Transform _cameraRoot;

        [Header("Anchors")]
        [Tooltip("สร้าง Empty Object ไว้ที่ระดับสายตาตอนยืน แล้วลากมาใส่")]
        [SerializeField] private Transform _standAnchor;

        [Tooltip("สร้าง Empty Object ไว้ที่ระดับสายตาตอนนั่ง แล้วลากมาใส่")]
        [SerializeField] private Transform _crouchAnchor;

        [Header("Settings")]
        [SerializeField] private float _transitionDuration = 0.3f;
        [SerializeField] private Ease _transitionEase = Ease.OutCubic;

        private void Start()
        {
            if (_playerController == null) _playerController = GetComponentInParent<PlayerController>();

            // ถ้าไม่ได้ลาก CameraRoot มา ให้ลองขอจาก Controller
            if (_cameraRoot == null && _playerController != null)
            {
                _cameraRoot = _playerController.CameraRoot;
            }

            // Snap ตำแหน่งเริ่มต้นทันที
            if (_standAnchor != null && _cameraRoot != null)
            {
                _cameraRoot.localPosition = _standAnchor.localPosition;
            }

            // Subscribe Event
            if (_playerController != null)
            {
                _playerController.OnCrouchChanged += UpdateCameraHeight;
            }
        }

        private void OnDestroy()
        {
            if (_playerController != null)
            {
                _playerController.OnCrouchChanged -= UpdateCameraHeight;
            }
        }

        private void UpdateCameraHeight(bool isCrouching)
        {
            if (_cameraRoot == null || _standAnchor == null || _crouchAnchor == null) return;

            // เลือกเป้าหมาย
            Transform targetAnchor = isCrouching ? _crouchAnchor : _standAnchor;

            // ฆ่า Tween เก่าก่อน
            _cameraRoot.DOKill();

            // ใช้ DOTween เลื่อน Local Position ไปหา Anchor
            _cameraRoot.DOLocalMove(targetAnchor.localPosition, _transitionDuration)
                .SetEase(_transitionEase);
        }
    }
}