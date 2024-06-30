using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DashUI : MonoBehaviour {
    private Player player;
    [SerializeField] private Image dashBar;
    private void Start() {
        player = Player.Instance;
    }
    private void Update() {
        dashBar.fillAmount = 1 - player.GetNormalizedDashCooldown();
    }
}
