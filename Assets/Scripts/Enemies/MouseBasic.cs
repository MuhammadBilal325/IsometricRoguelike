using KinematicCharacterController;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseBasic : BaseEnemy, KinematicCharacterController.ICharacterController {
    private enum State {
        Idle,
        Follow,
        Attacking,
        Beeping,
        Dead
    }
    public event EventHandler<AttackEventArgs> StartAttack;
    public event EventHandler StartBeeping;
    public event EventHandler OnDeath;
    public class AttackEventArgs : EventArgs {
        public float attackWarmUpTime;
    }
    [SerializeField] private float idleRange;
    [SerializeField] private float attackRange;
    //Attacks
    [SerializeField] private float attackWarmUp;
    private float attackTimer = 0f;
    //Movement
    [SerializeField] private float speed;
    [SerializeField] private float turnSharpness;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float drag;
    [SerializeField] private KinematicCharacterMotor Motor;
    [SerializeField] private Vector3 gravityVector;
    private Vector3 movementVector;
    private Vector3 targetPosition;
    private Vector3 vectorToPlayer;
    //Forces
    private Vector3 addForce;

    private State state;

    public override int GetHealth() {
        return health;
    }

    public override void Hit(BaseAttack attack, Collision collision = null) {
        health -= attack.GetDamage();
        base.InvokeHitEvent();
        if (health <= 0) {
            Destroy(this.gameObject);
        }
    }
    // Start is called before the first frame update
    private void Start() {
        Motor.CharacterController = this;
        state = State.Idle;
    }

    // Update is called once per frame
    private void Update() {
        switch (state) {
            case State.Idle:
                break;
            case State.Follow:
                break;
            case State.Attacking:
                break;
            case State.Beeping:
                break;
            case State.Dead:
                break;
            default:
                break;
        }
    }

    private void Idle() {
        if (Vector3.SqrMagnitude(transform.position - Player.Instance.transform.position) < idleRange * idleRange && Player.Instance.IsAlive()) {
            state = State.Follow;
            return;
        }
        movementVector = Vector3.zero;

    }

    private void Attack() {
     }

 
    #region KinematicCharacterController
    public void AfterCharacterUpdate(float deltaTime) {
    }

    public void BeforeCharacterUpdate(float deltaTime) {
    }

    public bool IsColliderValidForCollisions(Collider coll) {
        if (coll.CompareTag(Tags.PLAYER_ATTACK_TAG) || coll.CompareTag(Tags.ENEMY_ATTACK_TAG))
            return false;
        return true;
    }

    public void OnDiscreteCollisionDetected(Collider hitCollider) {
    }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) {
    }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) {
    }

    public void PostGroundingUpdate(float deltaTime) {
    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport) {
    }

    public void AddVelocity(Vector3 vel) {
        addForce += vel;
    }
    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime) {
        if (vectorToPlayer.sqrMagnitude > 0f)
            currentRotation = Quaternion.Lerp(currentRotation, Quaternion.LookRotation(vectorToPlayer), deltaTime * rotationSpeed);
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime) {
        Vector3 targetMovementVelocity = Vector3.zero;
        if (Motor.GroundingStatus.IsStableOnGround) {
            // Reorient velocity on slope
            currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, Motor.GroundingStatus.GroundNormal) * currentVelocity.magnitude;
            // Calculate target velocity
            Vector3 inputRight = Vector3.Cross(movementVector, Motor.CharacterUp);
            Vector3 reorientedInput = Vector3.Cross(Motor.GroundingStatus.GroundNormal, inputRight).normalized * movementVector.magnitude;
            targetMovementVelocity = reorientedInput * speed;
            // Smooth movement Velocity
            currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1 - Mathf.Exp(-turnSharpness * deltaTime));
        }
        else {
            // Add move input
            if (movementVector.sqrMagnitude > 0f) {
                targetMovementVelocity = movementVector * speed;
                // Prevent climbing on un-stable slopes with air movement
                if (Motor.GroundingStatus.FoundAnyGround) {
                    Vector3 perpenticularObstructionNormal = Vector3.Cross(Vector3.Cross(Motor.CharacterUp, Motor.GroundingStatus.GroundNormal), Motor.CharacterUp).normalized;
                    targetMovementVelocity = Vector3.ProjectOnPlane(targetMovementVelocity, perpenticularObstructionNormal);
                }
                Vector3 velocityDiff = Vector3.ProjectOnPlane(targetMovementVelocity - currentVelocity, gravityVector);
                currentVelocity += velocityDiff * speed * deltaTime;
            }
            // Gravity
            currentVelocity += gravityVector * deltaTime;
            // Drag
            currentVelocity *= (1f / (1f + (drag * deltaTime)));
        }
        if (addForce.sqrMagnitude > 0f) {
            currentVelocity += addForce;
            addForce = Vector3.zero;
        }
    }
    #endregion
}
