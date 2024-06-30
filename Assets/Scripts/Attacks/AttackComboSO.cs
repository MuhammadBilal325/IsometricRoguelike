using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class AttackComboSO : ScriptableObject {
    public List<AttackSO> attacks;
    public float resetTime;
    public Sprite ComboSprite;
}
