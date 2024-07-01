using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshTrail : MonoBehaviour {

    private class trailObject {
        public GameObject[] gObj;
        public MeshRenderer[] mr;
        public MeshFilter[] mf;
        public Mesh[] mesh;

        public trailObject(int length) {
            gObj = new GameObject[length];
            mr = new MeshRenderer[length];
            mf = new MeshFilter[length];
            mesh = new Mesh[length];
        }
    }
    [SerializeField] private bool activateTrail;
    [SerializeField] private float meshRefreshRate = 0.1f;
    [SerializeField] private float meshDestroyDelay = 0.3f;
    [SerializeField] private Material trailMaterial;
    [SerializeField] private string shaderVarRef;
    [SerializeField] private float shaderFadeOutRate;
    [SerializeField] private float shaderFadeOutRefreshRate;
    private bool isTrailActive = false;
    private int trailObjectsAmount;
    //Object pooling
    private trailObject[] trailObjects;
    private SkinnedMeshRenderer[] skinnedMeshRenderers;
    void Start() {
        Player.Instance.DashStart += Player_DashStart;
        Player.Instance.DashEnd += Player_DashEnd;
        InitializeObjectPool();
    }

    private void InitializeObjectPool() {
        trailObjectsAmount = Mathf.RoundToInt(Player.Instance.GetDashTime() / meshRefreshRate);
        skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        trailObjects = new trailObject[trailObjectsAmount];
        for (int i = 0; i < trailObjectsAmount; i++) {
            trailObjects[i] = new trailObject(skinnedMeshRenderers.Length);
            for (int j = 0; j < skinnedMeshRenderers.Length; j++) {
                trailObjects[i].gObj[j] = new GameObject();
                trailObjects[i].mr[j] = trailObjects[i].gObj[j].AddComponent<MeshRenderer>();
                trailObjects[i].mf[j] = trailObjects[i].gObj[j].AddComponent<MeshFilter>();
                trailObjects[i].mesh[j] = new Mesh();
                trailObjects[i].mr[j].material = trailMaterial;
            }
        }
    }
    private void Player_DashEnd(object sender, System.EventArgs e) {
        activateTrail = false;
    }

    private void Player_DashStart(object sender, System.EventArgs e) {
        activateTrail = true;
    }

    // Update is called once per frame
    void Update() {
        if (activateTrail && !isTrailActive) {
            isTrailActive = true;
            StartCoroutine(ActivateTrail());
        }

    }
    IEnumerator ActivateTrail() {
        for (int i = 0; i < trailObjectsAmount; i++) {
            for (int j = 0; j < skinnedMeshRenderers.Length; j++) {
                trailObjects[i].gObj[j].SetActive(true);
                trailObjects[i].gObj[j].transform.SetPositionAndRotation(Player.Instance.transform.position, Player.Instance.transform.rotation);
                skinnedMeshRenderers[j].BakeMesh(trailObjects[i].mesh[j]);
                trailObjects[i].mf[j].mesh = trailObjects[i].mesh[j];
                StartCoroutine(AnimateMaterialFloat(trailObjects[i].mr[j].material, 0, shaderFadeOutRate, shaderFadeOutRefreshRate));
                StartCoroutine(TurnObjectOff(trailObjects[i].gObj[j], meshDestroyDelay));
            }
            yield return new WaitForSeconds(meshRefreshRate);
        }
        isTrailActive = false;
    }

    IEnumerator TurnObjectOff(GameObject obj, float delay) {
        yield return new WaitForSeconds(delay);
        obj.SetActive(false);
    }

    IEnumerator AnimateMaterialFloat(Material mat, float goal, float rate, float refreshRate) {
        float valueToAnimate = 1;

        while (valueToAnimate > goal) {
            valueToAnimate -= rate;
            if (valueToAnimate < goal)
                valueToAnimate = goal;
            mat.SetFloat(shaderVarRef, valueToAnimate);
            yield return new WaitForSeconds(refreshRate);
        }
    }
}

