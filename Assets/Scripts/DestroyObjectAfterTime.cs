using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyObjectAfterTime : MonoBehaviour {
    [SerializeField] private float timeToDestroy = 1f;
    void Start() {
        StartCoroutine(DestroyAfterTime());
    }

    IEnumerator DestroyAfterTime() {
        yield return new WaitForSeconds(timeToDestroy);
        Destroy(gameObject);
    }
}
