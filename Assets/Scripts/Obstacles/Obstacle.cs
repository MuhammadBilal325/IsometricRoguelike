using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour, IHittable {
    public int GetHealth() {
        return 10000;
    }

    public int GetMaxHealth() {
        return 10000;
    }

    public void Hit(BaseAttack attack) {
        Debug.Log("Attack " + attack.GetName() + " hit obstacle " + this.name);
    }

    HittableType IHittable.GetType() {
        return HittableType.Environment;
    }

}
