using UnityEngine;
using UnityEngine.InputSystem;

public class SC_Input_Manager : MonoBehaviour
{
[Header("Wymagane akcje z Input")]
[SerializeField] private InputActionReference move_action;
[SerializeField] private InputActionReference sprint_action;
[SerializeField] private InputActionReference jump_action;
[SerializeField] private InputActionReference crouch_action;
[SerializeField] private InputActionReference look_action;
[SerializeField] private InputActionReference interact_action;

 
[Header("Wymagane referencje")]
public FirstPersonMovement player_move;
public FirstPersonLook player_look;
public Jump player_jump;
public Crouch player_crouch;
//----------------------------------------------------------------
private void OnEnable()
{
if(move_action.action==null)throw new System.NullReferenceException("Brak 'Move Action' Action!");
if(jump_action.action==null)throw new System.NullReferenceException("Brak 'Jump Action' Action!");
if(crouch_action.action==null)throw new System.NullReferenceException("Brak 'Crouch' Action!");
if(look_action.action==null)throw new System.NullReferenceException("Brak 'Look' Action!");
if(interact_action.action==null)throw new System.NullReferenceException("Brak 'Interact' Action!");
if(player_jump==null)throw new System.NullReferenceException("Brak Player!");

// W³¹czamy akcje i subskrybujemy
move_action.action.Enable();
sprint_action.action.started+=OnSprintStarted;
sprint_action.action.canceled+=OnSprintCanceled;
jump_action.action.Enable();
jump_action.action.started+=HandleJump;
jump_action.action.canceled+=JumpEnd;
crouch_action.action.Enable();
crouch_action.action.started+=OnCrouchStarted;
crouch_action.action.canceled+=OnCrouchCanceled;
look_action.action.Enable();
interact_action.action.started+=Interact;
interact_action.action.Enable();
}
//----------------------------------------------------------------
private void OnDisable()
{
sprint_action.action.started-=OnSprintStarted;
sprint_action.action.canceled-=OnSprintStarted;move_action.action.Disable();
jump_action.action.started-=HandleJump;
jump_action.action.canceled-=JumpEnd;
jump_action.action.Disable();
crouch_action.action.started-=OnCrouchStarted;
crouch_action.action.canceled-=OnCrouchCanceled;
crouch_action.action.Disable();
look_action.action.Disable();
interact_action.action.started-=Interact;
interact_action.action.Disable();
}
//----------------------------------------------------------------
private void Update()
{
player_move.Move_update(move_action.action.ReadValue<Vector2>());
player_look.Look(look_action.action.ReadValue<Vector2>());
}
//----------------------------------------------------------------
private void HandleJump(InputAction.CallbackContext context)
{
player_jump.Execute_Jump();
}
//----------------------------------------------------------------
private void JumpEnd(InputAction.CallbackContext context)
{
}
//----------------------------------------------------------------
private void OnCrouchStarted(InputAction.CallbackContext context)
{
player_crouch.Crouch_start();
}
//----------------------------------------------------------------
private void OnCrouchCanceled(InputAction.CallbackContext context)
{ 
player_crouch.Crouch_end();
}
//----------------------------------------------------------------
private void OnSprintStarted(InputAction.CallbackContext context)
{ 
player_move.Run_Start();
}//----------------------------------------------------------------
private void OnSprintCanceled(InputAction.CallbackContext context)
{ 
player_move.Run_Stop();
}
//----------------------------------------------------------------
private void Interact(InputAction.CallbackContext context)
{
}
//----------------------------------------------------------------
}

