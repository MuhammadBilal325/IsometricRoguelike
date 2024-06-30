using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBeamVisual : MonoBehaviour {
    [SerializeField] private LaserBeamAttack laserBeamAttack;
    [SerializeField] private MeshRenderer meshRenderer;
    private Material laserMaterial;
    private float timeElapsed = 0f;
    private float timeToFade;
    // Start is called before the first frame update
    void Start() {
        timeToFade = laserBeamAttack.GetDuration();
        laserMaterial = meshRenderer.material;

    }

    // Update is called once per frame
    void Update() {
        timeElapsed += Time.deltaTime;
        float alpha = timeElapsed / timeToFade;
        laserMaterial.SetFloat("_Fade", alpha);
    }
}
