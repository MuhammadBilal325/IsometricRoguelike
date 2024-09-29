using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBeamAttack : BaseAttack {

    [SerializeField] private Transform beamSpawnPoint;
    [SerializeField] private BoxCollider beamCollider;
    private float spawnTime = 0f;
    [SerializeField] private float maxRange;
    [SerializeField] LayerMask mask;
    private void Awake() {
        //Do a raycast for the range and set visual range to the distance of the raycast hit
        RaycastHit hit;
        float finalScaleY = maxRange / 2;
        if (Physics.Raycast(transform.position, transform.forward, out hit, mask)) {
            finalScaleY = hit.distance / 2;
        }
        beamSpawnPoint.localScale = new Vector3(
            beamSpawnPoint.localScale.x,
            beamSpawnPoint.localScale.y,
            finalScaleY);
        beamCollider.center = new Vector3(
            beamCollider.center.x,
            beamCollider.center.y,
            finalScaleY);
        beamCollider.size = new Vector3(
            beamCollider.size.x,
            beamCollider.size.y,
            finalScaleY * 2f);
    }

    void Update() {
        spawnTime += Time.deltaTime;
        if (spawnTime >= attackData.attackDuration)
            Destroy(this.gameObject);
    }


    public override void OnHit(IHittable hitObject) {
        if (hitObject.GetHittableType() == hittableTarget || hitObject.GetHittableType() == HittableType.Environment) {
            hitObject.Hit(this);
        }
    }

    private void OnCollisionEnter(Collision collision) {
        IHittable hitObject = collision.gameObject.GetComponent<IHittable>();
        if (hitObject != null) {
            OnHit(hitObject);
        }
    }

    public float GetDuration() {
        return attackData.attackDuration;
    }


}
