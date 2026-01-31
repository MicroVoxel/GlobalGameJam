using UnityEngine;
using DG.Tweening;

namespace Core.Player
{
    public class PlayerMaskVisuals : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private Transform _maskObject;

        [Header("Anchors (Parents)")]
        [Tooltip("จุดอ้างอิงตอนเก็บหน้ากาก (เช่น เอว)")]
        [SerializeField] private Transform _pocketParent;

        [Tooltip("จุดอ้างอิงบนมือ (Hand Bone ของ Model) - ต้องมี!")]
        [SerializeField] private Transform _handParent;

        [Tooltip("จุดอ้างอิงบนหน้า (CameraRoot หรือ Head Bone)")]
        [SerializeField] private Transform _faceParent;

        [Header("Animation Settings")]
        [SerializeField] private float _moveSpeed = 0.15f; // ความเร็วในการพุ่งเข้าหา Anchor
        [SerializeField] private Ease _moveEase = Ease.OutQuad;

        private void Start()
        {
            if (_playerController == null) _playerController = GetComponentInParent<PlayerController>();

            if (_maskObject != null && _pocketParent != null)
            {
                // เริ่มต้นให้หน้ากากอยู่ที่กระเป๋าแบบ Snap (ไม่ต้อง Tween)
                SnapTo(_pocketParent);
            }

            if (_playerController != null)
            {
                _playerController.OnMoveMaskTo += MoveMaskTo;
                Debug.Log("✅ PlayerMaskVisuals: Subscribed to Controller");
            }
            else
            {
                Debug.LogError("❌ PlayerMaskVisuals: PlayerController not found!");
            }
        }

        private void OnDestroy()
        {
            if (_playerController != null)
            {
                _playerController.OnMoveMaskTo -= MoveMaskTo;
            }
        }

        // 0 = Pocket, 1 = Hand, 2 = Face
        private void MoveMaskTo(int locationIndex)
        {
            //Debug.Log($"🎭 Visuals Moving Mask To Index: {locationIndex}");

            switch (locationIndex)
            {
                case 0: // Pocket
                    AttachTo(_pocketParent);
                    break;
                case 1: // Hand
                    AttachTo(_handParent);
                    break;
                case 2: // Face
                    AttachTo(_faceParent);
                    break;
            }
        }

        private void AttachTo(Transform parent)
        {
            if (_maskObject == null || parent == null)
            {
                Debug.LogWarning($"⚠️ Cannot attach mask. Mask: {_maskObject}, Parent: {parent}");
                return;
            }

            // ฆ่า Tween เก่าก่อน
            _maskObject.DOKill();

            // 1. ย้าย Parent ทันที เพื่อให้ Transform เกาะติดไปกับการเคลื่อนไหวของ Parent นั้นๆ
            _maskObject.SetParent(parent);

            // 2. ใช้ DOTween เลื่อน Local Position/Rotation เข้าหา 0,0,0 ของ Parent ใหม่
            // เพื่อให้ดูเหมือนมือหยิบไป หรือแปะลงหน้า
            _maskObject.DOLocalMove(Vector3.zero, _moveSpeed).SetEase(_moveEase);
            _maskObject.DOLocalRotate(Vector3.zero, _moveSpeed).SetEase(_moveEase);
        }

        private void SnapTo(Transform parent)
        {
            if (_maskObject == null || parent == null) return;
            _maskObject.SetParent(parent);
            _maskObject.localPosition = Vector3.zero;
            _maskObject.localRotation = Quaternion.identity;
        }
    }
}