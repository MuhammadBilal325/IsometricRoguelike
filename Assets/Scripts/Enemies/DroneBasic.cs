using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneBasic : BaseEnemy {

    private enum State {
        Idle,
        Attack,
    }
    [SerializeField] private float idleRange;
    [SerializeField] private float attackRange;
    [SerializeField] private float speed;


    public override int GetHealth() {
        return health;
    }

    public override void Hit(BaseAttack attack) {
        health -= attack.GetDamage();
        base.InvokeHitEvent();
        if (health <= 0) {
            Destroy(this.gameObject);
        }
    }
    // Start is called before the first frame update
    private void Start() {

    }

    // Update is called once per frame
    private void Update() {

    }
}
