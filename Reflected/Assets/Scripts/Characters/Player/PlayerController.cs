//
// Script created by Valter Lindecrantz at the Game Development Program, MAU, 2022.
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// PlayerController description
/// </summary>
public class PlayerController : MonoBehaviour
{
    PlayerControls playerControls;
    [SerializeField] ThirdPersonMovement movement;

    [SerializeField] Player player;
    [SerializeField] bool movementLocked;
    [SerializeField] bool actionLocked;
    [SerializeField] bool damageLocked;
    [SerializeField] bool dead;
    private UiManager uiManager;

    void Awake()
    {
        playerControls = new PlayerControls();
        playerControls.Player.Enable();
        uiManager = FindObjectOfType<UiManager>();
    }

    void Update()
    {
        if (player)
            Move(playerControls.Player.Movement.ReadValue<Vector2>(), playerControls.Player.Mouse.ReadValue<Vector2>());
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && !movementLocked && !actionLocked && !dead)
            movement.Jump();
    }

    public void Move(Vector2 movementVector, Vector2 mousePosition)
    {
        if (!movementLocked && !dead)
        {
            movement.Move(new Vector3(movementVector.x, 0, movementVector.y).normalized);
        }

    }

    public void Attack(InputAction.CallbackContext context)
    {
        if (context.performed && !actionLocked && !dead)
            player.Attack();
    }

    public void SpecialAttack(InputAction.CallbackContext context)
    {
        if (context.performed && !actionLocked && !dead)
            player.SpecialAttack();
    }

    public void Dash(InputAction.CallbackContext context)
    {
        if (context.performed && !movementLocked && !actionLocked && !dead)
            movement.Dash();
    }

    public void SwapDimension(InputAction.CallbackContext context)
    {
        if (context.performed && !dead && !actionLocked)
            player.TryDimensionSwap();
    }

    public void Interact(InputAction.CallbackContext context)
    {
        if (context.performed && !dead)
            player.Interact();
    }

    public void MovementLock()
    {
        movementLocked = true;
        player.GetAnim().SetBool("movementLocked", true);
    }

    public void LockPlayer()
    {
        movementLocked = true;
        player.GetAnim().SetBool("movementLocked", true);

        actionLocked = true;
        player.GetAnim().SetBool("actionLocked", true);

        //damageLocked = true;
    }

    public void UnlockPlayer()
    {
        movementLocked = false;
        player.GetAnim().SetBool("movementLocked", false);

        actionLocked = false;
        player.GetAnim().SetBool("actionLocked", false);

        //damageLocked = false;
    }

    public void MovementUnlock()
    {
        movementLocked = false;
        player.GetAnim().SetBool("movementLocked", false);
    }

    public bool GetMovementLocked()
    {
        return movementLocked;
    }

    public bool Back()
    {
        return playerControls.Player.Back.triggered;
    }

    public bool ExpandInterface()
    {
        return playerControls.Player.ExpandInterface.IsPressed();
    }

    public void SetActionLocked(bool state)
    {
        actionLocked = state;
    }

    public void SetDead()
    {
        dead = true;
    }

    public void ActionLock()
    {
        actionLocked = true;
        player.GetAnim().SetBool("actionLocked", true);
    }

    public void ActionUnlock()
    {
        actionLocked = false;
        player.GetAnim().SetBool("actionLocked", false);
    }

    public void DamageLock()
    {
        damageLocked = true;
    }

    public void DamageUnlock()
    {
        damageLocked = false;
    }

    public bool DamageLocked() => damageLocked;

    public bool GetActionLock()
    {
        return actionLocked;
    }

    public void GoToMainMenu()
    {
        uiManager.EnableCursor();
        uiManager.EnableDeathPanel();
    }

    public void GravityOn()
    {
        movement.TurnOnGravity();
    }

    public void GravityOff()
    {
        movement.TurnOffGravity();
    }

    public float Zoom()
    {
        if (playerControls.Player.ZoomIn.IsPressed())
            return 1f;
        if (playerControls.Player.ZoomOut.IsPressed())
            return -1f;
        return 0f;
    }

}