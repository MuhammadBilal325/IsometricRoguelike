using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    public static CameraController Instance { get; private set; }
    [SerializeField] private Transform player;
    [SerializeField] private Transform shakeEmpty;
    private Vector2 quadrantBottomLeft;
    private Vector2 quadrantTopRight;
    private Vector2 quadrantMiddle;
    private Vector3 quadrantMiddleOnPlayerPlane;

    private float MinInfinity = Mathf.NegativeInfinity;
    private float MaxInfinity = Mathf.Infinity;
    [SerializeField] private float cameraFollowSpeed;
    [SerializeField] private float maxYaw;
    [SerializeField] private float maxPitch;
    [SerializeField] private float maxRoll;
    [SerializeField] private float maxOffsetX;
    [SerializeField] private float maxOffsetY;
    [SerializeField] private int noiseSeed;
    [SerializeField] private float traumaDecreaseFactor;
    [SerializeField] private float noiseFrequency;
    [SerializeField] private float shakeSnapSpeed;
    [SerializeField] private float shakeMagnitude;
    private float trauma = 0f;
    private float shake;
    private float yaw;
    private float pitch;
    private float roll;
    private float shakeOffsetX;
    private float shakeOffsetY;
    private Vector3 playerOffset;
    private Vector3 offsetVector;

    private void Awake() {
        if (Instance != null) {
            Debug.LogError("There is more than one CameraController in the scene");
        }
        else {
            Instance = this;
        }
    }
    void Start() {
        MoveCameraToCentrePlayer();
        playerOffset = transform.position;
    }

    // Update is called once per frame
    private void LateUpdate() {
        CalculateSafeArea();
        if (!IsPlayerInSafeArea()) {
            MoveCamera();
        }
        HandleCameraShake();
    }
    private void MoveCamera() {
        // Convert the player's world position to screen position
        Ray ray = Camera.main.ScreenPointToRay(quadrantMiddle);
        Plane playerPlane = new Plane(Vector3.up, player.transform.position);
        Vector3 adjustmentVector = Vector3.zero;
        if (playerPlane.Raycast(ray, out float enter)) {
            quadrantMiddleOnPlayerPlane = ray.GetPoint(enter);
            adjustmentVector = player.transform.position - quadrantMiddleOnPlayerPlane;
            transform.position = Vector3.Lerp(transform.position, adjustmentVector, cameraFollowSpeed * Time.deltaTime);
        }
    }

    private void MoveCameraToCentrePlayer() {
        Vector2 pointer = new Vector2(Screen.width / 2, Screen.height / 2);
        Ray ray = Camera.main.ScreenPointToRay(pointer);
        Plane playerPlane = new Plane(Vector3.up, player.transform.position);
        if (playerPlane.Raycast(ray, out float enter)) {
            Vector3 cameraCentrePoint = ray.GetPoint(enter);
            Vector3 newPos = player.transform.position - cameraCentrePoint;
            transform.position += new Vector3(newPos.x, 0, newPos.z);
        }
    }
    bool IsPlayerInSafeArea() {
        Vector2 playerPosition = Camera.main.WorldToScreenPoint(player.position);
        if (playerPosition.x >= quadrantBottomLeft.x
            && playerPosition.x <= quadrantTopRight.x
            && playerPosition.y >= quadrantBottomLeft.y
            && playerPosition.y <= quadrantTopRight.y) {
            return true;
        }
        return false;
    }

    private void CalculateSafeArea() {
        Vector2 pointerPos = Input.mousePosition;
        int screenWidthMiddle = Screen.width / 2;
        int screenHeightMiddle = Screen.height / 2;
        if (pointerPos.x < screenHeightMiddle && pointerPos.y < screenHeightMiddle) {
            quadrantBottomLeft = new Vector2(screenWidthMiddle, screenHeightMiddle);
            quadrantTopRight = new Vector2(MaxInfinity, MaxInfinity);
            quadrantMiddle = new Vector2((screenWidthMiddle + Screen.width) / 2f, (screenHeightMiddle + Screen.height) / 2f);
        }
        else if (pointerPos.x < screenWidthMiddle && pointerPos.y > screenHeightMiddle) {
            quadrantBottomLeft = new Vector2(screenWidthMiddle, MinInfinity);
            quadrantTopRight = new Vector2(MaxInfinity, screenHeightMiddle);
            quadrantMiddle = new Vector2((screenWidthMiddle + Screen.width) / 2f, screenHeightMiddle / 2f);
        }
        else if (pointerPos.x > screenWidthMiddle && pointerPos.y < screenHeightMiddle) {
            quadrantBottomLeft = new Vector2(MinInfinity, screenHeightMiddle);
            quadrantTopRight = new Vector2(screenWidthMiddle, MaxInfinity);
            quadrantMiddle = new Vector2(screenWidthMiddle / 2f, (screenHeightMiddle + Screen.height) / 2f);
        }
        else if (pointerPos.x > screenWidthMiddle && pointerPos.y > screenHeightMiddle) {
            quadrantBottomLeft = new Vector2(MinInfinity, MinInfinity);
            quadrantTopRight = new Vector2(screenWidthMiddle, screenHeightMiddle);
            quadrantMiddle = new Vector2(screenWidthMiddle / 2f, screenHeightMiddle / 2f);
        }
    }

    private void HandleCameraShake() {
        shake = trauma * trauma;
        CameraShakeRotation();
        CameraShakeLocation();
        trauma -= traumaDecreaseFactor * Time.deltaTime;
        trauma = Mathf.Clamp(trauma, 0, 1);
    }
    private void CameraShakeRotation() {
        yaw = maxYaw * shake * Mathf.Lerp(-1, 1, Mathf.PerlinNoise(noiseSeed, Time.realtimeSinceStartup * noiseFrequency));
        pitch = maxPitch * shake * Mathf.Lerp(-1, 1, Mathf.PerlinNoise(noiseSeed + 1, Time.realtimeSinceStartup * noiseFrequency));
        roll = maxRoll * shake * Mathf.Lerp(-1, 1, Mathf.PerlinNoise(noiseSeed + 2, Time.realtimeSinceStartup * noiseFrequency));
        Vector3 shakeRotationVector = new Vector3(pitch, yaw, roll);
        shakeRotationVector *= shakeMagnitude;
        Quaternion shakeRotation = Quaternion.Euler(shakeRotationVector);

        shakeEmpty.localRotation = Quaternion.Lerp(shakeEmpty.localRotation, shakeRotation, Time.fixedDeltaTime * shakeSnapSpeed);
    }
    private void CameraShakeLocation() {
        shakeOffsetX = Mathf.Lerp(-1, 1, Mathf.PerlinNoise(noiseSeed + 3, Time.realtimeSinceStartup * noiseFrequency));
        shakeOffsetY = Mathf.Lerp(-1, 1, Mathf.PerlinNoise(noiseSeed + 4, Time.realtimeSinceStartup * noiseFrequency));
        offsetVector = new Vector3(shakeOffsetX, shakeOffsetY, shakeEmpty.localPosition.z);
        offsetVector = offsetVector.normalized;
        offsetVector *= shake;
        offsetVector.x *= maxOffsetX;
        offsetVector.y *= maxOffsetY;
        offsetVector *= shakeMagnitude;
        shakeEmpty.localPosition = Vector3.Lerp(shakeEmpty.localPosition, offsetVector, Time.deltaTime * shakeSnapSpeed);
    }

    public void AddTrauma(float t) {

        trauma += t;
    }

}
