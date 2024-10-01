using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectController : MonoBehaviour {

    public enum EffectType {
        Particles,
    }


    public static EffectController Instance { get; private set; }
    [SerializeField] private Transform particleVFX;



    void Awake() {
        if (Instance == null) {
            Instance = this;
        }
        else {
            Destroy(gameObject);
        }
    }

    public void SpawnEffect(EffectType effect, Vector3 position, Quaternion? rot = null) {
        Quaternion rotation = rot ?? Quaternion.identity;
        Transform particleTransform = null;
        switch (effect) {
            case EffectType.Particles:
                particleTransform = particleVFX;
                break;
            default:
                break;
        }
        if (particleTransform != null) {
            Instantiate(particleTransform, position, rotation);
        }
    }
}
