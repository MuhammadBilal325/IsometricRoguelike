using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    public static CameraController Instance { get; private set; }
    [SerializeField] private Transform player;
    [SerializeField] private float safeAreaX;
    [SerializeField] private float safeAreaY;
    [SerializeField] private float cameraFollowSpeed;
    private Vector3 playerOffset;

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
        if (!IsPlayerInSafeArea()) {
            MoveCamera();
        }
    }
    private void MoveCamera() {
        //Move camera to put player in centre again
        transform.position = Vector3.Lerp(transform.position, player.position + playerOffset, Time.deltaTime * cameraFollowSpeed);
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
        Vector2 screenPos = Camera.main.WorldToScreenPoint(player.position);
        screenPos.x -= Screen.width / 2;
        screenPos.y -= Screen.height / 2;
        if (screenPos.x < -safeAreaX || screenPos.x > safeAreaX || screenPos.y < -safeAreaY || screenPos.y > safeAreaY) {
            return false;
        }
        return true;
    }

}
