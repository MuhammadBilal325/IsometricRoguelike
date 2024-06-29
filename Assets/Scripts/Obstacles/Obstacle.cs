using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour, IHittable {

    public event EventHandler OnHit;
    public int GetHealth() {
        return 10000;
    }

    public int GetMaxHealth() {
        return 10000;
    }

    public void Hit(BaseAttack attack) {
        OnHit?.Invoke(this, EventArgs.Empty);
        Debug.Log("Attack " + attack.GetName() + " hit obstacle " + this.name);
    }

    HittableType IHittable.GetHittableType() {
        return HittableType.Environment;
    }

}
