using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
        public bool attack;
        public bool jump;
		public bool sprint;

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

#if ENABLE_INPUT_SYSTEM
		public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

        public void OnAttack(InputValue value)
        {
            UnityEngine.Debug.Log("OnAttack.isPressed:" + value.isPressed);
            AttackInput(value.isPressed);
        }
        
		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}
#endif


		public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
            MainSystem.stick_x = move.x;
            MainSystem.stick_z = move.y;
        } 

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
            MainSystem.stick2_x = look.x;
            MainSystem.stick2_z = look.y;
        }

        private void AttackInput(bool isPressed)
        {
            attack = isPressed;
            MainSystem.Action0 = attack;
        }
        public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
            MainSystem.Action2 = jump;
        }

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
            MainSystem.Sprint = sprint;
        }

		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
	
}