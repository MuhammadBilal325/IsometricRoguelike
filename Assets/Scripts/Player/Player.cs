using Cinemachine.Utility;
using KinematicCharacterController;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class Player : MonoBehaviour, KinematicCharacterController.ICharacterController {

    public static Player Instance { get; private set; }

    [SerializeField] private Transform attackSpawnPoint;
    [SerializeField] private AttackListSO attackListSO;
    //Movement
    [SerializeField] private float playerSpeed;
    [SerializeField] private Vector3 gravityVector;
    [SerializeField] private KinematicCharacterMotor Motor;
    private Vector3 movementVector;
    private float playerToPointerAngle = 0f;

    //Rotation
    private Vector3 pointerPositionOnPlayerPlane;
    private Quaternion playerRotation;


    private void Awake() {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start() {
        GameInput.Instance.Attack1Pressed += GameInput_Attack1Pressed;
        Motor.CharacterController = this;
    }
    #region Inputs
    private void GameInput_Attack1Pressed(object sender, System.EventArgs e) {
        Attack1();
    }
    #endregion

    #region Attacks

    private void Attack1() {
        Instantiate(attackListSO.attackList[0].attackPrefab, attackSpawnPoint.transform.position, attackSpawnPoint.rotation);
    }

    #endregion


    // Update is called once per frame
    void Update() {
        ReorientPlayerRotationToPointer();
        ReorientMovementVectorToCamera();
    }

    void ReorientMovementVectorToCamera() {
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
        Vector3 pointer = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(pointer);
        Plane playerPlane = new Plane(Vector3.up, transform.position);
        if (playerPlane.Raycast(ray, out float enter)) {
            pointerPositionOnPlayerPlane = ray.GetPoint(enter);
        }
        Vector3 pointerPositionRelativeToPlayer = pointerPositionOnPlayerPlane - transform.position;
        playerRotation = Quaternion.LookRotation(pointerPositionRelativeToPlayer);
    }


    #region KinematicCharacterController
    public void AfterCharacterUpdate(float deltaTime) {
    }

    public void BeforeCharacterUpdate(float deltaTime) {
    }

    public bool IsColliderValidForCollisions(Collider coll) {
        if (coll.CompareTag(Tags.PLAYER_ATTACK_TAG))
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

    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime) {
        currentRotation = playerRotation;
        currentRotation.x = 0;
        currentRotation.z = 0;
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime) {
        currentVelocity.x = 0;
        currentVelocity.z = 0;
        //Horizontal Movement
        currentVelocity += movementVector * playerSpeed * deltaTime;
        //Gravity
        currentVelocity += gravityVector * deltaTime;
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

}
