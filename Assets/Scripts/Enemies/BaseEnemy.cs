using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseEnemy : MonoBehaviour, IHittable {

    public event EventHandler OnHit;

    [SerializeField] protected int maxHealth;
    protected int health;
    public virtual int GetHealth() {
        return health;
    }

    public virtual int GetMaxHealth() {
        return maxHealth;
    }

    protected virtual void InvokeHitEvent() {
        OnHit?.Invoke(this, EventArgs.Empty);
    }
    public virtual void Hit(BaseAttack attack) {
        Debug.LogError("BaseEnemy " + this.name + " Hit by attack" + attack);
    }

    public virtual HittableType GetHittableType() {
        return HittableType.Enemy;
    }
    protected virtual void Awake() {
        health = maxHealth;
    }

}
