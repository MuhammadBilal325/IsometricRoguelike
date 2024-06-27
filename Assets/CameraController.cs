using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    public static CameraController Instance { get; private set; }
    private void Awake() {
        if (Instance != null) {
            Debug.LogError("There is more than one CameraController in the scene");
        }
        else {
            Instance = this;
        }
    }
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

}
