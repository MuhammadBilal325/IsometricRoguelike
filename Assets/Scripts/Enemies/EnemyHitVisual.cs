using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHitVisual : MonoBehaviour {
    [SerializeField] private BaseEnemy baseEnemy;
    [SerializeField] private float flashSpeed;
    [SerializeField, ColorUsage(hdr: true, showAlpha: true)] private Color flashColor;
    private MeshRenderer meshRenderer;
    private readonly string EMISSION_OVERRIDE = "_EmissionOverride";
    private readonly string EMISSION_MIX = "_EmissionMix";
    private float flashLerp = 1f;
    private bool isFlashing = false;
    // Start is called before the first frame update
    void Start() {
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material.SetColor(EMISSION_OVERRIDE, flashColor);
        baseEnemy.OnHit += BaseEnemy_OnHit;
    }

    private void BaseEnemy_OnHit(object sender, System.EventArgs e) {
        flashLerp = 0f;
        isFlashing = true;
    }

    private void Update() {
        if (flashLerp < 1f) {
            flashLerp += Time.deltaTime * flashSpeed;
            meshRenderer.material.SetFloat(EMISSION_MIX, flashLerp);
        }
        else if (isFlashing) {
            isFlashing = false;
            meshRenderer.material.SetFloat(EMISSION_MIX, 1f);
        }
    }
}
