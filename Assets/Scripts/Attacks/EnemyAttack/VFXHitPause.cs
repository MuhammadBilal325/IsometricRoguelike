using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class VFXHitPause : MonoBehaviour {
    private VisualEffect visualEffect;
    void Start() {
        visualEffect = GetComponent<VisualEffect>();

        Player.Instance.HitPauseStart += Player_HitPauseStart;
        Player.Instance.HitPauseEnd += Player_HitPauseEnd;
    }

    private void Player_HitPauseEnd(object sender, System.EventArgs e) {
        if (visualEffect != null) {
            visualEffect.playRate = 1;
        }
    }

    private void Player_HitPauseStart(object sender, System.EventArgs e) {
        if (visualEffect != null) {
            visualEffect.playRate = 0;
        }
    }
}
