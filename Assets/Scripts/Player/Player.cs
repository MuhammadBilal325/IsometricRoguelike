using Cinemachine.Utility;
using KinematicCharacterController;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class Player : MonoBehaviour, KinematicCharacterController.ICharacterController, IHittable {

    public static Player Instance { get; private set; }

    public enum State {
        Normal,
        Dashing,
        Dead
    }

    public event EventHandler Attack1Pressed;
    public event EventHandler Attack2Pressed;
    public event EventHandler<BlockChangedArgs> BlockChanged;
    public class BlockChangedArgs : EventArgs {
        public bool isBlocking;
    }
    public event EventHandler OnHit;
    public event EventHandler OnDeath;
    public event EventHandler HitPauseStart;
    public event EventHandler HitPauseEnd;
    public event EventHandler DashStart;
    public event EventHandler DashEnd;

    //Health
    [Header("Health")]
    [SerializeField] private int maxHealth;
    private int health;
    private State state;

    //Attacks
    [Header("Attacks")]
    [SerializeField] private float attack1Shake;
    [SerializeField] private float attack1Pushback;
    [SerializeField] private float attack2Pushback;
    private float attackCooldown;
    private Coroutine attackCoroutine;
    private bool hitPaused = false;
    private bool hitPauseEnded = false;
    private Vector3 preHitPauseVelocity;
    private Coroutine hitPauseCoroutine = null;
    [SerializeField] private Transform attackSpawnPoint;
    [SerializeField] private AttackComboListSO attackComboListSO;
    private int currentAttackIndex = 0;
    private int currentAttackComboIndex = -1;
    private float attackComboTimer = 0f;

    //Blocking
    [Header("Block")]
    [SerializeField] private float damageReduction = 0.5f;
    [SerializeField] private float minBlockTime = 0.5f;
    [SerializeField] private float parryTime = 0.25f;
    private float parryFrameTime = 0;

    private bool isBlocking = false;
    Coroutine blockLiftCoroutine = null;
    //Movement
    [Header("Movement")]
    [SerializeField] private float playerSpeed;
    [SerializeField] private float moveSharpness;
    [SerializeField] private float dashDistance;
    [SerializeField] private float dashTime;
    [SerializeField] private float dashCooldownMax;
    private float dashCooldown = 0f;
    private float dashTimer = 0f;
    [SerializeField] private float drag;
    [SerializeField] private Vector3 gravityVector;
    [SerializeField] private KinematicCharacterMotor Motor;
    private Vector3 movementVector;
    private float playerToPointerAngle = 0f;
    private Vector3 addVelocity;
    //Rotation
    private Vector3 pointerPositionOnPlayerPlane;
    private Quaternion playerRotation;


    private void Awake() {
        Instance = this;
        health = maxHealth;
        state = State.Normal;
    }

    // Start is called before the first frame update
    void Start() {
        addVelocity = Vector3.zero;
        GameInput.Instance.Attack1Pressed += GameInput_Attack1Pressed;
        GameInput.Instance.Attack2Pressed += GameInput_Attack2Pressed;
        GameInput.Instance.BlockPressed += GameInput_BlockPressed;
        GameInput.Instance.DashPressed += GameInput_DashPressed;
        GameInput.Instance.BlockPressed += GameInput_BlockPressed;
        GameInput.Instance.BlockLifted += GameInput_BlockLifted;
        Motor.CharacterController = this;
    }




    #region Inputs
    private void GameInput_DashPressed(object sender, EventArgs e) {
        if (state != State.Dead) {
            Dash();
        }
    }
    private void GameInput_Attack1Pressed(object sender, System.EventArgs e) {
        if (attackCooldown <= 0 && state != State.Dead) {
            SetAttackIndexAndTimer(0);
            Attack1();
        }
    }
    private void GameInput_Attack2Pressed(object sender, System.EventArgs e) {

        if (attackCooldown <= 0 && state != State.Dead) {
            SetAttackIndexAndTimer(1);
            Attack2();
        }
    }

    #endregion

    #region Attacks
    public int GetCurrentAttackIndex() {
        return currentAttackIndex;
    }
    private void SetAttackIndexAndTimer(int attackComboIndex) {
        //if attackCooldown is done and player is alive
        if (currentAttackComboIndex == attackComboIndex && attackComboTimer > 0) {
            //If players last input was the same attack combo and the timer is still running
            currentAttackIndex++;
            currentAttackIndex %= attackComboListSO.attackCombos[attackComboIndex].attacks.Count;
        }
        else {
            //If the player is starting a new combo
            currentAttackIndex = 0;
            currentAttackComboIndex = attackComboIndex;
        }
        attackComboTimer = attackComboListSO.attackCombos[currentAttackComboIndex].resetTime;
        attackCooldown = attackComboListSO.attackCombos[currentAttackComboIndex].attacks[currentAttackIndex].attackCooldown;

    }
    private void Attack1() {
        isBlocking = false;
        BlockChanged?.Invoke(this, new BlockChangedArgs { isBlocking = isBlocking });
        Attack1Pressed?.Invoke(this, EventArgs.Empty);
        AddVelocity(transform.forward * attack1Pushback);
        CameraController.Instance.AddTrauma(attack1Shake);
        if (attackCoroutine == null) {
            attackCoroutine = StartCoroutine(AttackCoroutine(false, attackComboListSO.attackCombos[currentAttackComboIndex].attacks[currentAttackIndex].attackDelay));
        }
    }
    private void Attack2() {
        isBlocking = false;
        BlockChanged?.Invoke(this, new BlockChangedArgs { isBlocking = isBlocking });
        Attack2Pressed?.Invoke(this, EventArgs.Empty);
        AddVelocity(transform.forward * attack2Pushback);
        if (attackCoroutine == null) {
            attackCoroutine = StartCoroutine(AttackCoroutine(true, attackComboListSO.attackCombos[currentAttackComboIndex].attacks[currentAttackIndex].attackDelay));
        }
    }



    private void AttackCallback(bool isParent) {
        if (attackComboListSO.attackCombos[currentAttackComboIndex].attacks[currentAttackIndex].attackSpawnVFX != null) {
            Transform attackVFX;
            if (isParent) {
                attackVFX = Instantiate(attackComboListSO.attackCombos[currentAttackComboIndex].attacks[currentAttackIndex].attackSpawnVFX, attackSpawnPoint);
            }
            else {
                attackVFX = Instantiate(attackComboListSO.attackCombos[currentAttackComboIndex].attacks[currentAttackIndex].attackSpawnVFX, attackSpawnPoint.position, attackSpawnPoint.rotation);
            }
            StartCoroutine(DestroyCoroutine(attackVFX.gameObject, attackComboListSO.attackCombos[currentAttackComboIndex].attacks[currentAttackIndex].attackDuration));
        }
        Transform attack;
        if (isParent) {
            attack = Instantiate(attackComboListSO.attackCombos[currentAttackComboIndex].attacks[currentAttackIndex].attackPrefab, attackSpawnPoint);
        }
        else {
            attack = Instantiate(attackComboListSO.attackCombos[currentAttackComboIndex].attacks[currentAttackIndex].attackPrefab, attackSpawnPoint.position, attackSpawnPoint.rotation);
        }
        StartCoroutine(DestroyCoroutine(attack.gameObject, attackComboListSO.attackCombos[currentAttackComboIndex].attacks[currentAttackIndex].attackDuration));
    }

    IEnumerator AttackCoroutine(bool isParent, float delay) {
        yield return new WaitForSeconds(delay);
        AttackCallback(isParent);
        attackCoroutine = null;
    }

    IEnumerator DestroyCoroutine(GameObject obj, float duration) {
        float time = 0;
        while (time < duration) {
            if (!hitPaused)
                time += Time.deltaTime;
            yield return null;
        }
        if (obj != null)
            Destroy(obj);
    }
    #endregion


    #region Block
    private void GameInput_BlockPressed(object sender, EventArgs e) {
        if (attackCooldown <= 0 && !hitPaused) {
            isBlocking = true;
            parryFrameTime = 0;
            BlockChanged?.Invoke(this, new BlockChangedArgs { isBlocking = isBlocking });
        }
    }
    private void GameInput_BlockLifted(object sender, EventArgs e) {
        if (isBlocking) {
            if (blockLiftCoroutine == null) {
                StartCoroutine(BlockLiftCoroutine());
            }
        }
    }

    IEnumerator BlockLiftCoroutine() {
        yield return new WaitForSeconds(minBlockTime);
        isBlocking = false;
        BlockChanged?.Invoke(this, new BlockChangedArgs { isBlocking = isBlocking });
        blockLiftCoroutine = null;
    }

    #endregion

    #region HitPause

    public void HitPause(float pauseTime) {
        hitPaused = true;
        HitPauseStart?.Invoke(this, EventArgs.Empty);
        //Store all movement related variables
        if (hitPauseCoroutine != null) {
            StopCoroutine(hitPauseCoroutine);
        }
        hitPauseCoroutine = StartCoroutine(HitPauseCoroutine(pauseTime));
    }

    IEnumerator HitPauseCoroutine(float pauseTime) {
        yield return new WaitForSeconds(pauseTime);
        hitPaused = false;
        HitPauseEnd?.Invoke(this, EventArgs.Empty);
        hitPauseCoroutine = null;
        hitPauseEnded = true;
    }
    #endregion
    // Update is called once per frame
    void Update() {
        switch (state) {
            case State.Normal:
                UpdateAlive();
                break;
            case State.Dashing:
                UpdateDash();
                break;
            case State.Dead:
                UpdateDead();
                break;
        }
    }

    private void UpdateAlive() {
        DecrementCooldowns();
        ReorientPlayerRotationToPointer();
        ReorientMovementVectorToCamera();
    }

    private void UpdateDead() {
        movementVector = Vector3.zero;
    }
    private void UpdateDash() {
        DecrementCooldowns();
        ReorientPlayerRotationToPointer();
    }

    private void DecrementCooldowns() {
        //Handle dash cooldown and dash state switching
        if (dashCooldown > 0)
            dashCooldown -= Time.deltaTime;
        if (dashTimer > 0) {
            dashTimer -= Time.deltaTime;
        }
        //If dash timer has run out and we are in dashing state then go to idle state and end dash
        else if (state == State.Dashing) {
            DashEnd?.Invoke(this, EventArgs.Empty);
            state = State.Normal;
        }
        //Handle hitPause, do not decrement attack cooldowns if hit paused
        if (!hitPaused) {
            if (attackCooldown > 0) {
                attackCooldown -= Time.deltaTime;
            }
            if (attackComboTimer > 0) {
                attackComboTimer -= Time.deltaTime;
            }
        }
        if (isBlocking) {
            parryFrameTime += Time.deltaTime;
        }

    }

    void ReorientMovementVectorToCamera() {
        if (hitPaused) {
            movementVector = Vector3.zero;
            return;
        }
        Vector3 inputVector = GameInput.Instance.GetMovementVector();
        movementVector = inputVector.normalized;
        movementVector = new Vector3(movementVector.x, 0, movementVector.y);
        // Get the camera's forward and right vectors
        Vector3 forward = Camera.main.transform.forward;
        Vector3 right = Camera.main.transform.right;
        //Project the forward and right vectors onto the horizontal plane (y = 0)
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();
        // Reorient the movement vector to the camera
        movementVector = (forward * movementVector.z + right * movementVector.x);
        //Get angle between movement vector and the vector that points to the pointer
        Vector3 vectorToPointer = (pointerPositionOnPlayerPlane - transform.position).normalized;
        playerToPointerAngle = Vector3.SignedAngle(movementVector, vectorToPointer, Vector3.up);
    }

    void ReorientPlayerRotationToPointer() {
        if (hitPaused) {
            return;
        }
        Vector3 pointer = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(pointer);
        Plane playerPlane = new(Vector3.up, transform.position);
        if (playerPlane.Raycast(ray, out float enter)) {
            pointerPositionOnPlayerPlane = ray.GetPoint(enter);
        }
        Vector3 pointerPositionRelativeToPlayer = pointerPositionOnPlayerPlane - transform.position;
        playerRotation = Quaternion.LookRotation(pointerPositionRelativeToPlayer);
    }

    private void Dash() {
        if (dashCooldown > 0) {
            return;
        }
        DashStart?.Invoke(this, EventArgs.Empty);
        state = State.Dashing;
        dashTimer = dashTime;
        dashCooldown = dashCooldownMax;
        AddVelocity(movementVector * dashDistance);

    }

    #region KinematicCharacterController
    public void AfterCharacterUpdate(float deltaTime) {
    }

    public void BeforeCharacterUpdate(float deltaTime) {
    }

    public bool IsColliderValidForCollisions(Collider coll) {
        if (coll.CompareTag(Tags.PLAYER_ATTACK_TAG)
            || coll.CompareTag(Tags.ENEMY_ATTACK_TAG)
            || coll.CompareTag(Tags.PLAYER_TRIGGER_DETECTOR))
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
        addVelocity += vel;
    }
    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime) {
        currentRotation = playerRotation;
        currentRotation.x = 0;
        currentRotation.z = 0;
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime) {
        if (hitPauseEnded)
            currentVelocity = preHitPauseVelocity;
        Vector3 targetMovementVelocity = Vector3.zero;
        if (state == State.Normal) {
            if (Motor.GroundingStatus.IsStableOnGround) {
                // Reorient velocity on slope
                currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, Motor.GroundingStatus.GroundNormal) * currentVelocity.magnitude;
                // Calculate target velocity
                Vector3 inputRight = Vector3.Cross(movementVector, Motor.CharacterUp);
                Vector3 reorientedInput = Vector3.Cross(Motor.GroundingStatus.GroundNormal, inputRight).normalized * movementVector.magnitude;
                targetMovementVelocity = reorientedInput * playerSpeed;
                // Smooth movement Velocity
                currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1 - Mathf.Exp(-moveSharpness * deltaTime));
            }
            else {
                // Add move input
                if (movementVector.sqrMagnitude > 0f) {
                    targetMovementVelocity = movementVector * playerSpeed;
                    // Prevent climbing on un-stable slopes with air movement
                    if (Motor.GroundingStatus.FoundAnyGround) {
                        Vector3 perpenticularObstructionNormal = Vector3.Cross(Vector3.Cross(Motor.CharacterUp, Motor.GroundingStatus.GroundNormal), Motor.CharacterUp).normalized;
                        targetMovementVelocity = Vector3.ProjectOnPlane(targetMovementVelocity, perpenticularObstructionNormal);
                    }
                    Vector3 velocityDiff = Vector3.ProjectOnPlane(targetMovementVelocity - currentVelocity, gravityVector);
                    currentVelocity += playerSpeed * deltaTime * velocityDiff;
                }
                // Gravity
                currentVelocity += gravityVector * deltaTime;
                // Drag
                currentVelocity *= (1f / (1f + (drag * deltaTime)));
            }
            if (addVelocity.sqrMagnitude > 0f) {
                currentVelocity += addVelocity;
                addVelocity = Vector3.zero;
            }
        }
        else if (state == State.Dashing) {

            currentVelocity *= (1f / (1f + (drag * deltaTime)));
            if (addVelocity.sqrMagnitude > 0f) {
                currentVelocity += addVelocity;
                addVelocity = Vector3.zero;
            }

        }
        preHitPauseVelocity = currentVelocity;
    }



    public Vector3 GetPlayerMovementVectorRelativeToPointer() {
        if (movementVector == Vector3.zero) {
            return Vector3.zero;
        }
        else if (playerToPointerAngle > -45 && playerToPointerAngle < 45) {
            return Vector3.forward;
        }
        else if (playerToPointerAngle >= 45 && playerToPointerAngle < 135) {
            return Vector3.right;
        }
        else if (playerToPointerAngle <= -45 && playerToPointerAngle > -135) {
            return Vector3.left;
        }
        else {
            return Vector3.back;
        }
    }


    #endregion


    #region IHittable
    public void Hit(BaseAttack attack, Collision collision = null) {
        if (state == State.Dead) {
            return;
        }
        if (attack.GetTarget() == HittableType.Player) {
            CameraController.Instance.AddTrauma(0.3f);
            float damage = 0;
            //Determine if attack was in front of player, if collision is null assume attack was from behind
            bool inFront = false;
            if (collision != null) {
                Vector3 attackDirection = collision.GetContact(0).point - transform.position;
                attackDirection.Normalize();
                float dotProduct = Vector3.Dot(transform.forward, attackDirection); // 3D
                if (dotProduct > 0.5f) {
                    inFront = true;
                }
            }

            if (isBlocking && inFront) {
                if (parryFrameTime <= parryTime) {
                    damage = 0;
                }
                else {
                    damage = attack.GetDamage() * damageReduction;
                }
            }
            else {
                damage = attack.GetDamage();
            }
            health -= (int)damage;
            OnHit?.Invoke(this, EventArgs.Empty);
            if (health <= 0) {
                state = State.Dead;
                OnDeath?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public int GetHealth() {
        return health;
    }

    public int GetMaxHealth() {
        return maxHealth;
    }


    public HittableType GetHittableType() {
        return HittableType.Player;
    }
    #endregion


    public bool IsAlive() {
        return state == State.Normal;
    }


    public float GetNormalizedDashCooldown() {
        return dashCooldown / dashCooldownMax;
    }

    public float GetDashTime() {
        return dashTime;
    }


}
