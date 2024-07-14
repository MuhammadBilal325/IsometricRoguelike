using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack2 : BaseAttack {

    [SerializeField] private float hitPauseTime = 0.1f;
    [SerializeField] private float trauma = 0.3f;
    private bool hitPaused = false;
    public override void OnHit(IHittable hitObject) {
        if (hitObject.GetHittableType() == hittableTarget || hitObject.GetHittableType() == HittableType.Environment) {
            hitObject.Hit(this);
            if (!hitPaused) {
                Player.Instance.HitPause(hitPauseTime);
                CameraController.Instance.AddTrauma(trauma);
                hitPaused = true;
            }
        }
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.TryGetComponent<IHittable>(out IHittable hitObject)) {
            if (hitObject != null) {
                OnHit(hitObject);
            }
        }
    }
}
