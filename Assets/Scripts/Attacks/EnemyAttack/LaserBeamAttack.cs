using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBeamAttack : BaseAttack {

    [SerializeField] private float beamDuration;
    [SerializeField] private float beamFadePeriod;
    private float spawnTime = 0f;
    private bool canDamage = true;
    void Start() {
    }

    void Update() {
        spawnTime += Time.deltaTime;
        if (spawnTime < beamDuration) {
            if (spawnTime > beamDuration - beamFadePeriod) {
                canDamage = false;
            }
        }
        else {
            Destroy(this.gameObject);
        }
    }

    public override void OnHit(IHittable hitObject) {
        if (hitObject.GetHittableType() == hittableTarget || hitObject.GetHittableType() == HittableType.Environment) {
            hitObject.Hit(this);
        }
    }

    private void OnCollisionEnter(Collision collision) {
        if (canDamage) {
            IHittable hitObject = collision.gameObject.GetComponent<IHittable>();
            if (hitObject != null) {
                OnHit(hitObject);
            }
        }
    }

    public float GetDuration() {
        return beamDuration;
    }


}
