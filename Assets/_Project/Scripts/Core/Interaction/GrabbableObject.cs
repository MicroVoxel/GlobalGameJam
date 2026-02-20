using UnityEngine;
using Core.Player;
using Core.Managers;
using Core.World;

namespace Core.Interaction
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class GrabbableObject : MonoBehaviour, IInteractable
    {
        [SerializeField] private string _promptText = "Hold E to Grab";

        private Rigidbody _rb;
        private PlayerGrabber _currentGrabber;
        private int _originalLayer;
        private DualObject _dualObject;

        // [New] เปิดเผยสถานะว่าถูกถืออยู่ไหม
        public bool IsHeld => _currentGrabber != null;

        public string InteractionPrompt => _currentGrabber == null ? _promptText : "Press E to Drop";

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _originalLayer = gameObject.layer;
            _dualObject = GetComponent<DualObject>();
        }
        private void Start()
        {
            // ถ้าวัตถุเผลอค้างอยู่ที่ Layer "Ignore Raycast" ให้ดีดกลับเป็น Layer เดิม
            // หรือถ้ามี DualObject ให้ DualObject จัดการ Layer
            if (gameObject.layer == LayerMask.NameToLayer("Ignore Raycast"))
            {
                // คืนค่า Layer ตามสถานะ DualObject (ถ้ามี) หรือ Default
                if (_dualObject != null)
                {
                    // ให้ DualObject คำนวณ Layer เอง
                    // (แต่ต้องแน่ใจว่า DualObject.Start ทำงานแล้ว ซึ่ง Unity จะจัดการให้)
                }
                else
                {
                    gameObject.layer = LayerMask.NameToLayer("Default");
                }
            }
        }

        public void OnInteract(PlayerController player)
        {
            var grabber = player.GetComponent<PlayerGrabber>();
            if (grabber == null) return;

            if (_currentGrabber == null)
            {
                grabber.Grab(this);
            }
            else
            {
                if (_currentGrabber == grabber)
                {
                    grabber.Drop();
                }
            }
        }

        public void OnGrabbed(PlayerGrabber grabber)
        {
            _currentGrabber = grabber;
            _originalLayer = gameObject.layer;

            _rb.useGravity = false;
            _rb.linearDamping = 10f;
            _rb.constraints = RigidbodyConstraints.FreezeRotation;

            gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

            if (RealityManager.Instance != null)
                RealityManager.Instance.OnRealityChanged += CheckRealityStatus;
        }

        public void OnDropped()
        {
            if (RealityManager.Instance != null)
                RealityManager.Instance.OnRealityChanged -= CheckRealityStatus;

            _currentGrabber = null;

            _rb.useGravity = true;
            _rb.linearDamping = 0f;
            _rb.constraints = RigidbodyConstraints.None;

            gameObject.layer = _originalLayer;
        }

        private void CheckRealityStatus(bool isMaskEquipped)
        {
            if (_dualObject != null && !_dualObject.ShouldBeActive(isMaskEquipped))
            {
                if (_currentGrabber != null)
                {
                    Debug.Log("👻 Object ceased to exist in this reality. Dropping.");
                    _currentGrabber.Drop();
                }
            }
        }

        public void OnFocus() { }
        public void OnLoseFocus() { }
    }
}