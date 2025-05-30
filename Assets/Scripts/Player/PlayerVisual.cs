using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.iOS;

public class PlayerVisual : MonoBehaviour {

    [SerializeField] private float movementDampTime = 0.1f;
    [SerializeField] private List<float> maxAttackTimers;
    private Animator animator;
    private Player player;
    private readonly string VERTICAL_DIRECTION = "VerticalMovement";
    private readonly string HORIZONTAL_DIRECTION = "HorizontalMovement";
    private readonly string MOVEMENT_BOOL = "Moving";
    private readonly string ATTACK_BOOL = "Attacking";
    private readonly string BLOCK_BOOL = "Blocking";
    private readonly string ATTACK_INDEX = "Attack_Index";
    private float attackTimer = 0f;
    Vector3 pointerMovementVector;
    // Start is called before the first frame update
    void Start() {
        animator = GetComponent<Animator>();
        player = Player.Instance;
        player.Attack1Pressed += Player_Attack1Pressed;
        player.Attack2Pressed += Player_Attack2Pressed;
        player.BlockChanged += Player_BlockChanged;
    }
    private void Player_BlockChanged(object sender, Player.BlockChangedArgs e) {
        animator.SetBool(BLOCK_BOOL, e.isBlocking);
    }
    private void Player_Attack1Pressed(object sender, System.EventArgs e) {
        animator.SetBool(ATTACK_BOOL, true);
        attackTimer = maxAttackTimers[0];
        Attack1Visual();
    }

    private void Player_Attack2Pressed(object sender, System.EventArgs e) {
        animator.SetBool(ATTACK_BOOL, true);
        attackTimer = maxAttackTimers[1];
        Attack2Visual();
    }


    // Update is called once per frame
    void Update() {
        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0) {
            animator.SetBool(ATTACK_BOOL, false);
        }
        SetMovement();
    }

    private void SetMovement() {
        pointerMovementVector = player.GetPlayerMovementVectorRelativeToPointer();
        if (pointerMovementVector != Vector3.zero) {
            animator.SetFloat(VERTICAL_DIRECTION, pointerMovementVector.z, movementDampTime, Time.deltaTime);
            animator.SetFloat(HORIZONTAL_DIRECTION, pointerMovementVector.x, movementDampTime, Time.deltaTime);
            animator.SetBool(MOVEMENT_BOOL, true);
        }
        else {
            animator.SetBool(MOVEMENT_BOOL, false);
        }
    }

    private void Attack1Visual() {
        animator.SetInteger(ATTACK_INDEX, 0);
    }

    private void Attack2Visual() {
        animator.SetInteger(ATTACK_INDEX, player.GetCurrentAttackIndex() + 1);
    }
}
