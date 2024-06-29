using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HittableType {
    Player,
    Enemy,
    Environment
}
public interface IHittable {

    public event EventHandler OnHit;
    void Hit(BaseAttack attack);
    public int GetHealth();

    public int GetMaxHealth();
    public HittableType GetHittableType();
}
