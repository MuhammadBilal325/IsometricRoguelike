using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHitVisual : MonoBehaviour {
    [SerializeField] private BaseEnemy baseEnemy;
    [SerializeField] private float flashSpeed;
    [SerializeField, ColorUsage(hdr: true, showAlpha: true)] private Color flashColor;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer;
    private readonly string EMISSION_OVERRIDE = "_EmissionOverride";
    private readonly string EMISSION_MIX = "_EmissionMix";
    private float flashLerp = 1f;
    private bool isFlashing = false;
    // Start is called before the first frame update
    void Start() {
        if (meshRenderer != null) {
            meshRenderer.material.SetColor(EMISSION_OVERRIDE, flashColor);
        }
        if (skinnedMeshRenderer != null) {
            skinnedMeshRenderer.material.SetColor(EMISSION_OVERRIDE, flashColor);
        }
        baseEnemy.OnHit += BaseEnemy_OnHit;
    }

    private void BaseEnemy_OnHit(object sender, System.EventArgs e) {
        flashLerp = 0f;
        isFlashing = true;
    }

    private void Update() {
        if (flashLerp < 1f) {
            flashLerp += Time.deltaTime * flashSpeed;
            if (meshRenderer != null) {
                meshRenderer.material.SetFloat(EMISSION_MIX, 1f - flashLerp);
            }
            if (skinnedMeshRenderer != null) {
                skinnedMeshRenderer.material.SetFloat(EMISSION_MIX, 1f - flashLerp);
            }
        }
        else if (isFlashing) {
            isFlashing = false;
            if (meshRenderer != null) {
                meshRenderer.material.SetFloat(EMISSION_MIX, 0);
            }
            if (skinnedMeshRenderer != null) {
                skinnedMeshRenderer.material.SetFloat(EMISSION_MIX, 0);
            }
        }
    }
}
