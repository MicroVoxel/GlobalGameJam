using UnityEngine;
using DG.Tweening;

namespace Core.Mechanics
{
    public class DoorController : MonoBehaviour
    {
        [Header("Door Visuals")]
        [SerializeField] private Transform _doorModel;
        [SerializeField] private Vector3 _openPositionOffset = new Vector3(0, 3, 0); // เลื่อนขึ้น 3 หน่วย
        [SerializeField] private float _duration = 1.0f;

        private Vector3 _closedPosition;
        private bool _isOpen = false;

        private void Start()
        {
            if (_doorModel == null) _doorModel = transform;
            _closedPosition = _doorModel.localPosition;
        }

        // เรียกจาก UnityEvent ของ Pressure Plate
        public void OpenDoor()
        {
            if (_isOpen) return;
            _isOpen = true;

            _doorModel.DOKill();
            _doorModel.DOLocalMove(_closedPosition + _openPositionOffset, _duration).SetEase(Ease.OutBounce);
        }

        public void CloseDoor()
        {
            if (!_isOpen) return;
            _isOpen = false;

            _doorModel.DOKill();
            _doorModel.DOLocalMove(_closedPosition, _duration).SetEase(Ease.OutBounce);
        }
    }
}