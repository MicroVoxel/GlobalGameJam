using UnityEngine;
using Core.Player;
using Core.Managers; // เรียกใช้ Manager
using Core.World;    // เรียกใช้ DualObject

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
        private DualObject _dualObject; // [New] Reference to DualObject logic

        public string InteractionPrompt => _currentGrabber == null ? _promptText : "Press E to Drop";

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _originalLayer = gameObject.layer;
            _dualObject = GetComponent<DualObject>(); // ลองหาดูว่าชิ้นนี้เป็น DualObject ไหม
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

            // [New] Subscribe Event เพื่อเช็คสถานะโลก
            if (RealityManager.Instance != null)
                RealityManager.Instance.OnRealityChanged += CheckRealityStatus;
        }

        public void OnDropped()
        {
            // [New] Unsubscribe Event
            if (RealityManager.Instance != null)
                RealityManager.Instance.OnRealityChanged -= CheckRealityStatus;

            _currentGrabber = null;

            _rb.useGravity = true;
            _rb.linearDamping = 0f;
            _rb.constraints = RigidbodyConstraints.None;

            gameObject.layer = _originalLayer;
        }

        // [New] ตรวจสอบเมื่อโลกเปลี่ยน
        private void CheckRealityStatus(bool isMaskEquipped)
        {
            // ถ้าวัตถุนี้เป็น DualObject และไม่ควรโผล่ในโลกนี้ -> สั่ง Drop ทันที
            if (_dualObject != null && !_dualObject.ShouldBeActive(isMaskEquipped))
            {
                if (_currentGrabber != null)
                {
                    Debug.Log("👻 Object ceased to exist in this reality. Dropping.");
                    _currentGrabber.Drop();
                    // พอ Drop -> OnDropped ทำงาน -> Layer กลับเป็นค่าเดิม -> DualObject ซ่อนมันตามปกติ
                }
            }
        }

        public void OnFocus() { }
        public void OnLoseFocus() { }
    }
}