using UnityEngine;
using DG.Tweening;

namespace Core.Mechanics
{
    [RequireComponent(typeof(AudioSource))] // เพิ่ม AudioSource
    public class DoorController : MonoBehaviour
    {
        [Header("Door Visuals")]
        [SerializeField] private Transform _doorModel;
        [SerializeField] private Vector3 _openPositionOffset = new Vector3(0, 3, 0);
        [SerializeField] private float _duration = 1.0f;

        [Header("Audio")]
        [SerializeField] private AudioClip _doorSound; // เสียงประตู (ใช้เสียงเดียวก็ได้ หรือแยก Open/Close)

        private Vector3 _closedPosition;
        private bool _isOpen = false;
        private AudioSource _audioSource;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        private void Start()
        {
            if (_doorModel == null) _doorModel = transform;
            _closedPosition = _doorModel.localPosition;
        }

        public void OpenDoor()
        {
            if (_isOpen) return;
            _isOpen = true;

            _doorModel.DOKill();
            _doorModel.DOLocalMove(_closedPosition + _openPositionOffset, _duration).SetEase(Ease.OutBounce);

            PlaySound();
        }

        public void CloseDoor()
        {
            if (!_isOpen) return;
            _isOpen = false;

            _doorModel.DOKill();
            _doorModel.DOLocalMove(_closedPosition, _duration).SetEase(Ease.OutBounce);

            PlaySound();
        }

        private void PlaySound()
        {
            if (_audioSource && _doorSound)
            {
                _audioSource.pitch = Random.Range(0.9f, 1.1f);
                _audioSource.PlayOneShot(_doorSound);
            }
        }
    }
}