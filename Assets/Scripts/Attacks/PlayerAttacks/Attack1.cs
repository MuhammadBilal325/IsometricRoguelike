using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Attack1 : BaseAttack {
    [SerializeField] private float attackSpeed;
    [SerializeField] private float attackRange;
    private Vector3 spawnPoint;
    void Start() {
        spawnPoint = transform.position;

    }

    void Update() {
        transform.Translate(attackSpeed * Time.deltaTime * Vector3.forward);
        if (Vector3.SqrMagnitude(spawnPoint - transform.position) > attackRange * attackRange) {
            Destroy(this.gameObject);
        }
    }

    public override void OnHit(IHittable hitObject) {
        if (hitObject.GetType() == hittableTarget || hitObject.GetType() == HittableType.Environment) {
            hitObject.Hit(this);
            Destroy(this.gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision) {
        IHittable hitObject = collision.gameObject.GetComponent<IHittable>();
        if (hitObject != null) {
            OnHit(hitObject);
        }
    }
}

