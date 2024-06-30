using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour {
    [SerializeField] private Image barImage;
    [SerializeField] private TextMeshProUGUI healthText;

    // Start is called before the first frame update
    void Start() {
        SetHealth();
        Player.Instance.OnHit += Player_OnHit; ;
    }

    private void Player_OnHit(object sender, System.EventArgs e) {
        SetHealth();
    }

    private void SetHealth() {
        healthText.text = Player.Instance.GetHealth() + " / " + Player.Instance.GetMaxHealth();
        barImage.fillAmount = Player.Instance.GetHealth() / (float)Player.Instance.GetMaxHealth();
    }
}
