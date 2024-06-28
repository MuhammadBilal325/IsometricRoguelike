using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVisual : MonoBehaviour {

    private Animator animator;
    private Player player;
    private readonly string MOVE_FORWARD_BOOL = "MoveForward";
    private readonly string MOVE_BACKWARD_BOOL = "MoveBackward";
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
        if (player.GetPlayerMovementVectorRelativeToPointer().z > 0) {
            animator.SetBool(MOVE_FORWARD_BOOL, true);
            animator.SetBool(MOVE_BACKWARD_BOOL, false);
        }
        else if (player.GetPlayerMovementVectorRelativeToPointer().z < 0) {
            animator.SetBool(MOVE_FORWARD_BOOL, false);
            animator.SetBool(MOVE_BACKWARD_BOOL, true);
        }
        else {
            animator.SetBool(MOVE_FORWARD_BOOL, false);
            animator.SetBool(MOVE_BACKWARD_BOOL, false);
        }
    }
}
