using UnityEngine;
using Core.Player;

namespace Core.StateMachine
{
    // --- Base State ---
    public abstract class PlayerBaseState
    {
        protected PlayerController Controller;
        protected PlayerStateMachine StateMachine;
        protected PlayerConfig Config;

        public PlayerBaseState(PlayerController controller, PlayerStateMachine stateMachine, PlayerConfig config)
        {
            Controller = controller;
            StateMachine = stateMachine;
            Config = config;
        }

        public abstract void Enter();
        public abstract void Tick();
        public abstract void Exit();
    }

    // --- State Machine Processor ---
    public class PlayerStateMachine
    {
        public PlayerBaseState CurrentState { get; private set; }
        private PlayerController _controller;

        public PlayerStateMachine(PlayerController controller)
        {
            _controller = controller;
        }

        public void Initialize(PlayerBaseState startState)
        {
            CurrentState = startState;
            CurrentState.Enter();
        }

        public void ChangeState(PlayerBaseState newState)
        {
            CurrentState?.Exit();
            CurrentState = newState;
            CurrentState?.Enter();
        }
    }

    // --- Concrete State: Standing ---
    public class PlayerStandingState : PlayerBaseState
    {
        public PlayerStandingState(PlayerController controller, PlayerStateMachine stateMachine, PlayerConfig config)
            : base(controller, stateMachine, config) { }

        public override void Enter()
        {
            // แจ้ง Animator ว่าเลิกหมอบ
            Controller.SetCrouchAnimation(false);
        }

        public override void Tick()
        {
            Controller.HandleMovement(1.0f);
            Controller.SetHeight(Config.StandHeight, Config.CenterOffset);
        }

        public override void Exit() { }
    }

    // --- Concrete State: Crouching ---
    public class PlayerCrouchingState : PlayerBaseState
    {
        public PlayerCrouchingState(PlayerController controller, PlayerStateMachine stateMachine, PlayerConfig config)
            : base(controller, stateMachine, config) { }

        public override void Enter()
        {
            // แจ้ง Animator ว่ากำลังหมอบ
            Controller.SetCrouchAnimation(true);
        }

        public override void Tick()
        {
            float speedRatio = Config.CrouchSpeed / Config.WalkSpeed;
            Controller.HandleMovement(speedRatio);
            Controller.SetHeight(Config.CrouchHeight, Config.CrouchCenter);
        }

        public override void Exit() { }
    }
}