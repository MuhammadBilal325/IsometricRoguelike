using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseEnemyVisual : MonoBehaviour {
    [SerializeField] private SkinnedMeshRenderer mouseRenderer;
    [SerializeField] private Animator mouseAnimator;
    [SerializeField] private MouseEnemy mouseScript;
    [SerializeField] private float blinkingFrequency = 10f;
    private readonly string EMISSION_MULTIPLY = "_EmissionMultiply";
    private readonly string EAR_STATE = "EarState";
    private readonly string TAIL_SPEED = "TailSpeed";
    private int earState = 0;
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
        earState = 1;
        mouseAnimator.SetInteger(EAR_STATE, earState);
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
            earState = 2;
            mouseAnimator.SetInteger(EAR_STATE, earState);
            mouseAnimator.SetFloat(TAIL_SPEED, 0f);
            mouseRenderer.material.SetFloat(EMISSION_MULTIPLY, 0f);
            return;
        }
        float progress = beepingTimer / beepingTimerMax;
        float emissionIntensity = 15f;
        float blink = (1 + Mathf.Sin(2 * Mathf.PI * progress * progress * blinkingFrequency)) * emissionIntensity;
        mouseRenderer.material.SetFloat(EMISSION_MULTIPLY, blink);

    }
}
