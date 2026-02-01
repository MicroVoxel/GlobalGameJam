using UnityEngine;
using DG.Tweening;

namespace Core.Player
{
    public class PlayerMaskVisuals : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private Transform _maskObject;

        [Tooltip("Renderer ของตัวหน้ากาก (ใช้สำหรับซ่อนตอนใส่เสร็จแล้วไม่ให้บังจอ)")]
        [SerializeField] private Renderer _maskRenderer; // [New]

        [Header("Anchors (Parents)")]
        [SerializeField] private Transform _pocketParent;
        [SerializeField] private Transform _handParent;
        [SerializeField] private Transform _faceParent;

        [Header("Animation Settings")]
        [SerializeField] private float _moveSpeed = 0.15f;
        [SerializeField] private Ease _moveEase = Ease.OutQuad;

        private void Start()
        {
            if (_playerController == null) _playerController = GetComponentInParent<PlayerController>();

            // หา Renderer อัตโนมัติถ้าไม่ได้ลากมา (กันลืม)
            if (_maskRenderer == null && _maskObject != null)
                _maskRenderer = _maskObject.GetComponentInChildren<Renderer>();

            if (_maskObject != null && _pocketParent != null)
            {
                SnapTo(_pocketParent);
                ShowMask(); // เริ่มต้นต้องเห็น
            }

            if (_playerController != null)
            {
                _playerController.OnMoveMaskTo += MoveMaskTo;
            }
        }

        private void OnDestroy()
        {
            if (_playerController != null)
            {
                _playerController.OnMoveMaskTo -= MoveMaskTo;
            }
        }

        private void MoveMaskTo(int locationIndex)
        {
            switch (locationIndex)
            {
                case 0: // Pocket (เก็บลงกระเป๋า)
                    ShowMask(); // ต้องเห็น
                    AttachTo(_pocketParent);
                    break;
                case 1: // Hand (อยู่ในมือ)
                    ShowMask(); // ต้องเห็น
                    AttachTo(_handParent);
                    break;
                case 2: // Face (ใส่หน้า)
                    // ย้ายไปที่หน้า แล้วสั่งซ่อนเมื่อ Tween จบ (hideOnComplete = true)
                    AttachTo(_faceParent, hideOnComplete: true);
                    break;
            }
        }

        private void AttachTo(Transform parent, bool hideOnComplete = false)
        {
            if (_maskObject == null || parent == null) return;

            _maskObject.DOKill();
            _maskObject.SetParent(parent);

            // Tween เข้าหาตำแหน่ง
            var moveTween = _maskObject.DOLocalMove(Vector3.zero, _moveSpeed).SetEase(_moveEase);
            _maskObject.DOLocalRotate(Vector3.zero, _moveSpeed).SetEase(_moveEase);

            // ถ้าเป็นจังหวะแปะหน้า ให้ซ่อนโมเดลเมื่อขยับเสร็จ
            if (hideOnComplete)
            {
                moveTween.OnComplete(() => HideMask());
            }
        }

        private void SnapTo(Transform parent)
        {
            if (_maskObject == null || parent == null) return;
            _maskObject.SetParent(parent);
            _maskObject.localPosition = Vector3.zero;
            _maskObject.localRotation = Quaternion.identity;
        }

        private void ShowMask()
        {
            if (_maskRenderer != null) _maskRenderer.enabled = true;
        }

        private void HideMask()
        {
            if (_maskRenderer != null) _maskRenderer.enabled = false;
        }
    }
}