using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInput : MonoBehaviour {

    public static GameInput Instance { get; private set; }
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

    }

    // Update is called once per frame
    void Update() {

    }

    public Vector2 GetNormalizedMovementVector() {
        return playerInputActions.Player.Movement.ReadValue<Vector2>().normalized;
    }
}
