using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour {
    [SerializeField] private Image barImage;

    // Start is called before the first frame update
    void Start() {
        barImage.fillAmount = Player.Instance.GetHealth() / (float)Player.Instance.GetMaxHealth();
        Player.Instance.OnHit += Player_OnHit; ;
    }

    private void Player_OnHit(object sender, System.EventArgs e) {
        SetHealth();
    }

    private void SetHealth() {
        barImage.fillAmount = Player.Instance.GetHealth() / (float)Player.Instance.GetMaxHealth();
    }
}
