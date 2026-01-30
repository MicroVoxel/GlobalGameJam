using UnityEngine;
using Core.Managers;

namespace Core.World
{
    public enum ObjectRealityType
    {
        VisibleInRealityOnly, // เห็นตอนถอดหน้ากาก (เช่น กำแพงปิดทาง)
        VisibleInMaskOnly     // เห็นตอนใส่หน้ากาก (เช่น ทางลับ)
    }

    [RequireComponent(typeof(Collider))] // บังคับว่าต้องมี Collider
    public class DualObject : MonoBehaviour
    {
        [SerializeField] private ObjectRealityType _type;

        private Collider _collider;
        private Renderer _renderer;

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            _renderer = GetComponent<Renderer>();

            // Setup Layer อัตโนมัติตาม Type (ป้องกัน Human Error ลืมตั้ง Layer)
            AssignLayerAutomatically();
        }

        private void Start()
        {
            // Subscribe Event
            if (RealityManager.Instance != null)
            {
                RealityManager.Instance.OnRealityChanged += HandleRealityChange;

                // Sync สถานะเริ่มต้นให้ตรงกับ Manager ทันที
                HandleRealityChange(RealityManager.Instance.IsMaskEquipped);
            }
        }

        private void OnDestroy()
        {
            if (RealityManager.Instance != null)
            {
                RealityManager.Instance.OnRealityChanged -= HandleRealityChange;
            }
        }

        private void AssignLayerAutomatically()
        {
            // Layer 6 = RealityObject, Layer 7 = MaskObject (ต้องตั้งใน Unity Inspector ให้ตรง)
            if (_type == ObjectRealityType.VisibleInRealityOnly)
                gameObject.layer = LayerMask.NameToLayer("RealityObject");
            else
                gameObject.layer = LayerMask.NameToLayer("MaskObject");
        }

        private void HandleRealityChange(bool isMaskEquipped)
        {
            bool shouldBeActive = false;

            if (_type == ObjectRealityType.VisibleInRealityOnly)
            {
                // ถ้าเป็นของโลกจริง จะ Active เมื่อ "ไม่ได้ใส่หน้ากาก"
                shouldBeActive = !isMaskEquipped;
            }
            else if (_type == ObjectRealityType.VisibleInMaskOnly)
            {
                // ถ้าเป็นของโลกหน้ากาก จะ Active เมื่อ "ใส่หน้ากาก"
                shouldBeActive = isMaskEquipped;
            }

            // Toggle Collider (สำคัญมาก เพื่อให้เดินทะลุได้เมื่อมองไม่เห็น)
            if (_collider != null) _collider.enabled = shouldBeActive;

            // Note: Renderer ไม่ต้องปิดก็ได้เพราะ Camera Culling จัดการให้แล้ว (ประหยัด Performance)
            // แต่ถ้าอยากชัวร์ หรือใช้กับแสงเงาที่ Culling Mask ไม่ครอบคลุม ก็เปิดบรรทัดล่างได้ครับ
            // if (_renderer != null) _renderer.enabled = shouldBeActive;
        }
    }
}