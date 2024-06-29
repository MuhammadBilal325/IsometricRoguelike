using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBarVisual : MonoBehaviour {
    [SerializeField] private Transform interfaceObject;
    [SerializeField] private IHittable hittable;
    [SerializeField] private Image progressBar;
    void Start() {
        if (interfaceObject.TryGetComponent<IHittable>(out IHittable hit)) {
            this.hittable = hit;
        }
        else {
            hittable = null;
        }
        if (hittable != null) {
            hittable.OnHit += Hittable_OnHit;
            SetHealthBar();
        }
    }

    private void Hittable_OnHit(object sender, System.EventArgs e) {
        SetHealthBar();
    }

    private void SetHealthBar() {
        progressBar.fillAmount = (float)hittable.GetHealth() / hittable.GetMaxHealth();
    }

}
