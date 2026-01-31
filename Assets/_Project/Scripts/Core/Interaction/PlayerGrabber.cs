using UnityEngine;

namespace Core.Interaction
{
    /// <summary>
    /// จัดการ Logic การถือของ (Physics Grabbing)
    /// Updated: ล็อคการหมุนตามกล้องและเพิ่มความแม่นยำในการดึง
    /// </summary>
    public class PlayerGrabber : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("จุดที่ของจะลอยอยู่ (ต้องเป็นลูกของ CameraRoot เพื่อให้ขยับตามมุมกล้อง)")]
        [SerializeField] private Transform _holdPoint;

        [Tooltip("ความเร็วในการดึงของเข้าหาจุด Hold (ยิ่งเยอะยิ่งติดมือ)")]
        [SerializeField] private float _moveSpeed = 100f; // เพิ่มความเร็วเพื่อให้ตามกล้องทัน

        [SerializeField] private float _throwForce = 10f;

        public GrabbableObject CurrentHeldObject { get; private set; }
        public bool IsGrabbing => CurrentHeldObject != null;

        private void FixedUpdate()
        {
            if (CurrentHeldObject != null)
            {
                MoveObjectToHoldPoint();
            }
        }

        public void Grab(GrabbableObject obj)
        {
            if (IsGrabbing) Drop();

            CurrentHeldObject = obj;
            CurrentHeldObject.OnGrabbed(this);
        }

        public void Drop()
        {
            if (CurrentHeldObject != null)
            {
                CurrentHeldObject.OnDropped();
                CurrentHeldObject = null;
            }
        }

        public void Throw()
        {
            if (CurrentHeldObject != null)
            {
                GrabbableObject obj = CurrentHeldObject;
                Drop();

                if (obj.TryGetComponent(out Rigidbody rb))
                {
                    rb.AddForce(_holdPoint.forward * _throwForce, ForceMode.Impulse);
                }
            }
        }

        private void MoveObjectToHoldPoint()
        {
            if (CurrentHeldObject == null) return;

            if (CurrentHeldObject.TryGetComponent(out Rigidbody rb))
            {
                // 1. Rotation: สั่งให้หมุนตาม HoldPoint ทันที (Lock Rotation)
                // การใช้ MoveRotation ใน FixedUpdate จะยังคำนวณการชนได้อยู่ แต่จะหันตามกล้องเป๊ะๆ
                rb.MoveRotation(_holdPoint.rotation);

                // 2. Position: คำนวณความเร็ว (P-Controller Logic)
                Vector3 direction = _holdPoint.position - rb.position;
                float distance = direction.magnitude;

                // ใช้ linearVelocity ดึงเข้าหาจุดเป้าหมาย
                // ยิ่งห่าง ยิ่งดึงแรง (distance * _moveSpeed) เพื่อลดอาการ Lag เวลาสะบัดเมาส์เร็วๆ
                rb.linearVelocity = direction.normalized * (distance * _moveSpeed);
            }
        }
    }
}