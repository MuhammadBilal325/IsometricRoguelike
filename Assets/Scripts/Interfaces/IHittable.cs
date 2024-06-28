using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HittableType {
    Player,
    Enemy,
    Environment
}
public interface IHittable {

    void Hit(BaseAttack attack);
    public int GetHealth();

    public int GetMaxHealth();
    public HittableType GetType();
}
