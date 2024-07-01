using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneBasicVisual : MonoBehaviour {


    [SerializeField] private DroneBasic droneScript;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private float blinkingFrequency = 10f;
    private readonly string EMISSION_MULTIPLY = "_EmissionMultiply";
    private float originalEmission = 0f;
    private float blinkingTimer = 0f;
    private float blinkingTimerMax = 0f;
    private bool isBlinking = false;
    private float progress = 0f;
    private float blink = 0f;
    private void Start() {
        droneScript.StartAttack += DroneScript_StartAttack;
        originalEmission = meshRenderer.material.GetFloat(EMISSION_MULTIPLY);
    }

    private void DroneScript_StartAttack(object sender, DroneBasic.AttackEventArgs e) {
        isBlinking = true;
        blinkingTimer = 0f;
        blinkingTimerMax = e.attackWarmUpTime;
    }

    private void Update() {
        if (isBlinking) {
            HandleAttackBlinking();
        }
    }

    private void HandleAttackBlinking() {
        blinkingTimer += Time.deltaTime;
        if (blinkingTimer >= blinkingTimerMax) {
            isBlinking = false;
            meshRenderer.material.SetFloat(EMISSION_MULTIPLY, 0f);
        }
        progress = blinkingTimer / blinkingTimerMax;

        blink = (1 + Mathf.Sin(2 * Mathf.PI * progress * progress * blinkingFrequency)) / 2;
        blink = Mathf.Round(blink);
        if (blink == 1) {
            meshRenderer.material.SetFloat(EMISSION_MULTIPLY, originalEmission);
        }
        else {
            meshRenderer.material.SetFloat(EMISSION_MULTIPLY, 0f);
        }

    }
}
