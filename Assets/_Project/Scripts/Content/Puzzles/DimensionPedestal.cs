using UnityEngine;
using Core.World;
using DG.Tweening;

namespace Content.Puzzles
{
    public class DimensionPedestal : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("เมื่อวางของแล้ว จะเปลี่ยนของชิ้นนั้นให้ไปอยู่โลกไหน?")]
        [SerializeField] private ObjectRealityType _targetReality;

        [Header("Visuals")]
        [SerializeField] private Transform _visualFeedback; // เช่น แท่นเรืองแสง
        [SerializeField] private float _cooldown = 2.0f; // กันเปลี่ยนกลับไปมาเร็วเกิน

        private float _lastProcessTime;

        private void OnTriggerEnter(Collider other)
        {
            if (Time.time < _lastProcessTime + _cooldown) return;

            // ตรวจสอบว่าเป็นวัตถุที่สลับโลกได้หรือไม่ (ต้องมี DualObject script)
            DualObject dualObj = other.GetComponent<DualObject>();

            // ต้องเป็นวัตถุที่มี Rigidbody (ของที่ถือมาวาง) และยังไม่ได้อยู่ในโลกเป้าหมาย
            if (dualObj != null && other.attachedRigidbody != null)
            {
                if (dualObj.CurrentType != _targetReality)
                {
                    ProcessObject(dualObj);
                }
            }
        }

        private void ProcessObject(DualObject obj)
        {
            _lastProcessTime = Time.time;

            // Visual Feedback: ย่อขยายแท่นนิดหน่อยให้รู้ว่าทำงาน
            if (_visualFeedback)
            {
                _visualFeedback.DOPunchScale(Vector3.one * 0.1f, 0.5f);
            }

            Debug.Log($"🌀 Transforming object '{obj.name}' to World: {_targetReality}");

            // สั่งเปลี่ยนมิติของวัตถุ
            // (ต้องมั่นใจว่า DualObject.cs มีฟังก์ชัน SetRealityType แล้วตามที่เคยคุยกัน)
            obj.SetRealityType(_targetReality);

            // Effect เสียงวาร์ปควรใส่ตรงนี้
        }
    }
}