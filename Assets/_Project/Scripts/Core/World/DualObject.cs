using UnityEngine;
using Core.Managers;

namespace Core.World
{
    public enum ObjectRealityType
    {
        VisibleInRealityOnly,
        VisibleInMaskOnly
    }

    [RequireComponent(typeof(Collider))]
    public class DualObject : MonoBehaviour
    {
        [SerializeField] private ObjectRealityType _type;

        private Collider _collider;
        private Rigidbody _rb;
        private bool _initialKinematicState;

        // [New] เก็บ Reality Manager ไว้ใช้งาน
        private RealityManager _manager;

        public ObjectRealityType CurrentType => _type;

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            _rb = GetComponent<Rigidbody>();

            if (_rb != null)
            {
                _initialKinematicState = _rb.isKinematic;
            }

            AssignLayerAutomatically();
        }

        private void Start()
        {
            _manager = RealityManager.Instance;
            if (_manager != null)
            {
                _manager.OnRealityChanged += HandleRealityChange;
                HandleRealityChange(_manager.IsMaskEquipped);
            }
        }

        private void OnDestroy()
        {
            if (_manager != null)
            {
                _manager.OnRealityChanged -= HandleRealityChange;
            }
        }

        // [New] ฟังก์ชันเปลี่ยนมิติของวัตถุ (Runtime)
        public void SwitchRealityType()
        {
            if (_type == ObjectRealityType.VisibleInRealityOnly)
                SetRealityType(ObjectRealityType.VisibleInMaskOnly);
            else
                SetRealityType(ObjectRealityType.VisibleInRealityOnly);
        }

        public void SetRealityType(ObjectRealityType newType)
        {
            _type = newType;
            AssignLayerAutomatically();

            // อัปเดตสถานะทันทีให้ตรงกับโลกปัจจุบัน
            if (_manager != null)
            {
                HandleRealityChange(_manager.IsMaskEquipped);
            }
        }

        public bool ShouldBeActive(bool isMaskEquipped)
        {
            if (_type == ObjectRealityType.VisibleInRealityOnly)
                return !isMaskEquipped;

            if (_type == ObjectRealityType.VisibleInMaskOnly)
                return isMaskEquipped;

            return true;
        }

        private void AssignLayerAutomatically()
        {
            int realityLayerIndex = LayerMask.NameToLayer("RealityObject");
            int maskLayerIndex = LayerMask.NameToLayer("MaskObject");

            if (realityLayerIndex == -1 || maskLayerIndex == -1) return;

            if (_type == ObjectRealityType.VisibleInRealityOnly)
                gameObject.layer = realityLayerIndex;
            else
                gameObject.layer = maskLayerIndex;

            foreach (Transform child in transform)
            {
                child.gameObject.layer = gameObject.layer;
            }
        }

        private void HandleRealityChange(bool isMaskEquipped)
        {
            bool shouldBeActive = ShouldBeActive(isMaskEquipped);

            if (_collider != null) _collider.enabled = shouldBeActive;

            if (_rb != null)
            {
                if (!shouldBeActive)
                {
                    _rb.isKinematic = true;
                }
                else
                {
                    if (gameObject.layer != LayerMask.NameToLayer("Ignore Raycast"))
                    {
                        _rb.isKinematic = _initialKinematicState;
                        if (!_rb.isKinematic) _rb.WakeUp();
                    }
                }
            }
        }
    }
}