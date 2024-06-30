using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInput : MonoBehaviour {

    public static GameInput Instance { get; private set; }
    public event EventHandler Attack1Pressed;
    public event EventHandler Attack2Pressed;
    PlayerInputActions playerInputActions;

    private void Awake() {
        if (Instance != null) {
            Debug.LogError("There should only be one GameInput script in the scene");
        }
        else {
            Instance = this;
        }
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
    }
    void Start() {
        playerInputActions.Player.Attack_1.performed += Attack_1_performed;
        playerInputActions.Player.Attack_2.performed += Attack_2_performed;
    }

    private void Attack_2_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        Attack2Pressed?.Invoke(this, EventArgs.Empty);
    }

    private void Attack_1_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        Attack1Pressed?.Invoke(this, EventArgs.Empty);
    }

    // Update is called once per frame
    void Update() {

    }

    public Vector2 GetNormalizedMovementVector() {
        return playerInputActions.Player.Movement.ReadValue<Vector2>().normalized;
    }
    public Vector2 GetMovementVector() {
        return playerInputActions.Player.Movement.ReadValue<Vector2>();
    }
}
