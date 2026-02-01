using UnityEngine;

namespace Core.World
{
    /// <summary>
    /// แปะ Script นี้ไว้ที่วัตถุ (เช่น กล่อง) เพื่อให้จำตำแหน่งและสถานะเริ่มต้น
    /// และสามารถกลับมาที่เดิมได้เมื่อตกแมพ
    /// </summary>
    public class Respawnable : MonoBehaviour
    {
        private Vector3 _startPosition;
        private Quaternion _startRotation;
        private Rigidbody _rb;

        // [New] เพิ่มการจำสถานะโลก (DualObject)
        private DualObject _dualObject;
        private ObjectRealityType _startRealityType;

        private void Awake()
        {
            // 1. จำตำแหน่งฟิสิกส์
            _startPosition = transform.position;
            _startRotation = transform.rotation;
            _rb = GetComponent<Rigidbody>();

            // 2. จำสถานะโลก (ถ้ามี)
            _dualObject = GetComponent<DualObject>();
            if (_dualObject != null)
            {
                _startRealityType = _dualObject.CurrentType;
            }
        }

        public void Respawn()
        {
            // รีเซ็ตตำแหน่งและหยุดความเร็ว
            if (_rb != null)
            {
                _rb.linearVelocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
                _rb.position = _startPosition;
                _rb.rotation = _startRotation;
            }
            else
            {
                transform.position = _startPosition;
                transform.rotation = _startRotation;
            }

            // [New] รีเซ็ตสถานะโลกกลับเป็นค่าเริ่มต้น
            // เช่น ถ้าตอนเริ่มเป็น Reality Only แล้วโดนเปลี่ยนเป็น Mask Only
            // พอตายเกิดใหม่ ต้องกลับเป็น Reality Only เหมือนเดิม เพื่อให้ผู้เล่นเริ่มแก้ปริศนาใหม่ได้
            if (_dualObject != null)
            {
                _dualObject.SetRealityType(_startRealityType);
            }

            //Debug.Log($"♻️ Object '{name}' respawned and reset to original reality state.");
        }
    }
}