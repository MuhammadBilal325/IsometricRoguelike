using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseAttack : MonoBehaviour {
    [SerializeField] protected HittableType hittableTarget;
    [SerializeField] protected AttackSO attackData;

    public virtual void OnHitting(IHittable hitObject, Collision collision = null) {
        if (hitObject.GetHittableType() == hittableTarget || hitObject.GetHittableType() == HittableType.Environment) {
            hitObject.Hit(this, collision);
        }
    }

    public virtual int GetDamage() {
        return attackData.attackDamage;
    }

    public virtual HittableType GetTarget() {
        return hittableTarget;
    }

    public virtual string GetName() {
        return attackData.attackName;
    }
}
