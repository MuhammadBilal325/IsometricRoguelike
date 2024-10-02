using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseHitbox : BaseAttack {

    private void OnCollisionEnter(Collision collision) {
        IHittable hitObject = collision.gameObject.GetComponent<IHittable>();
        if (hitObject != null) {
            OnHitting(hitObject, collision);
        }
    }

}
