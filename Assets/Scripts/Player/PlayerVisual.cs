using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.iOS;

public class PlayerVisual : MonoBehaviour {

    [SerializeField] private float dampTime = 0.1f;
    [SerializeField] private List<float> maxAttackTimers;
    private Animator animator;
    private Player player;
    private readonly string VERTICAL_DIRECTION = "VerticalMovement";
    private readonly string HORIZONTAL_DIRECTION = "HorizontalMovement";
    private readonly string MOVEMENT_BOOL = "Moving";
    private readonly string ATTACK_BOOL = "Attacking";
    private readonly string ATTACK_INDEX = "Attack_Index";
    private float attackTimer = 0f;
    private bool isHitPaused;
    private float originalSpeed;
    Vector3 pointerMovementVector;
    // Start is called before the first frame update
    void Start() {
        animator = GetComponent<Animator>();
        player = Player.Instance;
        player.Attack1Pressed += Player_Attack1Pressed;
        player.Attack2Pressed += Player_Attack2Pressed;
        player.HitPauseStart += Player_HitPauseStart;
        player.HitPauseEnd += Player_HitPauseEnd;
    }


    private void Player_HitPauseEnd(object sender, System.EventArgs e) {
        isHitPaused = false;
        animator.speed = originalSpeed;
    }

    private void Player_HitPauseStart(object sender, System.EventArgs e) {
        if (!isHitPaused) {
            isHitPaused = true;
            originalSpeed = animator.speed;
            animator.speed = 0;
        }
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
        if (!isHitPaused)
            attackTimer -= Time.deltaTime;
        if (attackTimer <= 0) {
            animator.SetBool(ATTACK_BOOL, false);
        }
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

    private void Attack1Visual() {
        animator.SetInteger(ATTACK_INDEX, 0);
    }

    private void Attack2Visual() {
        animator.SetInteger(ATTACK_INDEX, player.GetCurrentAttackIndex() + 1);
    }
}
