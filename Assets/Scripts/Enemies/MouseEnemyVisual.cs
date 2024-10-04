using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseEnemyVisual : MonoBehaviour {
    [SerializeField] private SkinnedMeshRenderer mouseRenderer;
    [SerializeField] private MouseEnemy mouseScript;
    [SerializeField] private float blinkingFrequency = 10f;
    private readonly string EMISSION_MULTIPLY = "_EmissionMultiply";
    private float originalEmission = 0f;
    private float beepingTimer = 0f;
    private float beepingTimerMax = 0f;
    private bool isBeeping = false;
    void Start() {
        mouseScript.StartBeeping += MouseScript_StartBeeping;
        originalEmission = mouseRenderer.material.GetFloat(EMISSION_MULTIPLY);
    }

    private void MouseScript_StartBeeping(object sender, MouseEnemy.BeepingEventArgs e) {
        beepingTimerMax = e.beepingWarmUpTime;
        isBeeping = true;
    }

    // Update is called once per frame
    void Update() {
        if (isBeeping) {
            HandleExplosionBeeping();
        }

    }

    private void HandleExplosionBeeping() {
        beepingTimer += Time.deltaTime;
        if (beepingTimer >= beepingTimerMax) {
            isBeeping = false;
            mouseRenderer.material.SetFloat(EMISSION_MULTIPLY, 0f);
        }
        float progress = beepingTimer / beepingTimerMax;
        float emissionIntensity = 15f;
        float blink = (1 + Mathf.Sin(2 * Mathf.PI * progress * progress * blinkingFrequency)) * emissionIntensity;
        mouseRenderer.material.SetFloat(EMISSION_MULTIPLY, blink);

    }
}
