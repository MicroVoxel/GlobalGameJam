using UnityEngine;

namespace Core.Player
{
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "Game/Player Config")]
    public class PlayerConfig : ScriptableObject
    {
        [Header("Movement")]
        public float WalkSpeed = 4.0f;
        public float CrouchSpeed = 2.0f;
        public float SmoothTime = 0.1f;

        [Header("Camera & Look")]
        public float LookSensitivityX = 1.0f;
        public float LookSensitivityY = 1.0f;
        public float TopClamp = 90.0f;
        public float BottomClamp = -90.0f;

        [Header("Physics")]
        public float Gravity = -15.0f;
        public float JumpHeight = 1.2f; // [New] ความสูงในการกระโดด
        public float GroundCheckRadius = 0.2f;
        public LayerMask GroundLayer;

        [Header("Stealth")]
        public float StandHeight = 2.0f;
        public float CrouchHeight = 1.0f;
        public float CrouchTransitionSpeed = 10f;
        public Vector3 CenterOffset = new Vector3(0, 1, 0);
        public Vector3 CrouchCenter = new Vector3(0, 0.5f, 0);
    }
}