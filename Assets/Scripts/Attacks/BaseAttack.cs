using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseAttack : MonoBehaviour {
    [SerializeField] protected HittableType hittableTarget;
    [SerializeField] protected AttackSO attackData;
    [SerializeField] protected int damage;

    public virtual void OnHit(IHittable hitObject) {
        if (hitObject.GetHittableType() == hittableTarget || hitObject.GetHittableType() == HittableType.Environment) {
            hitObject.Hit(this);
        }
    }

    public virtual int GetDamage() {
        return damage;
    }

    public virtual HittableType GetTarget() {
        return hittableTarget;
    }

    public virtual string GetName() {
        return attackData.attackName;
    }
}
