using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using System.Collections.Generic;
using Core.Interaction;

namespace Core.Mechanics
{
    [RequireComponent(typeof(AudioSource))] // บังคับว่าต้องมี AudioSource
    public class PressurePlate : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private string _targetTag = "Untagged";

        [Header("Visual Feedback")]
        [SerializeField] private Transform _plateVisual;
        [SerializeField] private float _depressionDepth = 0.02f;
        [SerializeField] private float _animDuration = 0.2f;

        [Header("Audio")]
        [SerializeField] private AudioClip _activateSound;   // เสียงกดลง
        [SerializeField] private AudioClip _deactivateSound; // เสียงเด้งขึ้น

        [Header("Events")]
        public UnityEvent OnPlateActivated;
        public UnityEvent OnPlateDeactivated;

        private List<Collider> _collidersOnPlate = new List<Collider>();
        private Vector3 _initialLocalPos;
        private bool _isActivated = false;
        private AudioSource _audioSource;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }

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

            if (!_collidersOnPlate.Contains(other))
            {
                _collidersOnPlate.Add(other);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (_collidersOnPlate.Contains(other))
            {
                _collidersOnPlate.Remove(other);
            }
            _collidersOnPlate.RemoveAll(c => c == null);
        }

        private void Update()
        {
            EvaluateState();
        }

        private void EvaluateState()
        {
            int validCount = 0;

            foreach (var col in _collidersOnPlate)
            {
                if (col == null) continue;

                GrabbableObject grabbable = col.GetComponent<GrabbableObject>();

                // ถ้าของถูกถืออยู่ ไม่นับเป็นน้ำหนัก
                if (grabbable != null && grabbable.IsHeld)
                {
                    continue;
                }

                validCount++;
            }

            bool shouldBeActive = validCount > 0;

            // ตรวจสอบ State Change เพื่อเล่นเสียงแค่ครั้งเดียวตอนสถานะเปลี่ยน
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
            if (other.attachedRigidbody == null) return false;
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

            // Animation
            if (_plateVisual)
            {
                _plateVisual.DOKill();
                _plateVisual.DOLocalMoveY(_initialLocalPos.y - _depressionDepth, _animDuration).SetEase(Ease.OutQuad);
            }

            // [Re-Added] Sound
            if (_audioSource && _activateSound)
            {
                _audioSource.pitch = Random.Range(0.95f, 1.05f); // สุ่ม Pitch นิดหน่อยให้ดูเป็นธรรมชาติ
                _audioSource.PlayOneShot(_activateSound);
            }

            OnPlateActivated?.Invoke();
        }

        private void DeactivatePlate()
        {
            _isActivated = false;
            Debug.Log("🔴 Plate Deactivated");

            // Animation
            if (_plateVisual)
            {
                _plateVisual.DOKill();
                _plateVisual.DOLocalMoveY(_initialLocalPos.y, _animDuration).SetEase(Ease.OutQuad);
            }

            // [Re-Added] Sound
            if (_audioSource && _deactivateSound)
            {
                _audioSource.pitch = Random.Range(0.95f, 1.05f);
                _audioSource.PlayOneShot(_deactivateSound);
            }

            OnPlateDeactivated?.Invoke();
        }
    }
}