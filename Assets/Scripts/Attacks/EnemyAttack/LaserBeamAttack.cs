using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBeamAttack : BaseAttack {

    [SerializeField] private Transform beamSpawnPoint;
    [SerializeField] private BoxCollider beamCollider;
    private float spawnTime = 0f;
    [SerializeField] private float maxRange;
    [SerializeField] LayerMask raycastMask;
    private void Awake() {
        //Do a raycast for the range and set visual range to the distance of the raycast hit
        RaycastHit hit;
        float finalScaleY = maxRange / 2;
        if (Physics.Raycast(transform.position, transform.forward, out hit, float.PositiveInfinity, raycastMask)) {
            finalScaleY = (hit.distance) / 2;
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


    private void OnCollisionEnter(Collision collision) {
        IHittable hitObject = collision.gameObject.GetComponent<IHittable>();
        if (hitObject != null) {
            OnHitting(hitObject, collision);
        }
    }

    public float GetDuration() {
        return attackData.attackDuration;
    }


}
