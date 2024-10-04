using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseDeathExplosionVisual : MonoBehaviour {


    [SerializeField] private MouseDeathExplosionAttack attackScript;
    private readonly string MATERIAL_POWER = "_Power";
    [SerializeField] private MeshRenderer[] explosionRenderers;
    [SerializeField] private AnimationCurve visualCurve;
    private AnimationCurve scaleCurve;
    private float scaleMultiplier = 1f;
    private float t = 0;
    private float duration = 0f;
    void Awake() {
        scaleCurve = attackScript.GetExplosionSizeCurve();
        scaleMultiplier = attackScript.GetExplosionScaleMultiplier();
        duration = attackScript.GetExplosionDuration();
    }

    // Update is called once per frame
    void FixedUpdate() {
        t += Time.deltaTime;
        float lerpVal = t / duration;
        float scaleVal = scaleCurve.Evaluate(lerpVal) * scaleMultiplier;
        transform.localScale = new Vector3(scaleVal, scaleVal, scaleVal);
        foreach (MeshRenderer mouseRenderer in explosionRenderers)
            mouseRenderer.material.SetFloat(MATERIAL_POWER, visualCurve.Evaluate(t));
    }
}
