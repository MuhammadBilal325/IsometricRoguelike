using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVisual : MonoBehaviour {

    [SerializeField] private float dampTime = 0.1f;
    private Animator animator;
    private Player player;
    private readonly string VERTICAL_DIRECTION = "VerticalMovement";
    private readonly string HORIZONTAL_DIRECTION = "HorizontalMovement";
    private readonly string MOVEMENT_BOOL = "Moving";
    Vector3 pointerMovementVector;
    // Start is called before the first frame update
    void Start() {
        animator = GetComponent<Animator>();
        player = Player.Instance;
    }

    // Update is called once per frame
    void Update() {
        SetMovement();
    }

    private void SetMovement() {
        pointerMovementVector = player.GetPlayerMovementVectorRelativeToPointer();
        if (pointerMovementVector != Vector3.zero) {
            animator.SetFloat(VERTICAL_DIRECTION, pointerMovementVector.z, dampTime, Time.deltaTime);
            animator.SetFloat(HORIZONTAL_DIRECTION, pointerMovementVector.x, dampTime, Time.deltaTime);
            animator.SetBool(MOVEMENT_BOOL, true);
        }
        else {
            animator.SetBool(MOVEMENT_BOOL, false);
        }
    }
}
