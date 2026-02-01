using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using System.Collections.Generic;

namespace Core.Mechanics
{
    [RequireComponent(typeof(AudioSource))] // เพิ่ม AudioSource
    public class PressurePlate : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private string _targetTag = "Untagged";

        [Header("Visual Feedback")]
        [SerializeField] private Transform _plateVisual;
        [SerializeField] private float _depressionDepth = 0.02f;
        [SerializeField] private float _animDuration = 0.2f;

        [Header("Audio")]
        [SerializeField] private AudioClip _activateSound;
        [SerializeField] private AudioClip _deactivateSound;

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

            EvaluateState();
        }

        private void OnTriggerExit(Collider other)
        {
            if (_collidersOnPlate.Contains(other))
            {
                _collidersOnPlate.Remove(other);
            }

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

            if (_plateVisual)
            {
                _plateVisual.DOKill();
                _plateVisual.DOLocalMoveY(_initialLocalPos.y - _depressionDepth, _animDuration).SetEase(Ease.OutQuad);
            }

            // Play Sound
            if (_audioSource && _activateSound) _audioSource.PlayOneShot(_activateSound);

            OnPlateActivated?.Invoke();
        }

        private void DeactivatePlate()
        {
            _isActivated = false;
            Debug.Log("🔴 Plate Deactivated");

            if (_plateVisual)
            {
                _plateVisual.DOKill();
                _plateVisual.DOLocalMoveY(_initialLocalPos.y, _animDuration).SetEase(Ease.OutQuad);
            }

            // Play Sound
            if (_audioSource && _deactivateSound) _audioSource.PlayOneShot(_deactivateSound);

            OnPlateDeactivated?.Invoke();
        }
    }
}