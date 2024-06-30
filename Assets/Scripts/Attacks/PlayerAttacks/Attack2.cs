using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack2 : BaseAttack {



    public override void OnHit(IHittable hitObject) {
        if (hitObject.GetHittableType() == hittableTarget || hitObject.GetHittableType() == HittableType.Environment) {
            hitObject.Hit(this);
        }
    }

    private void OnCollisionEnter(Collision collision) {
        IHittable hitObject = collision.gameObject.GetComponent<IHittable>();
        if (hitObject != null) {
            OnHit(hitObject);
        }
    }
}
