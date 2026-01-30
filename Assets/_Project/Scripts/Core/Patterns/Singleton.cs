using UnityEngine;

namespace Core.Patterns
{
    /// <summary>
    /// Base class for Singleton pattern.
    /// Updated: Fixes "Instance destroyed" warning in Unity Editor with Domain Reload disabled.
    /// </summary>
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static readonly object _lock = new object();
        private static bool _isQuitting = false;

        public static T Instance
        {
            get
            {
                // ถ้าเกมกำลังปิดตัวจริงๆ ให้คืนค่า null เพื่อป้องกัน Error
                if (_isQuitting)
                {
                    Debug.LogWarning($"[Singleton] Instance '{typeof(T)}' already destroyed on application quit. Won't create again - returning null.");
                    return null;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        // Unity 6 ใช้ FindFirstObjectByType แทน FindObjectOfType เดิม
                        _instance = (T)FindFirstObjectByType(typeof(T));

                        if (_instance == null)
                        {
                            GameObject singleton = new GameObject(typeof(T).Name);
                            _instance = singleton.AddComponent<T>();
                            DontDestroyOnLoad(singleton);
                        }
                    }
                    return _instance;
                }
            }
        }

        protected virtual void Awake()
        {
            // [FIX] รีเซ็ตสถานะ Quitting เมื่อเริ่ม Awake ใหม่ 
            // ช่วยแก้ปัญหาเวลาเล่นใน Editor แล้วค่า static ไม่ถูกเคลียร์
            if (_isQuitting)
            {
                _isQuitting = false;
            }

            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        protected virtual void OnApplicationQuit()
        {
            _isQuitting = true;
        }
    }
}