using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Attack1 : BaseAttack {
    [SerializeField] private float attackSpeed;
    private Vector3 spawnPoint;
    void Start() {
        spawnPoint = transform.position;

    }

    void Update() {
        transform.Translate(attackSpeed * Time.deltaTime * Vector3.forward);    
    }

    public override void OnHit(IHittable hitObject) {
        if (hitObject.GetHittableType() == hittableTarget || hitObject.GetHittableType() == HittableType.Environment) {
            hitObject.Hit(this);
            Destroy(this.gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.TryGetComponent<IHittable>(out IHittable hitObject)) {
            if (hitObject != null) {
                OnHit(hitObject);
            }
        }

    }
}

