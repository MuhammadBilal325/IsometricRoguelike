using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseDeathExplosionAttack : BaseAttack {

    [SerializeField] private SphereCollider sphereCollider;
    [SerializeField] private AnimationCurve scaleCurve;
    [SerializeField] private float scaleMultiplier = 1f;
    private float t = 0;


    // Update is called once per frame
    void FixedUpdate() {
        if (t > attackData.attackDuration) {
            Destroy(gameObject);
        }
        t += Time.deltaTime;
        float lerpValue = t / attackData.attackDuration;
        sphereCollider.radius = (scaleCurve.Evaluate(lerpValue) * scaleMultiplier) / 2;
    }

    private void OnCollisionEnter(Collision collision) {
        IHittable hitObject = collision.gameObject.GetComponent<IHittable>();
        if (hitObject != null) {
            base.OnHitting(hitObject);
        }
    }
    public float GetExplosionDuration() {
        return attackData.attackDuration;
    }
    public AnimationCurve GetExplosionSizeCurve() {
        return scaleCurve;
    }
    public float GetExplosionScaleMultiplier() {
        return scaleMultiplier;
    }
}
