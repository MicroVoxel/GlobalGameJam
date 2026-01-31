using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using System.Collections.Generic;

namespace Core.Mechanics
{
    public class PressurePlate : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Tag ของวัตถุที่สามารถวางทับได้ (เช่น 'Grabbable' หรือ 'Player')")]
        [SerializeField] private string _targetTag = "Untagged";

        [Header("Visual Feedback")]
        [SerializeField] private Transform _plateVisual;
        [Tooltip("ระยะที่แท่นจะยุบลงไป (หน่วยเมตร)")]
        [SerializeField] private float _depressionDepth = 0.05f;
        [SerializeField] private float _animDuration = 0.2f;

        [Header("Events")]
        public UnityEvent OnPlateActivated;
        public UnityEvent OnPlateDeactivated;

        // ใช้ List เพื่อจำว่าวัตถุไหนเหยียบอยู่บ้าง (แม่นยำกว่าการนับ int)
        private List<Collider> _collidersOnPlate = new List<Collider>();
        private Vector3 _initialLocalPos;
        private bool _isActivated = false;

        private void Start()
        {
            if (_plateVisual != null)
            {
                _initialLocalPos = _plateVisual.localPosition;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsValidObject(other)) return;

            // ถ้ายังไม่มีใน List ให้เพิ่มเข้าไป
            if (!_collidersOnPlate.Contains(other))
            {
                _collidersOnPlate.Add(other);
            }

            EvaluateState();
        }

        private void OnTriggerExit(Collider other)
        {
            // ตอนออก ไม่ต้องเช็ค Tag เผื่อ Tag เปลี่ยนกลางทาง ให้เช็คว่าเคยอยู่ใน List เราไหมก็พอ
            if (_collidersOnPlate.Contains(other))
            {
                _collidersOnPlate.Remove(other);
            }

            // ทำความสะอาด List (เอา Null ออก เผื่อวัตถุถูกทำลายไปแล้ว)
            _collidersOnPlate.RemoveAll(c => c == null);

            EvaluateState();
        }

        private void EvaluateState()
        {
            bool shouldBeActive = _collidersOnPlate.Count > 0;

            if (shouldBeActive && !_isActivated)
            {
                ActivatePlate();
            }
            else if (!shouldBeActive && _isActivated)
            {
                DeactivatePlate();
            }
        }

        private bool IsValidObject(Collider other)
        {
            // 1. ต้องมี Rigidbody (เพื่อให้แน่ใจว่าเป็นวัตถุฟิสิกส์ หรือ Player)
            if (other.attachedRigidbody == null) return false;

            // 2. เช็ค Tag (ถ้ากำหนดไว้)
            if (!string.IsNullOrEmpty(_targetTag) && _targetTag != "Untagged")
            {
                if (!other.CompareTag(_targetTag)) return false;
            }

            return true;
        }

        private void ActivatePlate()
        {
            _isActivated = true;
            Debug.Log("🟢 Plate Activated");

            if (_plateVisual)
            {
                _plateVisual.DOKill();
                // ขยับลงจากจุดเริ่มต้น ตามระยะที่กำหนด
                _plateVisual.DOLocalMoveY(_initialLocalPos.y - _depressionDepth, _animDuration).SetEase(Ease.OutQuad);
            }

            OnPlateActivated?.Invoke();
        }

        private void DeactivatePlate()
        {
            _isActivated = false;
            Debug.Log("🔴 Plate Deactivated");

            if (_plateVisual)
            {
                _plateVisual.DOKill();
                // กลับจุดเริ่มต้น
                _plateVisual.DOLocalMoveY(_initialLocalPos.y, _animDuration).SetEase(Ease.OutQuad);
            }

            OnPlateDeactivated?.Invoke();
        }
    }
}