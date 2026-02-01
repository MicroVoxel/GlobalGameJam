using UnityEngine;
using Core.World;
using Core.Interaction; // ต้องใช้เพื่อเช็ค GrabbableObject
using DG.Tweening;

namespace Content.Puzzles
{
    public class DimensionPedestal : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("เมื่อวางของแล้ว จะเปลี่ยนของชิ้นนั้นให้ไปอยู่โลกไหน?")]
        [SerializeField] private ObjectRealityType _targetReality;

        [Header("Visuals")]
        [SerializeField] private Transform _visualFeedback;
        [SerializeField] private float _cooldown = 2.0f;

        private float _lastProcessTime;

        // [Changed] เปลี่ยนจาก OnTriggerEnter เป็น OnTriggerStay
        // เพื่อให้เช็คตลอดเวลาที่วัตถุแช่อยู่ใน Trigger
        private void OnTriggerStay(Collider other)
        {
            if (Time.time < _lastProcessTime + _cooldown) return;

            // 1. ต้องมี Rigidbody (ของที่ขยับได้)
            if (other.attachedRigidbody == null) return;

            // 2. [Fix] ต้องไม่ถูกถืออยู่ (IsHeld == false)
            // ลองดึง Component GrabbableObject ออกมาเช็ค
            GrabbableObject grabbable = other.GetComponent<GrabbableObject>();
            if (grabbable != null && grabbable.IsHeld)
            {
                return; // ถ้าถืออยู่ ห้ามเปลี่ยนมิติ
            }

            // 3. ตรวจสอบว่าเป็น DualObject และยังไม่ได้อยู่โลกเป้าหมาย
            DualObject dualObj = other.GetComponent<DualObject>();
            if (dualObj != null && dualObj.CurrentType != _targetReality)
            {
                ProcessObject(dualObj);
            }
        }

        private void ProcessObject(DualObject obj)
        {
            _lastProcessTime = Time.time;

            if (_visualFeedback)
            {
                _visualFeedback.DOPunchScale(Vector3.one * 0.1f, 0.5f);
            }

            Debug.Log($"🌀 Transforming object '{obj.name}' to World: {_targetReality}");

            obj.SetRealityType(_targetReality);
        }
    }
}